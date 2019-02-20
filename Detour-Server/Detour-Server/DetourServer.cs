using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetourServer
{
    public static class DetourServer
    {
        public static Dictionary<string, DetourClientContainer> Clients = new Dictionary<string, DetourClientContainer>();

        public static void AddClient(DetourClientContainer Client)
        {
            Clients.Add(Client.Id, Client);
        }

        public static void SendMessage(string Address, byte[] Message)
        {
            Clients[Address].EnqueuedMessagesToSend.Add(Message);
        }
    }
}
