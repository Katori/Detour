using UnityEngine;

namespace Detour.Examples.Client
{
    internal class ExampleGameController : MonoBehaviour
    {
        internal static ExampleGameController Instance { get; private set; }

        internal TileData[,] tiles;

        [SerializeField]
        private LayerMask ClickableTiles;

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

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var _hit = new RaycastHit();
                var _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(_ray, out _hit, Mathf.Infinity, ClickableTiles))
                {
                    var c = new Vector2(_hit.collider.transform.position.x, _hit.collider.transform.position.z);
                    var p = MapControllerComponent.Instance.WorldToMapPosition(c);
                    ClickedCell(new Vector2Int(Mathf.RoundToInt(p.x), Mathf.RoundToInt(p.y)));
                }
            }
            
        }

        internal void SetTiles(TileData[,] newTiles)
        {
            tiles = newTiles;
        }

        internal void ClickedCell(Vector2Int Position)
        {
            var c = tiles[Position.x, Position.y];
            DetourExampleConnection.Instance.Send(new PlayerMoveMessage
            {
                MessageType = (int)MessageTypes.PlayerMoveMessage,
                PositionToOperateOn = new SimpleVector2Int(Position.x, Position.y)
            });
        }
    }
}