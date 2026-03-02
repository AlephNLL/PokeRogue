using System.Collections.Generic;
using UnityEngine;

public static class ProceduralGenerationAllgorithms
{
    // Contiene algoritmos de generación procedural

    public static List<Vector2Int> directions = new List<Vector2Int>
    {
        new Vector2Int(0, 1),
        new Vector2Int(1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(-1, 0),
    };

    public static HashSet<Vector2Int> SimpleRandomWalk(Vector2Int startPosition, int walkLenght)
    {
        // Elige una dirección aleatoria a la que moverse de forma aleatoria y devuelve las posiciones del camino recorrido
        HashSet<Vector2Int> path = new HashSet<Vector2Int>();

        path.Add(startPosition);
        var previousPosition = startPosition;

        for (int i = 0; i < walkLenght; i++) 
        {
            var newPosition = previousPosition + GetRandomDirection();
            path.Add(newPosition);
            previousPosition = newPosition;
        }
        return path;
    }

    public static Vector2Int GetRandomDirection()
    {
        // Devuelve una dirección cardinal aleatoria
        return directions[Random.Range(0, directions.Count)];
    }
}
