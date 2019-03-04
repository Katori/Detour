using Microsoft.Extensions.Hosting;
using Ninja.WebSockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetourServer.Standalone
{
    public class ListenService : BackgroundService
    {
        private IWebSocketServerFactory _webSocketServerFactory = new WebSocketServerFactory();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IList<string> supportedSubProtocols = new string[] { "Detour" };
            using (WebServer server = new WebServer(_webSocketServerFactory, supportedSubProtocols, "/game"))
            {
                await server.Listen(27416);
            }
        }
    }
}
