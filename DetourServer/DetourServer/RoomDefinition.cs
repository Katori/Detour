using System.Collections.Generic;

namespace DetourServer
{
    public class RoomDefinition
    {
        public string RoomType;
        public int RoomCapacity;
        public RoomJoinedHandler OnRoomJoined;
        public int StartPoints;
    }

    public delegate void RoomJoinedHandler(string Address, string RoomId, DetourMessage ExtraJoinData);
}