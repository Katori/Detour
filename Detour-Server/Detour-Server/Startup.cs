using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DetourServer.Middleware;

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

            app.UseDetourServer();
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
            System.Console.WriteLine("received some stuff: " + testMsg.TestString);
        }

        [System.Serializable]
        public class ClientSentTestMessage : DetourMessage
        {
            public string TestString;
        }
    }

    
}
