using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetourServer.Standalone
{
    public class SendService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (true)
            {
                if (Server.AllClients.Count > 0)
                {
                    foreach (var item in Server.AllClients)
                    {
                        if (item.Value.EnqueuedMessagesToSend.Count > 0)
                        {
                            foreach (var Message in item.Value.EnqueuedMessagesToSend)
                            {
                                var JSONBuffer = JsonConvert.SerializeObject(Message);
                                var MessageBuffer = Encoding.UTF8.GetBytes(JSONBuffer);
                                await item.Value.Socket.SendAsync(new System.ArraySegment<byte>(MessageBuffer, 0, MessageBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                                item.Value.EnqueuedMessagesToSend.Remove(item);
                            }
                        }
                    }
                }
            }
        }
    }
}
