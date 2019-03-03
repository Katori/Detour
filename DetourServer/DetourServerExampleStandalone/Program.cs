using System.Collections.Generic;
using System.Threading.Tasks;
using DetourServer;

namespace DetourServerExample
{
    class Program
    {
        static void Main(string[] args)
        {
            Server.RegisterHandler((int)MessageTypes.ClientSentTestMessage, typeof(ClientSentTestMessage), OnClientSentTestMessage);
            Server.UseRoomHandling(typeof(ClientRequestingRoomJoin), new RoomDefinition {RoomType = "Default", RoomCapacity = 100, OnRoomJoined = OnClientJoinedRoom });
            Server.ApplicationVersion = 0.1f;
            Server.DetourVersion = 0.2f;
            Task task = StandaloneServer.StartDetourServer();
            task.Wait();
        }

        private static void OnClientJoinedRoom(string Address, string RoomId, DetourMessage RoomMessage)
        {
            System.Console.WriteLine("made it to roomjoined ");
            var p = RoomMessage as ClientRequestingRoomJoin;
            var Pl = new PlayerDefinition
            {
                Id = Address,
                Name = p.Name,
                Position = new Vector2(1, 1)
            };
            Server.StoreClientData(Address, "Player", Pl);
            Server.SendToRoomExcept(RoomId, new List<string>(new string[] { Address }), new ClientJoinedRoomMessage() {ApplicationVersion = Server.ApplicationVersion, DetourVersion = Server.DetourVersion, RoomId = RoomId, MessageType = (int)MessageTypes.ClientJoinedRoomMessage,  Player = Pl });
            var PlayerList = new List<PlayerDefinition>();
            foreach (var item in Server.Rooms[RoomId].RoomClients.Values)
            {
                if (item.Id != Address)
                {
                    PlayerList.Add(item.StoredData["Player"] as PlayerDefinition);
                }
            }
            Server.SendMessage(Address, new ClientRoomDataCatchUp { DetourVersion = Server.DetourVersion, ApplicationVersion = Server.ApplicationVersion, RoomId = RoomId, MessageType = (int)MessageTypes.ClientRoomDataCatchUp, Players = PlayerList });
            System.Console.WriteLine("sent roomdatacatchup message");
        }

        private static void OnClientSentTestMessage(string Address, DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            System.Console.WriteLine("Received TestString: " + testMsg.TestString);
        }
    }
}
