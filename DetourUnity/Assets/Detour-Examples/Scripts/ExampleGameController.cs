﻿using UnityEngine;

namespace Detour.Examples.Client
{
    internal class ExampleGameController : MonoBehaviour
    {
        internal static ExampleGameController Instance { get; private set; }

        internal TileData[,] tiles;

        private void Start()
        {
            if(Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        internal void SetTiles(TileData[,] newTiles)
        {
            tiles = newTiles;
        }

        internal void ClickedCell(Vector2Int Position)
        {
            var c = tiles[Position.x, Position.y];
            //var p = MapControllerComponent.Instance.WorldToMapPosition(Position);
            DetourExampleConnection.Instance.Send(new PlayerMoveMessage
            {
                MessageType = (int)MessageTypes.PlayerMoveMessage,
                PositionToOperateOn = new SimpleVector2Int(Position.x, Position.y)
            });
        }
    }
}