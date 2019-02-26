using System.Collections.Generic;
using DetourClient;

namespace Detour.Examples.Client
{
    [System.Serializable]
    public enum MessageTypes
    {
        ClientSentTestMessage = 1,
        RoomRequestMessage = 10,
        ClientJoinedRoomMessage = 15,
        ClientRoomDataCatchUp = 16
    }

    [System.Serializable]
    public class ClientSentTestMessage : DetourMessage
    {
        public string TestString;
    }

    [System.Serializable]
    public class ClientRoomDataCatchUp : DetourMessage
    {
        public List<string> Names;
    }

    [System.Serializable]
    public class ClientJoinedRoomMessage : DetourMessage
    {
        public string Name;
    }

    [System.Serializable]
    public class ClientRequestingRoomJoin : RoomRequestMessage
    {
        public string Name;
    }
}
