using Ninja.WebSockets;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DetourServer
{
    public static class StandaloneServer
    {
        private static WebSocketServerFactory _webSocketServerFactory = new WebSocketServerFactory();

        private static string _GamePath;

        private static int _Port;

        public static async Task StartDetourServer(string GamePath = "/game", int Port = 27416)
        {
            _GamePath = GamePath;
            _Port = Port;
            await StartWebServer();
        }

        static async Task StartWebServer()
        {
            try
            {
                IList<string> supportedSubProtocols = new string[] { "chat" };
                using (WebServer server = new WebServer(_webSocketServerFactory, supportedSubProtocols, _GamePath))
                {
                    await server.Listen(_Port);
                    Console.WriteLine($"Listening on port {_Port}");
                    Console.WriteLine("Press any key to quit");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Console.ReadKey();
            }
        }
    }
}
