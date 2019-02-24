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

        public static async Task StartDetourServer()
        {
            await StartWebServer();
        }

        static async Task StartWebServer()
        {
            try
            {
                int port = 27416;
                using (WebServer server = new WebServer(_webSocketServerFactory))
                {
                    await server.Listen(port);
                    Console.WriteLine($"Listening on port {port}");
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
