using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkGenerator : AbstractLevelGenerator
{
    [SerializeField]
    private RandomWalkSO randomWalkData;

    protected override void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalk();

        tilemapVisualizer.PaintTiles(floorPositions);
        WallGenerator.CreateWalls(floorPositions, tilemapVisualizer);
    }


    protected HashSet<Vector2Int> RunRandomWalk()
    {
        var currentPosition = startPosition;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < randomWalkData.iterations; i++)
        {
            var path = ProceduralGenerationAllgorithms.SimpleRandomWalk(currentPosition, randomWalkData.walkLength);
            floorPositions.UnionWith(path);

            if (randomWalkData.startRandomly)
            {
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            }
        }
        return floorPositions;
    }
}
