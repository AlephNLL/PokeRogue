using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
public class MapGenerator : MonoBehaviour
{
    [SerializeField] private int gridWidth = 10;
    [SerializeField] private int gridHeight = 10;
    [SerializeField] private int startingPaths = 2;

    [SerializeField] private MapView mapView;

    private MapNode[,] currentGrid;
    List<MapNode> path;

    public void GenerateMap()
    {
        mapView.ClearMap();
        mapView.CreateEmptyMap();

        currentGrid = InitializeGrid();
        Debug.Log(currentGrid);
        path = GeneratePaths(currentGrid, startingPaths);
        Debug.Log(path);

    }

    private MapNode[,] InitializeGrid()
    {
        currentGrid = new MapNode[gridHeight, gridWidth];
        int nextId = 1;

        // Inicializar matriz de nodos vacios
        for (int floor = 0; floor < gridHeight; floor++)
        {
            for (int room = 0; room < gridWidth; room++)
            {
                currentGrid[floor, room] = new MapNode();
                currentGrid[floor, room].id = nextId++;
                currentGrid[floor, room].position = new Vector3(floor * 3, 0, room * 3);
                currentGrid[floor, room].floorLevel = floor;
            }
        }
        return currentGrid;
    }
    private List<MapNode> GeneratePaths(MapNode[,] grid, int startingPaths) 
    {
        path = new();

        // Elegir puntos de inicio
        for (int i = 0; i < startingPaths; i++)
        {
            int randomRoom = SelectRandomNode(path, grid);
            path.Add(grid[0, randomRoom]);

            mapView.DrawNode(grid[0, randomRoom]);
            Debug.Log("Creacion nodo nicial");

            for (int floor = 0; floor < gridHeight - 1; floor++)
            {
                randomRoom = SetupConnection(path, randomRoom, floor);
            }
        }
        return path;
    }

    private int SelectRandomNode(List<MapNode> path, MapNode[,] grid, int floorLevel = 0)
    {
        // Elige posiciones de inicio y verifica que no se encuentren ya en el path
        int randomRoom = Random.Range(0, gridWidth - 1);

        if (path.Contains(grid[floorLevel, randomRoom]))
        {
            return SelectRandomNode(path, grid);
        }
        return randomRoom;
    }

    private int SetupConnection(List<MapNode> path, int room, int floor)
    {
        // Conecta un nodo con otro del siguiente piso.

        int randomRoom = 0;
        MapNode nextRoom = null;
        MapNode currentRoom = currentGrid[floor, room];

        int maxAttempts = 100;
        int attempt = 0;

        while (nextRoom == null || CrossExistingPaths(room, floor, nextRoom))
        {
            // Limitar intentos para evitar crasheos :D
            if (attempt == maxAttempts)
            {
                Debug.Log("Max attempts reached");
                break;
            }
            randomRoom = (int)Math.Clamp(Random.Range(room - 1, room + 1), 0, gridWidth - 1);
            int nextFloor = floor + 1;

            nextRoom = currentGrid[nextFloor, randomRoom];
            attempt++;
        }
        currentRoom.AddConnection(nextRoom);
        mapView.DrawConnection(currentRoom, nextRoom);
        if (!path.Contains(nextRoom)) { path.Add(nextRoom); mapView.DrawNode(nextRoom); }
        return randomRoom;
    }

    private bool CrossExistingPaths(int room, int floor, MapNode nextRoom)
    {
        // Comprueba que el camino no se cruce con otros ya existentes antes de conectarlo.

        MapNode leftNeighbour = null;
        MapNode rightNeighbour = null;
        if (room > 0)
        {
            leftNeighbour = currentGrid[floor, room - 1];
        }
        if (room < gridWidth - 1)
        {
            rightNeighbour = currentGrid[floor, room + 1];
        }

        // Comprobar que no se crucen los caminos con el vecino izquierdo.
        if (leftNeighbour != null && nextRoom.position.x < room)
        {
            foreach (MapNode connections in leftNeighbour.connectedNodes)
            {
                if (connections.position.x / 3 > room)
                {
                    return true;
                }
            }
        }

        // Comprobar que no se crucen los caminos con el vecino derecho.
        if (rightNeighbour != null && nextRoom.position.x > room)
        {
            foreach (MapNode connections in rightNeighbour.connectedNodes)
            {
                if (connections.position.x / 3 < room)
                {
                    return true;
                }
            }
        }
        return false;
    }
}