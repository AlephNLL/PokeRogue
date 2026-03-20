using System;
using System.Collections.Generic;
using UnityEngine;

public static class WallGenerator
{
    public static void CreateWalls(HashSet<Vector2Int> floorPositions, TilemapVisualizer tilemapVisualizer)
    {
        HashSet<Vector2Int> wallPositions = FindWalls(floorPositions, ProceduralGenerationAllgorithms.directions);
        GameObject walls = new("Walls");
        foreach (Vector2Int wallPosition in wallPositions) 
        {
            tilemapVisualizer.PaintWall(wallPosition, floorPositions, ProceduralGenerationAllgorithms.directions, walls);
        }
    }

    private static HashSet<Vector2Int> FindWalls(HashSet<Vector2Int> floorPositions, List<Vector2Int> directions)
    {
        //Encuentra los bordes del suelo y devuelve una lista de posiciones.
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        foreach (var position in floorPositions)
        {
            foreach (var direction in directions) 
            { 
                var neighbourPosition = position + direction;
                if (floorPositions.Contains(neighbourPosition) == false)
                {
                    wallPositions.Add(neighbourPosition);
                }
            }
        }
        return wallPositions;
    }
}
