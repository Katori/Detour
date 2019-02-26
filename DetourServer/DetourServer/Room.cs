using System;
using System.Collections.Generic;
using System.Linq;

namespace DetourServer
{
    public class Room
    {
        public string RoomId;
        public string RoomType;
        public int RoomClientCapacity;
        public bool PrivateRoom = false;
        public int RoomClientCount { get { return RoomClients.Count; } }

        public Dictionary<string, ClientContainer> RoomClients = new Dictionary<string, ClientContainer>();

        public void SendToAll(DetourMessage message)
        {
            foreach (var item in RoomClients.Values)
            {
                item.EnqueuedMessagesToSend.Add(message);
            }
        }

        public bool AddToRoom(ClientContainer Client)
        {
            if(RoomClientCount < RoomClientCapacity)
            {
                RoomClients.Add(Client.Id, Client);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SendToAllExcept(List<string> addressesToExclude, DetourMessage message)
        {
            var p = RoomClients.Where(x => addressesToExclude.Contains(x.Key) != true).ToList();
            foreach (var item in p)
            {
                item.Value.EnqueuedMessagesToSend.Add(message);
            }
        }
    }
}