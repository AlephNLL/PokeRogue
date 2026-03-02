using System;
using UnityEngine;

public abstract class AbstractLevelGenerator : MonoBehaviour
{
    [SerializeField]
    protected TilemapVisualizer tilemapVisualizer =null;
    [SerializeField]
    protected Vector2Int startPosition = Vector2Int.zero;

    public void GenerateLevel()
    {
        tilemapVisualizer.ClearFloor();
        tilemapVisualizer.ClearWalls();
        RunProceduralGeneration();
    }

    protected abstract void RunProceduralGeneration();
}
