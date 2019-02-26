using System;
using System.Collections.Generic;

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
    }
}