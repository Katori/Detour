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
            while (!stoppingToken.IsCancellationRequested)
            {
                if(Server.MessagesToSend.Count > 0)
                {
                    SendableMessage sendMsg;
                    var msgToSend = Server.MessagesToSend.TryDequeue(out sendMsg);
                    if (msgToSend)
                    {
                        var JSONBufffer = JsonConvert.SerializeObject(sendMsg.Message);
                        var MessageBuffer = Encoding.UTF8.GetBytes(JSONBufffer);
                        await Server.AllClients[sendMsg.Address].Socket.SendAsync(new System.ArraySegment<byte>(MessageBuffer, 0, MessageBuffer.Length), WebSocketMessageType.Text, true, stoppingToken);
                    }
                }
            }
        }
    }
}
