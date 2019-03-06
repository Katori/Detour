using UnityEngine;

/// <summary>
/// In this example we use the non-generic version of HexMap and store the tile data in a simple array which we keep aligned to the indices of the map tiles.
/// We randomly assign a terrain value to each tile and set each tile of the map boundary to terrain type 0 (water). We also can cycle through the terrain types of a tile by clicking on it
/// </summary>
namespace Detour.Examples.Client
{
    public class MapControllerComponent : MonoBehaviour
    {
        internal static MapControllerComponent Instance { get; private set; }

        [SerializeField] GameObject tilePrefab;
        [SerializeField] GameObject forestPrefab;
        [SerializeField] Material[] materials;
        private TileData[,] tiles;

        [SerializeField]
        private Transform MapContainer;

        private void Start()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        internal void RenderMap(TileData[,] TileArray, SimpleVector2Int MapSize)
        {
            tiles = TileArray;
            for (int i = 0; i < MapSize.x; i++)
            {
                for (int u = 0; u < MapSize.y; u++)
                {
                    var tile = tiles[i, u];
                    var c = Instantiate(tilePrefab, MapContainer);
                    c.transform.position = new Vector3(i, 0, u);
                    c.GetComponent<MeshRenderer>().material = materials[tile.terrainType];
                    if (tile.forest)
                    {
                        var b = Instantiate(forestPrefab, MapContainer);
                        b.transform.position = new Vector3(i, 0, u) + Vector3.up;
                    }
                    var l = c.GetComponent<MapCellComponent>();
                    l.Position = new Vector2Int(i, u);
                }
            }
            MapContainer.position = new Vector3(MapSize.x / -2, 0, MapSize.y/-2);
        }

        internal Vector2 WorldToMapPosition(Vector2 WorldPosition)
        {
            return WorldPosition - new Vector2(MapContainer.position.x, MapContainer.position.z);
        }

        internal Vector2 MapToWorldPosition(Vector2 MapPosition)
        {
            return MapPosition + new Vector2(MapContainer.position.x, MapContainer.position.z);
        }
    }
}