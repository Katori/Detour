namespace DetourServer
{
    [System.Serializable]
    public class DetourMessage
    {
        public int MessageType;
        public float DetourVersion;
        public float ApplicationVersion;
        public string RoomId;
    }

    [System.Serializable]
    public class RoomRequestMessage : DetourMessage
    {
        public string RequestedRoomId;
        public string RequestedRoomType;
    }

    [System.Serializable]
    public class PlayerRemovedMessage : DetourMessage
    {
        public string Id;
    }

    public class MessageDefinition
    {
        public MessageEventHandler EventHandler;
        public System.Type Type;
    }

    [System.Serializable]
    public class SendableMessage
    {
        public string Address;
        public DetourMessage Message;
    }

    public delegate void MessageEventHandler(string Address, DetourMessage netMsg);
}