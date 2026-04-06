using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using System.Linq;

public class MapView : MonoBehaviour
{
    [Header("Prefabs Tipos de sala")]
    [SerializeField] private GameObject notAssignedPrefab;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private GameObject treasurePrefab;
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private GameObject healPrefab;
    [SerializeField] private GameObject shopPrefab;

    [SerializeField] private Material lineMaterial;

    private GameObject map;
    private GameObject connections;
    private GameObject nodes;


    public void DrawMap(List<MapNode> path)
    {
        ClearMap();
        CreateEmptyMap();
        DrawNodes(path);
        DrawConnections(path);
        PassConnectedRooms(path);
        if (!MapManager.instance.mapCreated)
        {
            MapManager.instance.UnlockStartingPaths();
            MapManager.instance.mapCreated = true;
        }
    }

    public void CreateEmptyMap()
    {
        map = new("Map");
        connections = new("Conections");
        connections.transform.parent = map.transform;
        nodes = new("Nodes");
        nodes.transform.parent = map.transform;
    }

    public void DrawNodes(List<MapNode> path)
    {
        foreach (MapNode node in path)
        {
            switch (node.roomType)
            {
                case RoomType.NOT_ASSIGNED:
                    GameObject mapNode = Instantiate(notAssignedPrefab, node.position, Quaternion.identity);
                    mapNode.name = node.roomType + "-" + node.id;
                    PassNodeData(mapNode, node);
                    MapManager.instance.createdRooms.Add(mapNode);
                    break;
                case RoomType.Boss:
                    GameObject bossNode = Instantiate(bossPrefab, node.position, Quaternion.identity);
                    bossNode.name = node.roomType + "-" + node.id;
                    PassNodeData(bossNode, node);
                    MapManager.instance.createdRooms.Add(bossNode);
                    break;
                case RoomType.Heal:
                    GameObject healNode = Instantiate(healPrefab, node.position, Quaternion.identity);
                    healNode.name = node.roomType + "-" + node.id;
                    PassNodeData(healNode, node);
                    MapManager.instance.createdRooms.Add(healNode);
                    break;
                case RoomType.Enemy:
                    GameObject enemyNode = Instantiate(enemyPrefab, node.position, Quaternion.identity);
                    enemyNode.name = node.roomType + "-" + node.id;
                    enemyNode.GetComponent<Node>().sceneName = "BattleScene";
                    PassNodeData(enemyNode, node);
                    MapManager.instance.createdRooms.Add(enemyNode);
                    break;
                case RoomType.Shop:
                    GameObject shopNode = Instantiate(shopPrefab, node.position, Quaternion.identity);
                    shopNode.name = node.roomType + "-" + node.id;
                    PassNodeData(shopNode, node);
                    MapManager.instance.createdRooms.Add(shopNode);
                    break;
                case RoomType.Treasure:
                    GameObject treasureNode = Instantiate(treasurePrefab, node.position, Quaternion.identity);
                    treasureNode.name = node.roomType + "-" + node.id;
                    PassNodeData(treasureNode, node);
                    MapManager.instance.createdRooms.Add(treasureNode);
                    break;
            }
        }
    }

    public void DrawConnections(List<MapNode> path)
    {
        foreach (MapNode node in path)
        {
            foreach (MapNode connection in node.connectedNodes)
            {
                GameObject connectionGO = new("Connection", typeof(LineRenderer));
                LineRenderer lr = connectionGO.GetComponent<LineRenderer>();
                lr.material = lineMaterial;
                lr.startWidth = 0.1f;
                lr.endWidth = 0.1f;
                lr.positionCount = 2;
                lr.SetPosition(0, node.position);
                lr.SetPosition(1, connection.position);
                connectionGO.transform.parent = connections.transform;
            }
        }
    }

    public void ClearMap()
    {
        if (map != null && MapManager.instance.nodes.Count() != 0) { Destroy(map); MapManager.instance.createdRooms.Clear(); MapManager.instance.nodes.Clear(); }
    }

    public void PassNodeData(GameObject mapNode, MapNode node)
    {
        mapNode.transform.parent = nodes.transform;
        Node nodeData = mapNode.GetComponent<Node>();
        nodeData.position = node.position;
        nodeData.gridPosition = node.gridPosition;
        nodeData.roomType = node.roomType;
        //nodeData.sceneName = "TestScene"; // Escena de testeo de momento
        nodeData.id = node.id;
        nodeData.floorLevel = node.floorLevel;

        if (!MapManager.instance.nodes.Contains(node))
        {
            MapManager.instance.nodes.Add(node);
        }
    }

    public void PassConnectedRooms(List<MapNode> path)
    {
        foreach (MapNode node in path)
        {
            GameObject instance = MapManager.instance.createdRooms.FirstOrDefault(g => g.name == $"{node.roomType}-{node.id}");
            Node mapNode = instance.GetComponent<Node>();

            if (mapNode != null)
            {
                foreach (MapNode connectionTarget in node.connectedNodes)
                {
                    GameObject targetInstance = MapManager.instance.createdRooms.FirstOrDefault(g => g.name == $"{connectionTarget.roomType}-{connectionTarget.id}");

                    if (targetInstance != null)
                    {
                        mapNode.connectedNodes.Add(targetInstance);
                        //Debug.Log($"Nodo conectado con {targetInstance.name}");
                    }
                }
            }
        }
    }
}