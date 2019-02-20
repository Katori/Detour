using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DetourServer
{
    public class ClientContainer
    {
        public string Id;

        public HttpContext Context;

        public WebSocket Socket;

        public List<dynamic> EnqueuedMessagesToSend = new List<dynamic>();

        private string JSONBuffer;
        private byte[] MessageBuffer;

        public ClientContainer(HttpContext _Context, WebSocket _Socket)
        {
            Id = _Context.Connection.Id;
            Context = _Context;
            Socket = _Socket;
        }

        public async Task ClientProcess()
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                if (EnqueuedMessagesToSend.Count > 0)
                {
                    foreach (var item in EnqueuedMessagesToSend)
                    {
                        JSONBuffer = JsonConvert.SerializeObject(item);
                        MessageBuffer = Encoding.UTF8.GetBytes(JSONBuffer);
                        await Socket.SendAsync(new ArraySegment<byte>(MessageBuffer, 0, MessageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }

                result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                JSONBuffer = Encoding.UTF8.GetString(buffer, 0, result.Count);
                JObject jObject = JObject.Parse(JSONBuffer);
                if (jObject.HasValues && jObject.ContainsKey("MessageType"))
                {
                        Server.ReceivedMessage(Id, (DetourMessage)jObject.ToObject(Type.GetType(Server.MessageCodeToType[jObject.Value<int>("MessageType")].FullName)));
                }
            }
            await Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
