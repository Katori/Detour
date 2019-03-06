using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Ninja.WebSockets;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DetourServer.Standalone
{
    public class ListenService : BackgroundService
    {
        private IWebSocketServerFactory _webSocketServerFactory = new WebSocketServerFactory();

        IConfiguration configuration;
        Config config;

        class Config
        {
            public int Port { get; set; }
            public bool Secure { get; set; }
            public string CertificatePassword { get; set; }
        }

        public ListenService(IConfiguration conf)
        {
            this.configuration = conf;
            var section = configuration.GetSection("Detour:ws");
            config = new Config();
            section.Bind(config);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IList<string> supportedSubProtocols = new string[] { "Detour" };
            using (WebServer server = new WebServer(_webSocketServerFactory, supportedSubProtocols, "/game"))
            {
                server.Secure = config.Secure;
                if (server.Secure)
                {
                    server._sslConfig = new WebServer.SslConfiguration
                    {
                        Certificate = new System.Security.Cryptography.X509Certificates.X509Certificate2(System.AppContext.BaseDirectory + "/certificate.pfx", config.CertificatePassword),
                        CheckCertificateRevocation = false,
                        ClientCertificateRequired = false,
                        EnabledSslProtocols = System.Security.Authentication.SslProtocols.None
                    };
                }
                await server.Listen(config.Port);
            }
        }
    }
}
