using System.Collections.Generic;
using DetourServer;

namespace DetourServerExampleAsp
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
        public List<Vector2> Positions;
    }

    [System.Serializable]
    public class Vector2
    {
        public float X;
        public float Y;

        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
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
