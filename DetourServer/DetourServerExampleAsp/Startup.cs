using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DetourServer;
using DetourServer.Asp;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;

namespace DetourServerExampleAsp
{
    public class Startup
    {
        static System.Random RanGen = new System.Random();

        static List<Vector2Int> SpawnPositions = new List<Vector2Int>
        {
            new Vector2Int(3, 3),
            new Vector2Int(6, 6),
            new Vector2Int(1, 5),
            new Vector2Int(9, 9)
        };

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSingleton<IHostedService, AspSendService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, Microsoft.AspNetCore.Hosting.IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseWebSockets();

            Server.RegisterHandler((int)MessageTypes.ClientSentTestMessage, typeof(ClientSentTestMessage), OnClientSentTestMessage);
            Server.RegisterHandler((int)MessageTypes.PlayerMoveMessage, typeof(PlayerMoveMessage), PlayerMoved);
            Server.UseRoomHandling(typeof(ClientRequestingRoomJoin), new RoomDefinition { RoomType = "Default", RoomCapacity = 100, StartPoints = 4, OnRoomJoined = OnClientJoinedRoom, OnRoomInitialized = OnRoomInit });
            Server.ClientRemoved += Server_ClientRemoved;
            Server.ApplicationVersion = 0.1f;
            Server.DetourVersion = 0.2f;

            app.UseDetourServer();

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private static void PlayerMoved(string Address, DetourMessage netMsg)
        {
            var c = Server.AllClients[Address].StoredData["Player"] as PlayerDefinition;
            var b = netMsg as PlayerMoveMessage;
            var _dist = Vector2Int.Distance(c.Position, b.PositionToOperateOn);
            if (_dist < 4)
            {
                c.Position = b.PositionToOperateOn;
                if (!c.HasMoved)
                {
                    c.HasMoved = true;
                }
                Server.SendMessage(Address, new PlayerMoveCommand
                {
                    PlayerId = "0",
                    PositionToMoveTo = c.Position,
                    ApplicationVersion = Server.ApplicationVersion,
                    DetourVersion = Server.DetourVersion,
                    MessageType = (int)MessageTypes.PlayerMoveCommand
                });
                Server.SendToRoomExcept(Server.AllClients[Address].StoredData["Room"] as string, new List<string> { Address }, new PlayerMoveCommand
                {
                    ApplicationVersion = Server.ApplicationVersion,
                    DetourVersion = Server.DetourVersion,
                    MessageType = (int)MessageTypes.PlayerMoveCommand,
                    PlayerId = Address,
                    PositionToMoveTo = c.Position
                });
            }
        }

        private static void OnRoomInit(string RoomId, string RoomType)
        {
            Vector2Int MapSize = new Vector2Int(25, 25);
            TileData[,] Tiles = new TileData[MapSize.x, MapSize.y];
            Server.Rooms[RoomId].StoreRoomData("MapSize", MapSize);
            var TileSize = MapSize.x * MapSize.y;
            for (int i = 0; i < MapSize.x; i++)
            {
                for (int u = 0; u < MapSize.y; u++)
                {
                    int terrainType = RanGen.Next(1, 4);
                    if (i == 0 || i == MapSize.x - 1 || u == 0 || u == MapSize.y - 1)
                    {
                        terrainType = 0;
                    }
                    bool forest = (RanGen.NextDouble() > 0.7f && terrainType != 0);
                    Tiles[i, u] = new TileData(terrainType, forest);
                }
            }
            Server.Rooms[RoomId].StoreRoomData("TileData", Tiles);
        }

        private static void Server_ClientRemoved(string obj)
        {

        }

        private static void OnClientJoinedRoom(string Address, string RoomId, DetourMessage RoomMessage)
        {
            var p = RoomMessage as ClientRequestingRoomJoin;
            var _startPos = RanGen.Next(0, Server.Rooms[RoomId].RoomStartPoints);
            var Pl = new PlayerDefinition
            {
                Id = Address,
                Name = p.Name,
                Position = SpawnPositions[_startPos],
                HasMoved = false,
            };
            System.Console.WriteLine("spawn position: " + Pl.Position.x + " " + Pl.Position.y);

            Server.StoreClientData(Address, "Player", Pl);
            Server.StoreClientData(Address, "Room", RoomId);
            Server.SendToRoomExcept(RoomId, new List<string>(new string[] { Address }), new ClientJoinedRoomMessage() { ApplicationVersion = Server.ApplicationVersion, DetourVersion = Server.DetourVersion, RoomId = RoomId, MessageType = (int)MessageTypes.ClientJoinedRoomMessage, Player = Pl });
            var PlayerList = new List<PlayerDefinition>();
            foreach (var item in Server.Rooms[RoomId].RoomClients.Values)
            {
                if (item.Id != Address)
                {
                    PlayerList.Add(item.StoredData["Player"] as PlayerDefinition);
                }
            }
            Server.SendMessage(Address, new ClientRoomDataCatchUp
            {
                DetourVersion = Server.DetourVersion,
                ApplicationVersion = Server.ApplicationVersion,
                RoomId = RoomId,
                MessageType = (int)MessageTypes.ClientRoomDataCatchUp,
                Players = PlayerList,
                ClientStartPosition = SpawnPositions[_startPos],
                MapSize = Server.Rooms[RoomId].RoomData["MapSize"] as Vector2Int,
                MapTiles = Server.Rooms[RoomId].RoomData["TileData"] as TileData[,]
            });
        }

        private static void OnClientSentTestMessage(string Address, DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            System.Console.WriteLine("Received TestString: " + testMsg.TestString);
        }
    }
}
