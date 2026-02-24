using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomWalkGenerator : MonoBehaviour
{
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;

    [SerializeField]
    protected int iterations = 10;
    [SerializeField]
    protected int walkLength = 10;
    [SerializeField]
    protected bool startRandomly = false;

    [SerializeField]
    private TilemapVisualizer tilemapVisualizer;

    public void RunProceduralGeneration()
    {
        HashSet<Vector2Int> floorPositions = RunRandomWalk();

        //foreach (var position in floorPositions)
        //{
        //    Debug.Log(position);
        //}

        tilemapVisualizer.Clear();
        tilemapVisualizer.paintTiles(floorPositions);
    }

    protected HashSet<Vector2Int> RunRandomWalk()
    {
        var currentPosition = startPosition;
        HashSet<Vector2Int> floorPositions = new HashSet<Vector2Int>();

        for (int i = 0; i < iterations; i++)
        {
            var path = ProceduralGenerationAllgorithms.SimpleRandomWalk(currentPosition, walkLength);
            floorPositions.UnionWith(path);

            if (startRandomly)
            {
                currentPosition = floorPositions.ElementAt(Random.Range(0, floorPositions.Count));
            }
        }
        return floorPositions;
    }
}
