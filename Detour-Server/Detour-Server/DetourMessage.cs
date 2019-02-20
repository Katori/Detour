namespace DetourServer
{
    [System.Serializable]
    public class DetourMessage
    {
        public int MessageType;
        public string DetourVersion;
        public string ApplicationVersion;

        public DetourMessage() { }
    }
}