using System;
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

        public static void UseRoomHandling()
        {
            RegisterHandler(10, typeof(RoomRequestMessage), RoomRequestReceived);
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

        public static bool MatchToRoom(string RoomTypeRequested, ClientContainer ClientToMatch)
        {
            var c = Rooms.Where(x => x.Value.RoomType == RoomTypeRequested).ToList();
            if (c.Count > 0)
            {
                var p = c[Tumbler.Next(0, c.Count)];
                p.Value.AddToRoom(ClientToMatch);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void RoomRequestReceived(string Address, DetourMessage RoomMessage)
        {
            var p = RoomMessage as RoomRequestMessage;
            var cl = AllClients[Address];
            MatchToRoom(p.RequestedRoomType, cl);
        }
    }
}
