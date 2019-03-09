using System.Collections.Generic;
using DetourServer;
using Newtonsoft.Json;

namespace DetourServerExampleAsp
{
    [System.Serializable]
    public enum MessageTypes
    {
        ClientSentTestMessage = 1,
        RoomRequestMessage = 10,
        ClientJoinedRoomMessage = 15,
        ClientRoomDataCatchUp = 16,
        PlayerMoveMessage = 20,
        PlayerMoveCommand = 21
    }

    [System.Serializable]
    public class ClientSentTestMessage : DetourMessage
    {
        public string TestString;
    }

    [System.Serializable]
    public class ClientRoomDataCatchUp : DetourMessage
    {
        public List<PlayerDefinition> Players;
        public Vector2Int ClientStartPosition;
        public Vector2Int MapSize;
        public TileData[,] MapTiles;
    }

    [System.Serializable]
    public class PlayerMoveMessage : DetourMessage
    {
        public Vector2Int PositionToOperateOn;
    }

    [System.Serializable]
    public class PlayerMoveCommand : DetourMessage
    {
        public string PlayerId;
        public Vector2Int PositionToMoveTo;
    }

    [System.Serializable]
    public class PlayerDefinition
    {
        public string Id;
        public string Name;
        public Vector2Int Position;
        public bool HasMoved;
    }

    [System.Serializable]
    public class ClientJoinedRoomMessage : DetourMessage
    {
        public PlayerDefinition Player;
    }

    [System.Serializable]
    public class ClientRequestingRoomJoin : RoomRequestMessage
    {
        public string Name;
    }

    [System.Serializable]
    public class TileData
    {
        public int terrainType;
        public bool forest;

        public TileData(int terrainType, bool forest)
        {
            this.terrainType = terrainType;
            this.forest = forest;
        }
    }

    [System.Serializable]
    public class Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }
    }

    [System.Serializable]
    public class Vector2Int
    {
        public int x;
        public int y;

        [JsonIgnore]
        public float Magnitude
        {
            get
            {
                return (float)System.Math.Sqrt(this.x * this.x + this.y * this.y);
            }
        }

        public static float Distance(Vector2Int a, Vector2Int b)
        {
            return (a - b).Magnitude;
        }

        static public Vector2Int operator +(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x + b.x, a.y + b.y);
        }

        static public Vector2Int operator -(Vector2Int a, Vector2Int b)
        {
            return new Vector2Int(a.x - b.x, a.y - b.y);
        }

        public Vector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
