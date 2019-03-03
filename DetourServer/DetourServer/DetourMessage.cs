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

    public class MessageDefinition
    {
        public MessageEventHandler EventHandler;
        public System.Type Type;
    }

    public delegate void MessageEventHandler(string Address, DetourMessage netMsg);
}