namespace DetourClient
{
    [System.Serializable]
    public class DetourMessage
    {
        public int MessageType;
        public string DetourVersion;
        public string ApplicationVersion;
    }

    public class MessageDefinition
    {
        public MessageEventHandler EventHandler;
        public System.Type Type;
    }

    public delegate void MessageEventHandler(DetourMessage netMsg);
}