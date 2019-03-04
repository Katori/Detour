#if !UNITY_WEBGL || UNITY_EDITOR

using Ninja.WebSockets;
using System.Linq;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DetourClient
{
    class DetourClient
    {
        public event System.Action Connected;
        public event System.Action<byte[]> ReceivedData;
        public event System.Action Disconnected;
        public event System.Action<System.Exception> ReceivedError;

        private CancellationTokenSource cancellation;
        public WebSocket WebSocket;

        public bool Connecting { get; set; }

        public bool ConnectionActive;

        private WebSocketClientOptions ClientOptions = new WebSocketClientOptions
        {
            NoDelay = true
        };

        public async void Connect(System.Uri uri)
        {
            var clientFactory = new WebSocketClientFactory();

            cancellation = new CancellationTokenSource();

            Connecting = true;

            try
            {
                using (WebSocket = await clientFactory.ConnectAsync(uri, ClientOptions, cancellation.Token))
                {
                    Connecting = false;
                    ConnectionActive = true;
                    Connected?.Invoke();

                    await Receive(WebSocket, cancellation.Token);
                }
            }
            catch (System.ObjectDisposedException)
            {
                // client closed
                Debug.Log("closed connection");
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                ReceivedError?.Invoke(ex);
            }
            finally
            {
                Disconnect();
            }
        }

        public void Disconnect()
        {
            cancellation?.Cancel();
            if (WebSocket != null)
            {
                WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                WebSocket = null;
                Connecting = false;
                ConnectionActive = false;
                Disconnected?.Invoke();
            }
        }

        private async Task Receive(WebSocket webSocket, CancellationToken token)
        {
            var buffer = new byte[int.MaxValue];

            while (true)
            {

                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new System.ArraySegment<byte>(buffer), token);

                if (result == null)
                {
                    break;
                }
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                byte[] data = await ReadFrames(result, webSocket, buffer);

                if (data == null)
                {
                    break;
                }

                try
                {
                    ReceivedData.Invoke(data);
                    //var msg = ProcessJsonToDetourMessage(data, data.Length);
                    //if (msg != null)
                    //{
                    //    ReceivedMessage(msg);
                    //}
                }
                catch (Newtonsoft.Json.JsonReaderException ex)
                {
                    Debug.LogError(ex);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
        }

        public async void Send(byte[] data)
        {
            if (WebSocket == null)
            {
                ReceivedError?.Invoke(new SocketException((int)SocketError.NotConnected));
                return;
            }

            try
            {
                await WebSocket.SendAsync(new System.ArraySegment<byte>(data), WebSocketMessageType.Text, true, cancellation.Token);
            }
            catch (System.Exception ex)
            {
                Disconnect();
                ReceivedError?.Invoke(ex);
            }
        }

        private async Task<byte[]> ReadFrames(WebSocketReceiveResult result, WebSocket webSocket, byte[] buffer)
        {
            int count = result.Count;

            while (!result.EndOfMessage)
            {
                if (count >= int.MaxValue)
                {
                    string closeMessage = string.Format("Maximum message size: {0} bytes.", int.MaxValue);
                    await webSocket.CloseAsync(WebSocketCloseStatus.MessageTooBig, closeMessage, CancellationToken.None);
                    return null;
                }

                result = await webSocket.ReceiveAsync(new System.ArraySegment<byte>(buffer, count, int.MaxValue - count), CancellationToken.None);
                count += result.Count;
            }
            return new System.ArraySegment<byte>(buffer, 0, count).ToArray();
        }
    }
}
#endif