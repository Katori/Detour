﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DetourServer;
using DetourServer.Asp;

namespace DetourServerExampleAsp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
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

            Server.RegisterHandler(1, typeof(ClientSentTestMessage), OnClientSentTestMessage);
            Server.UseRoomHandling(typeof(ClientRequestingRoomJoin), new RoomDefinition{RoomType = "Default", RoomCapacity = 2, OnRoomJoined = ClientJoinedRoom });

            app.UseDetourServer();

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private void ClientJoinedRoom(string Address, string RoomId, DetourMessage ExtraJoinData)
        {
            throw new NotImplementedException();
        }

        private static void OnClientSentTestMessage(string Address, DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            System.Console.WriteLine("Received TestString: " + testMsg.TestString);
        }

        [System.Serializable]
        public class ClientSentTestMessage : DetourMessage
        {
            public string TestString;
        }
    }
}
