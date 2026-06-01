    using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using GameData;
using UnityEditor.Experimental.GraphView;
using System.Collections;
using UnityEngine.SceneManagement;
public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;
    [Header("Ajustes del grid")]
    [SerializeField] public int gridWidth = 10;
    [SerializeField] public int gridHeight = 10;
    [SerializeField] private int startingPaths = 2;
    [SerializeField] private int iterations = 2;
    [SerializeField] private float randomRange = 0.5f;

    [Header("Referencias")]
    [SerializeField] private MapView mapView;
    [SerializeField] private RoomAssigner roomAssigner;

    private MapNode[,] currentGrid;
    List<MapNode> path;
    List<MapNode> worldPath = new();

    private int lastId = 0;

    private MapNode bossRoom;

    private void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); }
    }

    public void GenerateMap()
    {
        worldPath.Clear();
        lastId = 0;

        MapManager.instance.mapCreated = false;
        currentGrid = InitializeGrid();
        path = GeneratePaths(currentGrid, startingPaths);
        GenerateBossRoom();
        GenerateStartRoom();

        roomAssigner.AssignRoomTypes(path);

        MapManager.instance.nodes = path;
        MapManager.instance.mapCreated = true;

        if (SceneManager.GetActiveScene().name != "MapGeneration") return;
        mapView.DrawMap(path);
        MapManager.instance.currentRoom = GameObject.Find(MapManager.instance.currentRoomName);
        MapCamera.SetSelectedObject(MapManager.instance.currentRoom);
    }

    private MapNode[,] InitializeGrid(int startFloor = 0, int offset = 0)
    {
        currentGrid = new MapNode[gridHeight, gridWidth];

        // Inicializar matriz de nodos vacios
        for (int floor = 0; floor < gridHeight; floor++)
        {
            for (int room = 0; room < gridWidth; room++)
            {
                currentGrid[floor, room] = new MapNode();
                currentGrid[floor, room].id = lastId++;
                currentGrid[floor, room].gridPosition = new Vector2Int(floor, room);
                currentGrid[floor, room].floorLevel = floor + startFloor;

                if (startFloor != 0) { currentGrid[floor, room].floorLevel = floor + startFloor + 1; }

                float floorPosRandomized = Random.Range((floor + startFloor) * 3 + offset - randomRange, (floor + startFloor) * 3 + offset + randomRange);
                float roomPosRandomized = Random.Range(room * 3 - randomRange, room * 3 + randomRange);
                currentGrid[floor, room].position = new Vector3(floorPosRandomized, 0, roomPosRandomized);
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
            int randomStartRoom = SelectRandomNode(path, grid);
            path.Add(grid[0, randomStartRoom]);
            worldPath.Add(grid[0, randomStartRoom]);

            for (int iteration = 0; iteration < iterations; iteration++)
            {
                int randomRoom = randomStartRoom;
                for (int floor = 0; floor < gridHeight - 1; floor++)
                {
                    randomRoom = SetupConnection(path, randomRoom, floor);
                }
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
            randomRoom = (int)Math.Clamp(Random.Range(room - 1, room + 2), 0, gridWidth - 1);
            int nextFloor = floor + 1;

            nextRoom = currentGrid[nextFloor, randomRoom];
            attempt++;
        }
        if (nextRoom != null)
        {
            if (!currentRoom.connectedNodesIds.Contains(nextRoom.id)) { currentRoom.AddConnection(nextRoom); }
            if (!path.Contains(nextRoom)) { path.Add(nextRoom); worldPath.Add(nextRoom); }
        }

        return randomRoom;
    }

    private bool CrossExistingPaths(int room, int floor, MapNode nextRoom)
    {
        // Comprueba que el camino no se cruce con otros ya existentes antes de conectarlo.

        if (nextRoom == null)
        {
            return true;
        }

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

        if (nextRoom.gridPosition.y == room) { return false; }

        // Comprobar que no se crucen los caminos con el vecino izquierdo.
        if (leftNeighbour != null && nextRoom.gridPosition.y < room)
        {
            foreach (int connectionId in leftNeighbour.connectedNodesIds)
            {
                MapNode connection = path.FirstOrDefault(g => g.id == connectionId);
                if (connection.gridPosition.y > leftNeighbour.gridPosition.y)
                {
                    return true;
                }
            }
        }

        // Comprobar que no se crucen los caminos con el vecino derecho.
        if (rightNeighbour != null && nextRoom.gridPosition.y > room)
        {
            foreach (int connectionId in rightNeighbour.connectedNodesIds)
            {
                MapNode connection = path.FirstOrDefault(g => g.id == connectionId);
                if (connection.gridPosition.y < rightNeighbour.gridPosition.y)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void GenerateBossRoom(int startFloor = 0, int offset = 0)
    {
        bossRoom = new MapNode();
        bossRoom.roomType = RoomType.Boss;
        bossRoom.position = new Vector3((startFloor + gridHeight) * 3 + 2 + offset, 0, (gridWidth * 3) / 2);
        bossRoom.floorLevel = gridHeight;
        bossRoom.id = -1;

        int secondBossOffset = 0;
        if (startFloor != 0) { bossRoom.id = lastId++; secondBossOffset = 1; }

        bossRoom.floorLevel = gridHeight + startFloor + secondBossOffset;

        foreach (MapNode room in path)
        {
            if (room.floorLevel == gridHeight - 1 + startFloor + secondBossOffset)
            {
                room.AddConnection(bossRoom);
            }
        }
        path.Add(bossRoom);
        worldPath.Add(bossRoom);
    }

    private void GenerateStartRoom()
    {
        MapNode start = new MapNode();
        start.roomType = RoomType.Spawn;
        start.position = new Vector3(-3, 0, (gridWidth * 3) / 2);
        start.floorLevel = -1;

        foreach (MapNode room in path)
        {
            if (room.floorLevel == 0)
            {
                start.AddConnection(room);
            }
        }
        path.Add(start);
        worldPath.Add(start);
    }

    public void GenerateNextMap()
    {
        currentGrid = InitializeGrid(gridHeight, 7);
        path = GeneratePaths(currentGrid, startingPaths);
        GenerateBossRoom(gridHeight, 7);

        ConnectNextMap(gridHeight);

        roomAssigner.AssignRoomTypes(path);
        mapView.DrawMap(worldPath);
    }

    private void ConnectNextMap(int startFloor = 0)
    {
        GameObject bossGO = GameObject.Find("Boss--1");
        Node bossNode = bossGO.GetComponent<Node>();
        bossRoom = MapManager.instance.NodeToMapNode(bossNode);
        bossRoom.id = -2;
        bossRoom.floorLevel = startFloor + 1;

        foreach (MapNode room in path)
        {
            if (room.floorLevel == startFloor + 1)
            {
                bossRoom.AddConnection(room);
            }
        }
        path.Add(bossRoom);
        MapNode oldBossRoom = worldPath.FirstOrDefault(x => x.roomType == RoomType.Boss);
        Debug.Log(oldBossRoom.id);
        worldPath.Add(bossRoom);

        StartCoroutine(FindSpawn());
    }

    private IEnumerator FindSpawn()
    {
        yield return new WaitForNextFrameUnit();
        MapManager.instance.currentRoom = GameObject.Find("Spawn-0");
        MapCamera.SetSelectedObject(MapManager.instance.currentRoom);
        MapCamera.UpdateLayers(MapManager.instance.currentRoom);
        FixDuplicateBoss();
    }

    public void FixDuplicateBoss()
    {
        GameObject boss0 = GameObject.Find("Boss--1");
        GameObject boss1 = GameObject.Find("Boss--2");

        Node node0 = boss0.GetComponent<Node>();
        Node node1 = boss1.GetComponent<Node>();

        foreach (GameObject node in node1.connectedNodes)
        {
            node0.connectedNodes.Add(node);
            Debug.Log("Connected boss to next map");
        }

        MapManager.instance.createdRooms.Remove(boss1);

        //MapManager.instance.UpdatePathNodes(MapManager.instance.createdRooms);
        Destroy(boss1);
    }
}
