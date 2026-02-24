using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualizer : MonoBehaviour
{
    private int tileSize = 1;

    [SerializeField]
    private GameObject tile;

    public void paintTiles(HashSet<Vector2Int> path)
    {
        Vector3Int cellPosition = new Vector3Int(0, 0, 0);
        GameObject floor = new GameObject("Floor");
        foreach (var position in path)
        {
            cellPosition = new Vector3Int(position.x * tileSize, 0, position.y * tileSize);

            GameObject cell = Instantiate(tile, cellPosition, Quaternion.identity);
            cell.transform.parent = floor.transform;
        }
    }

    public void Clear()
    {
        GameObject floor = GameObject.Find("Floor");

        if (floor != null )
        {
            Destroy(floor);
        }
    }
}
