namespace DetourClient
{
    [System.Serializable]
    public class DetourMessage
    {
        public int MessageType;
        public string DetourVersion;
        public string ApplicationVersion;
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

    public delegate void MessageEventHandler(DetourMessage netMsg);
}