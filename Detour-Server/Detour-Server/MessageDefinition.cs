namespace DetourServer
{
    public class MessageDefinition
    {
        public MessageEventHandler EventHandler;
        public System.Type Type;
    }

    public delegate void MessageEventHandler(DetourMessage netMsg);
}