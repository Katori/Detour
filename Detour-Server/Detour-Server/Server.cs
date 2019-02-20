using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DetourServer
{
    public delegate void MessageEventHandler(DetourMessage netMsg);

    public static class Server
    {
        public static Dictionary<string, ClientContainer> Clients = new Dictionary<string, ClientContainer>();

        public static Dictionary<int, MessageEventHandler> MessageCodeToHandler = new Dictionary<int, MessageEventHandler>();

        public static Dictionary<int, Type> MessageCodeToType = new Dictionary<int, Type>();

        public static event Action<string, DetourMessage> ServerReceivedMessage;

        /// <summary>  
        ///  Adds a client to the Dictionary of clients.
        /// </summary> 
        public static void AddClient(ClientContainer Client)
        {
            Clients.Add(Client.Id, Client);
        }

        /// <summary>  
        ///  Enqueues a Message to send to Address as JSON.
        /// </summary> 
        public static void SendMessage(string Address, dynamic Message)
        {
            Clients[Address].EnqueuedMessagesToSend.Add(Message);
        }

        /// <summary>  
        ///  Registers a MessageType and its associated Handler for receiving messages.
        /// </summary> 
        public static void RegisterHandler(int HandlerId, Type MessageType, MessageEventHandler Handler)
        {
            MessageCodeToHandler.Add(HandlerId, Handler);
            MessageCodeToType.Add(HandlerId, MessageType);
        }

        /// <summary>  
        ///  Called by the ClientContainer when message is received, runs it through the appropriate handler.
        /// </summary>
        internal static void ReceivedMessage(string Address, DetourMessage v)
        {
            MessageCodeToHandler[v.MessageType](v);
            ServerReceivedMessage.Invoke(Address, v);
        }
    }
}
