using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DetourServer
{
    public class DetourClientContainer
    {
        public string Id;

        public HttpContext Context;

        public WebSocket Socket;

        public List<byte[]> EnqueuedMessagesToSend = new List<byte[]>();

        public DetourClientContainer(HttpContext _Context, WebSocket _Socket)
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
                        await Socket.SendAsync(new ArraySegment<byte>(item, 0, item.Length), WebSocketMessageType.Binary, result.EndOfMessage, CancellationToken.None);
                    }
                }

                result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
