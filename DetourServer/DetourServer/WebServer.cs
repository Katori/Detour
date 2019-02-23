using Ninja.WebSockets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
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

        public WebServer(IWebSocketServerFactory webSocketServerFactory, IList<string> supportedSubProtocols = null)
        {
            _webSocketServerFactory = webSocketServerFactory;
            _supportedSubProtocols = new HashSet<string>(supportedSubProtocols ?? new string[0]);
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

            //try
            //{
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
                WebSocketHttpContext context = await _webSocketServerFactory.ReadHttpHeaderFromStreamAsync(stream);
                if (context.IsWebSocketRequest)
                {
                    string subProtocol = GetSubProtocol(context.WebSocketRequestedProtocols);
                    var options = new WebSocketServerOptions() { KeepAliveInterval = TimeSpan.FromSeconds(30), SubProtocol = subProtocol };

                    WebSocket webSocket = await _webSocketServerFactory.AcceptWebSocketAsync(context, options);

                    var newClient = Server.AddClient(new ClientContainer(System.Guid.NewGuid().ToString(), webSocket));
                    await newClient.ClientProcess();
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

        public async Task RespondToWebSocketRequestAsync(WebSocket webSocket, CancellationToken token)
        {
            const int bufferLen = 4 * 1024 * 1024; // 4MB
            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[bufferLen]);

            while (true)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(buffer, token);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }

                if (result.Count > bufferLen)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig,
                        $"Web socket frame cannot exceed buffer size of {bufferLen:#,##0} bytes. Send multiple frames instead.",
                        token);
                    break;
                }

                // just echo the message back to the client
                ArraySegment<byte> toSend = new ArraySegment<byte>(buffer.Array, buffer.Offset, result.Count);
                await webSocket.SendAsync(toSend, WebSocketMessageType.Binary, true, token);
            }
        }

        public async Task Listen(int port)
        {
            try
            {
                IPAddress localAddress = IPAddress.Any;
                _listener = new TcpListener(localAddress, port);
                _listener.Start();
                while (true)
                {
                    TcpClient tcpClient = await _listener.AcceptTcpClientAsync();
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