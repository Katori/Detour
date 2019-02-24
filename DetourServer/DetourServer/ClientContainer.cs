using System.Net.WebSockets;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DetourServer
{
    public class ClientContainer
    {
        public string Id;

        public WebSocket Socket;

        public List<dynamic> EnqueuedMessagesToSend = new List<dynamic>();

        private string JSONBuffer;
        private byte[] MessageBuffer;

        JsonSerializerSettings jsonSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.Objects
        };

        public ClientContainer(string _Id, WebSocket _Socket)
        {
            Id = _Id;
            Socket = _Socket;
        }

        public async Task ClientProcess()
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await Socket.ReceiveAsync(new System.ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                if (EnqueuedMessagesToSend.Count > 0)
                {
                    foreach (var item in EnqueuedMessagesToSend)
                    {
                        JSONBuffer = JsonConvert.SerializeObject(item, jsonSettings);
                        MessageBuffer = Encoding.UTF8.GetBytes(JSONBuffer);
                        await Socket.SendAsync(new System.ArraySegment<byte>(MessageBuffer, 0, MessageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }

                result = await Socket.ReceiveAsync(new System.ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.Count > 0)
                {
                    var msg = ProcessJsonIntoDetourMessage(buffer, result.Count);
                    if (msg != null)
                    {
                        Server.ReceivedMessage(Id, msg);
                    }
                }
            }
            await Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        public DetourMessage ProcessJsonIntoDetourMessage(byte[] JsonBuffer, int ByteLength)
        {
            JSONBuffer = Encoding.UTF8.GetString(JsonBuffer, 0, ByteLength);
            JObject jObject = JObject.Parse(JSONBuffer);
            if (jObject.HasValues && jObject.ContainsKey("MessageType"))
            {
                return (DetourMessage)jObject.ToObject(TypeExtensions.FindType(Server.MessageTypeToMessageDefinition[jObject.Value<int>("MessageType")].Type.FullName));
            }
            else
            {
                return null;
            }
        }
    }
}
