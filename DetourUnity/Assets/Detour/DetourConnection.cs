using Ninja.WebSockets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;
using UnityEngine;

namespace DetourClient
{
    public class DetourConnection
    {
        public event Action Connected;
        public event Action Disconnected;
        public event Action<Exception> ReceivedError;

        public bool Connecting { get; private set; }
        public bool ConnectionActive { get; private set; }

        public Dictionary<int, MessageDefinition> MessageTypeToMessageDefinition = new Dictionary<int, MessageDefinition>();

        public List<DetourMessage> EnqueuedMessagesToSend = new List<DetourMessage>();

        private WebSocket WebSocket;

        private System.Uri Uri;

        private string JSONBuffer;
        private byte[] MessageBuffer;

        private CancellationTokenSource cancellation;
        
        private JsonSerializerSettings JSONSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        private WebSocketClientOptions ClientOptions = new WebSocketClientOptions
        {
            NoDelay = true
        };

        public async Task Connect(System.Uri uri)
        {
            var clientFactory = new WebSocketClientFactory();

            cancellation = new CancellationTokenSource();

            Uri = uri;
            try
            {
                using (WebSocket = await clientFactory.ConnectAsync(Uri, ClientOptions, cancellation.Token))
                {
                    Connecting = false;
                    ConnectionActive = true;
                    Connected.Invoke();

                    await Receive(WebSocket, cancellation.Token);
                }
            }
            catch (ObjectDisposedException)
            {
                // client closed
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex);
                throw;
            }
            finally
            {
                Disconnect();
            }
        }

        private void Disconnect()
        {
            cancellation.Cancel();
            if (WebSocket != null)
            {
                WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                WebSocket = null;
                Connecting = false;
                ConnectionActive = false;
                Disconnected.Invoke();
            }
        }

        private async Task Receive(WebSocket webSocket, CancellationToken token)
        {
            var buffer = new byte[int.MaxValue];

            while (true)
            {
                if(EnqueuedMessagesToSend.Count > 0)
                {
                    await SendEnqueuedMessages(EnqueuedMessagesToSend);
                }

                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), token);

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

                var msg = ProcessJsonToDetourMessage(data, data.Length);
            }
        }

        private async Task SendEnqueuedMessages(List<DetourMessage> enqueuedMessagesToSend)
        {
            foreach (var item in enqueuedMessagesToSend)
            {
                JSONBuffer = JsonConvert.SerializeObject(item, JSONSettings);
                MessageBuffer = Encoding.UTF8.GetBytes(JSONBuffer);
                await WebSocket.SendAsync(new ArraySegment<byte>(MessageBuffer, 0, MessageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        private DetourMessage ProcessJsonToDetourMessage(byte[] JsonBuffer, int ByteLength)
        {
            JSONBuffer = Encoding.UTF8.GetString(JsonBuffer, 0, ByteLength);
            JObject jObject = JObject.Parse(JSONBuffer);
            if (jObject.HasValues && jObject.Property("MessageType")!=null)
            {
                return (DetourMessage)jObject.ToObject(TypeExtensions.FindType(MessageTypeToMessageDefinition[jObject.Value<int>("MessageType")].Type.FullName));
            }
            else
            {
                return null;
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

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer, count, int.MaxValue - count), CancellationToken.None);
                count += result.Count;
            }
            return new ArraySegment<byte>(buffer, 0, count).ToArray();
        }

        public void RegisterHandler(int MessageType, Type MessageClass, MessageEventHandler Handler)
        {
            MessageTypeToMessageDefinition.Add(MessageType, new MessageDefinition { Type = MessageClass, EventHandler = Handler });
        }
    }
}