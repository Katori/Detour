﻿using Ninja.WebSockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace DetourServer
{
    internal class WebServer : IDisposable
    {
        private TcpListener _listener;
        private bool _isDisposed = false;
        private readonly IWebSocketServerFactory _webSocketServerFactory;
        private readonly HashSet<string> _supportedSubProtocols;

        private string _GamePath;
        public bool Secure;
        public SslConfiguration _sslConfig;

        public class SslConfiguration
        {
            public System.Security.Cryptography.X509Certificates.X509Certificate2 Certificate;
            public bool ClientCertificateRequired;
            public System.Security.Authentication.SslProtocols EnabledSslProtocols;
            public bool CheckCertificateRevocation;
        }

        public WebServer(IWebSocketServerFactory webSocketServerFactory, IList<string> supportedSubProtocols = null, string GamePath = "/game")
        {
            _webSocketServerFactory = webSocketServerFactory;
            _supportedSubProtocols = new HashSet<string>(supportedSubProtocols ?? new string[0]);
            _GamePath = GamePath;
        }

        private void ProcessTcpClient(TcpClient tcpClient)
        {
            Task.Run(() => ProcessTcpClientAsync(tcpClient));
        }

        private string GetSubProtocol(IList<string> requestedSubProtocols)
        {
            foreach (string subProtocol in requestedSubProtocols)
            {
                // match the first sub protocol that we support (the client should pass the most preferable sub protocols first)
                if (_supportedSubProtocols.Contains(subProtocol))
                {

                    return subProtocol;
                }
            }

            if (requestedSubProtocols.Count > 0)
            {
            }

            return null;
        }

        private async Task ProcessTcpClientAsync(TcpClient tcpClient)
        {
            CancellationTokenSource source = new CancellationTokenSource();

            if (_isDisposed)
            {
                return;
            }

            // this worker thread stays alive until either of the following happens:
            // Client sends a close conection request OR
            // An unhandled exception is thrown OR
            // The server is disposed

            // get a secure or insecure stream
            Stream stream = tcpClient.GetStream();
            if (Secure)
            {
                var sslStream = new SslStream(stream, false, CertVerificationCallback);
                await sslStream.AuthenticateAsServerAsync(_sslConfig.Certificate, _sslConfig.ClientCertificateRequired, _sslConfig.EnabledSslProtocols, _sslConfig.CheckCertificateRevocation);
                stream = sslStream;
            }
            WebSocketHttpContext context = await _webSocketServerFactory.ReadHttpHeaderFromStreamAsync(stream);
            if (context.IsWebSocketRequest)
            {
                if (context.Path == _GamePath)
                {

                    string subProtocol = GetSubProtocol(context.WebSocketRequestedProtocols);
                    var options = new WebSocketServerOptions() { KeepAliveInterval = TimeSpan.FromSeconds(30), SubProtocol = subProtocol };

                    WebSocket webSocket = await _webSocketServerFactory.AcceptWebSocketAsync(context, options);

                    var newClient = Server.AddClient(new ClientContainer(System.Guid.NewGuid().ToString(), webSocket));
                    await newClient.ClientProcess();
                }
                else
                {
                    Console.WriteLine("path not match");
                }

            }
            else
            {
            }

            //}
            //catch (ObjectDisposedException)
            //{
            //    // do nothing. This will be thrown if the Listener has been stopped
            //}
            //finally
            //{
            //    try
            //    {
            //        tcpClient.Client.Close();
            //        tcpClient.Close();
            //        source.Cancel();
            //    }
            //    catch (Exception ex)
            //    {
            //    }
            //}
        }

        private bool CertVerificationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        public async Task Listen(int port)
        {
            try
            {
                _listener = TcpListener.Create(port);
                _listener.Start();
                Console.WriteLine($"Server started listening on port {port}");
                while (true)
                {
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
                    Console.WriteLine("found client");
                    ProcessTcpClient(tcpClient);
                }
            }
            catch (SocketException ex)
            {
                string message = string.Format("Error listening on port {0}. Make sure IIS or another application is not running and consuming your port.", port);
                throw new Exception(message, ex);
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;

                // safely attempt to shut down the listener
                try
                {
                    if (_listener != null)
                    {
                        if (_listener.Server != null)
                        {
                            _listener.Server.Close();
                        }

                        _listener.Stop();
                    }
                }
                catch (Exception ex)
                {
                }

            }
        }
    }
}