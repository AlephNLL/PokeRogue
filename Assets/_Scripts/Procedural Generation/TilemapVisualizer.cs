using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class TilemapVisualizer : MonoBehaviour
{
    private int tileSize = 1;

    [SerializeField]
    private GameObject floorTile;
    [SerializeField]
    private GameObject wallTile;



    public void PaintTiles(HashSet<Vector2Int> path)
    {
        Vector3Int cellPosition = new Vector3Int(0, 0, 0);
        GameObject floor = new GameObject("Floor");
        foreach (var position in path)
        {
            cellPosition = new Vector3Int(position.x * tileSize, 0, position.y * tileSize);

            GameObject cell = Instantiate(floorTile, cellPosition, Quaternion.identity);
            cell.transform.parent = floor.transform;
        }
    }

    public void ClearFloor()
    {
        GameObject floor = GameObject.Find("Floor");

        if (floor != null )
        {
            Destroy(floor);
        }
    }
    public void ClearWalls()
    {
        GameObject walls = GameObject.Find("Walls");

        if (walls != null)
        {
            Destroy(walls);
        }
    }

    internal void PaintWall(Vector2Int wallPosition, HashSet<Vector2Int> floorPositions, List<Vector2Int> directions, GameObject walls)
    {
        Vector3Int wallDirection3D;
        Vector3 wallPosition3D = new(wallPosition.x, 0, wallPosition.y);

        // Busca en que dirección colocar la pared
        foreach (var direction in directions) 
        {
            var neighbourPosition = wallPosition + direction;
            if (floorPositions.Contains(neighbourPosition) == true)
            {
                wallDirection3D = new(direction.x, 0, direction.y);
                wallPosition3D = new(wallPosition.x + (direction.x * 0.5f), 0.5f, wallPosition.y + (direction.y * 0.5f));
                GameObject wall = Instantiate(wallTile, wallPosition3D, Quaternion.LookRotation(wallDirection3D));
                wall.transform.parent = walls.transform;
            }
        }
    }
}
