﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetourServer
{
    public static class Server
    {
        public static Dictionary<string, ClientContainer> AllClients = new Dictionary<string, ClientContainer>();

        public static Dictionary<int, MessageDefinition> MessageTypeToMessageDefinition = new Dictionary<int, MessageDefinition>();

        public static event Action<string, DetourMessage> ServerReceivedMessage;

        public static Dictionary<string, RoomDefinition> RoomTypes = new Dictionary<string, RoomDefinition>();

        public static Dictionary<string, Room> Rooms = new Dictionary<string, Room>();

        private static Random Tumbler = new Random();
        public static float DetourVersion;
        public static float ApplicationVersion;

        public static void StoreClientData(string address, string v, dynamic Data)
        {
            if (AllClients[address].StoredData.ContainsKey(v))
            {
                AllClients[address].StoredData[v] = Data;
            }
            else
            {
                AllClients[address].StoredData.Add(v, Data);
            }
        }

        public static void UseRoomHandling(System.Type RoomMessageType, RoomDefinition DefaultRoomType)
        {
            RegisterHandler(10, RoomMessageType, RoomRequestReceived);
            RoomTypes.Add(DefaultRoomType.RoomType, DefaultRoomType);
        }

        /// <summary>  
        ///  Adds a client to the Dictionary of clients.
        /// </summary> 
        public static ClientContainer AddClient(ClientContainer Client)
        {
            AllClients.Add(Client.Id, Client);
            return AllClients[Client.Id];
        }

        /// <summary>  
        ///  Enqueues a Message to send to Address as JSON.
        /// </summary> 
        public static void SendMessage(string Address, DetourMessage Message)
        {
            AllClients[Address].EnqueuedMessagesToSend.Add(Message);
        }

        /// <summary>  
        ///  Registers a MessageType and its associated Handler for receiving messages.
        /// </summary> 
        public static void RegisterHandler(int HandlerId, Type MessageType, MessageEventHandler Handler)
        {
            MessageTypeToMessageDefinition.Add(HandlerId, new MessageDefinition { EventHandler = Handler, Type = MessageType });
        }

        public static void RemoveClient(ClientContainer clientContainer)
        {
            System.Console.WriteLine("removing client");
            AllClients.Remove(clientContainer.Id);
            var c = Rooms.Values;
            foreach (var item in c)
            {
                // for each room
                var p = item.RoomClients.Values.Where(x => x.Id == clientContainer.Id).ToList();
                if (p.Count > 0)
                {
                    // if player in room
                    item.RoomClients.Remove(clientContainer.Id);
                }
            }
        }

        /// <summary>  
        ///  Called by the ClientContainer when message is received, runs it through the appropriate handler.
        /// </summary>
        public static void ReceivedMessage(string Address, DetourMessage v)
        {
            if (MessageTypeToMessageDefinition.ContainsKey(v.MessageType))
            {
                MessageTypeToMessageDefinition[v.MessageType].EventHandler(Address, v);
            }

            if (ServerReceivedMessage != null)
            {
                ServerReceivedMessage.Invoke(Address, v);
            }
        }

        /// <summary>  
        ///  Re-initializes Message Handlers (all handlers will be cleared)
        /// </summary>
        public static void ClearHandlers()
        {
            MessageTypeToMessageDefinition = new Dictionary<int, MessageDefinition>();
        }

        public static void RegisterRoomDefinition(string RoomType, int RoomCapacity)
        {
            RoomTypes.Add(RoomType, new RoomDefinition {RoomType = RoomType, RoomCapacity = RoomCapacity });
        }

        public static void CreateRoom(string RoomType, string RoomId = "", bool privateRoom = false)
        {
            var c = RoomTypes[RoomType];
            string _roomId = System.Guid.NewGuid().ToString();
            if (RoomId != "")
            {
                _roomId = RoomId;
            }
            Rooms.Add(_roomId, new Room { RoomId = _roomId, RoomType = RoomType, RoomClientCapacity = c.RoomCapacity, PrivateRoom = privateRoom });
        }

        public static void SendToRoom(string RoomId, DetourMessage Message)
        {
            Rooms[RoomId].SendToAll(Message);
        }

        public static void SendToRoomExcept(string RoomId, List<string> AddressesToExclude, DetourMessage Message)
        {
            Rooms[RoomId].SendToAllExcept(AddressesToExclude, Message);
        }

        public static string MatchToRoom(string RoomTypeRequested, ClientContainer ClientToMatch)
        {
            var _AvailableRooms = Rooms.Where(x => x.Value.RoomType == RoomTypeRequested).ToList();
            if (_AvailableRooms.Count > 0)
            {
                var _SelectedRoom = _AvailableRooms[Tumbler.Next(0, _AvailableRooms.Count)];
                _SelectedRoom.Value.AddToRoom(ClientToMatch);
                Console.WriteLine("client added to existing room");
                return _SelectedRoom.Key;
            }
            else
            {
                var _newRoomId = System.Guid.NewGuid().ToString();
                var RequestedRoomType = RoomTypes[RoomTypeRequested];
                Rooms.Add(_newRoomId, new Room {RoomId = _newRoomId, RoomClientCapacity = RequestedRoomType.RoomCapacity, RoomType = RequestedRoomType.RoomType });
                Rooms[_newRoomId].AddToRoom(ClientToMatch);
                Console.WriteLine("client added to new room");
                return _newRoomId;
            }
        }

        public static void RoomRequestReceived(string Address, DetourMessage RoomMessage)
        {
            Console.WriteLine("room request received");
            var _RoomMessageFromClient = RoomMessage as RoomRequestMessage;
            var _SelectedClient = AllClients[Address];
            if (RoomTypes.ContainsKey(_RoomMessageFromClient.RequestedRoomType))
            {
                var successfulMatch = MatchToRoom(_RoomMessageFromClient.RequestedRoomType, _SelectedClient);
                if (successfulMatch!=null)
                {
                    RoomTypes[_RoomMessageFromClient.RequestedRoomType].OnRoomJoined.Invoke(Address, successfulMatch, RoomMessage);
                }
            }
        }
    }
}
