﻿using System.Collections.Generic;
using DetourClient;
using UnityEngine;
using UnityEngine.Scripting;

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
        public List<PlayerDefinition> Players;
        public int ClientStartPosition;
        public SimpleVector2Int MapSize;
        public TileData[,] MapTiles;
    }

    [System.Serializable]
    public class PlayerDefinition
    {
        public string Id;
        public string Name;
        public Vector2 Position;
        public bool HasMoved;
        public int StartPosition;
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

    [System.Serializable, Preserve]
    public class SimpleVector2Int
    {
        public int x;
        public int y;

        public SimpleVector2Int(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
