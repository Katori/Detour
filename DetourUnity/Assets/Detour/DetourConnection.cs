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
using System.Net.Sockets;

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

        public async void Connect(System.Uri uri)
        {
            var clientFactory = new WebSocketClientFactory();

            cancellation = new CancellationTokenSource();

            Connecting = true;

            Uri = uri;
            try
            {
                using (WebSocket = await clientFactory.ConnectAsync(Uri, ClientOptions, cancellation.Token))
                {
                    Connecting = false;
                    ConnectionActive = true;
                    Connected?.Invoke();

                    await Receive(WebSocket, cancellation.Token);
                }
            }
            catch (ObjectDisposedException)
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

        public void SendMessage(DetourMessage Message)
        {
            EnqueuedMessagesToSend.Add(Message);
        }

        public void RegisterHandler(int MessageType, Type MessageClass, MessageEventHandler Handler)
        {
            MessageTypeToMessageDefinition.Add(MessageType, new MessageDefinition { Type = MessageClass, EventHandler = Handler });
        }

        public void Send()
        {
            if (WebSocket == null)
            {
                ReceivedError?.Invoke(new SocketException((int)SocketError.NotConnected));
                return;
            }

            if (EnqueuedMessagesToSend.Count > 0)
            {
                SendEnqueuedMessages(EnqueuedMessagesToSend);
                EnqueuedMessagesToSend = new List<DetourMessage>();
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

                try
                {
                    var msg = ProcessJsonToDetourMessage(data, data.Length);
                    if (msg != null)
                    {
                        ReceivedMessage(msg);
                    }
                }
                catch(Newtonsoft.Json.JsonReaderException ex)
                {
                    Debug.LogError(ex);
                }
                catch(System.Exception ex)
                {
                    Debug.LogError(ex);
                }
            }
        }

        private void ReceivedMessage(DetourMessage msg)
        {
            if (MessageTypeToMessageDefinition.ContainsKey(msg.MessageType))
            {
                MessageTypeToMessageDefinition[msg.MessageType].EventHandler(msg);
            }
        }

        private async void SendEnqueuedMessages(List<DetourMessage> enqueuedMessagesToSend)
        {
            foreach (var item in enqueuedMessagesToSend)
            {
                if (ConnectionActive)
                {
                    try
                    {
                        JSONBuffer = JsonConvert.SerializeObject(item, JSONSettings);
                        MessageBuffer = Encoding.UTF8.GetBytes(JSONBuffer);
                        await WebSocket.SendAsync(new ArraySegment<byte>(MessageBuffer, 0, MessageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        Disconnect();
                        ReceivedError?.Invoke(ex);
                    }
                }
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
    }
}