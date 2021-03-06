﻿using System.Collections.Generic;

namespace DetourServer
{
    public class Room
    {
        public string RoomId;
        public string RoomType;
        public int RoomClientCapacity;
        public bool PrivateRoom = false;
        public int RoomClientCount { get { return RoomClients.Count; } }
        public int RoomStartPoints;

        public Dictionary<string, ClientContainer> RoomClients = new Dictionary<string, ClientContainer>();

        public Dictionary<string, dynamic> RoomData = new Dictionary<string, dynamic>();

        public void SendToAll(DetourMessage message)
        {
            foreach (var item in RoomClients.Values)
            {
                Server.MessagesToSend.Enqueue(new SendableMessage
                {
                    Address = item.Id,
                    Message = message
                });
            }
        }

        public bool AddToRoom(ClientContainer Client)
        {
            if(RoomClientCount < RoomClientCapacity)
            {
                if (RoomClients.ContainsKey(Client.Id))
                {
                    RoomClients[Client.Id] = Client;
                    return true;
                }
                else
                {
                    RoomClients.Add(Client.Id, Client);
                    return true;
                }
            }
            else
            {
                return false;
            }
        }

        public void StoreRoomData(string Key, dynamic Item)
        {
            RoomData.Add(Key, Item);
        }

        public void SendToAllExcept(List<string> addressesToExclude, DetourMessage message)
        {
            if (addressesToExclude.Count == 1)
            {
                foreach (var item in RoomClients)
                {
                    if (item.Key != addressesToExclude[0])
                    {
                        Server.MessagesToSend.Enqueue(new SendableMessage
                        {
                            Address = item.Key,
                            Message = message
                        });
                    }
                }
            }
            else
            {
                foreach (var item in RoomClients)
                {
                    if (!addressesToExclude.Contains(item.Key))
                    {
                        Server.MessagesToSend.Enqueue(new SendableMessage
                        {
                            Address = item.Key,
                            Message = message
                        });
                    }
                }
            }
        }
    }
}