﻿using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetourServer.Asp
{
    public class AspSendService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (Server.MessagesToSend.Count > 0)
                {
                    SendableMessage sendMsg;
                    var msgToSend = Server.MessagesToSend.TryDequeue(out sendMsg);
                    if (msgToSend)
                    {
                        var JSONBufffer = JsonConvert.SerializeObject(sendMsg.Message);
                        var MessageBuffer = Encoding.UTF8.GetBytes(JSONBufffer);
                        if (Server.AllClients.ContainsKey(sendMsg.Address))
                        {
                            await Server.AllClients[sendMsg.Address].Socket.SendAsync(new System.ArraySegment<byte>(MessageBuffer, 0, MessageBuffer.Length), WebSocketMessageType.Text, true, stoppingToken);
                        }
                    }
                }
                else
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
