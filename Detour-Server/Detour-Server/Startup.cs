using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DetourServer
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

            Server.ServerReceivedMessage += DetourServer_ServerReceivedData;
            Server.RegisterHandler(1, typeof(ClientSentTestMessage), OnClientSentTestMessage);

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/ws")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var newClient = new ClientContainer(context, webSocket);
                        Server.AddClient(newClient);
                        await newClient.ClientProcess();
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }
            });
            app.UseHttpsRedirection();
            app.UseMvc();

            
        }

        private void DetourServer_ServerReceivedData(string SenderAddr, dynamic ReceivedData)
        {
            // is this necessary? handlers can do all of this, we might not need this direct pipe
        }

        private void OnClientSentTestMessage(DetourMessage msg)
        {
            var testMsg = msg as ClientSentTestMessage;
            Console.WriteLine("received some stuff: " + testMsg.TestString);
        }

        [System.Serializable]
        public class ClientSentTestMessage : DetourMessage
        {
            public string TestString;
        }
    }

    
}
