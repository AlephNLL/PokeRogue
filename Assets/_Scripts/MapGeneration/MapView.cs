using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;

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
                    passNodeData(mapNode, node);
                    break;
                case RoomType.Boss:
                    GameObject bossNode = Instantiate(bossPrefab, node.position, Quaternion.identity);
                    passNodeData(bossNode, node);
                    break;
                case RoomType.Heal:
                    GameObject healNode = Instantiate(healPrefab, node.position, Quaternion.identity);
                    passNodeData(healNode, node);
                    break;
                case RoomType.Enemy:
                    GameObject enemyNode = Instantiate(enemyPrefab, node.position, Quaternion.identity);
                    passNodeData(enemyNode, node);
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
                GameObject connectionGO = new GameObject("Connection", typeof(LineRenderer));
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
        if (map != null) { Destroy(map); }
    }

    public void passNodeData(GameObject mapNode, MapNode node)
    {
        mapNode.transform.parent = nodes.transform;
        Node nodeData = mapNode.GetComponent<Node>();
        nodeData.position = node.position;
        nodeData.gridPosition = node.gridPosition;
        nodeData.connectedNodes = node.connectedNodes;
        nodeData.roomType = node.roomType;
        nodeData.roomPrefab = node.roomPrefab;
        nodeData.id = node.id;
        nodeData.floorLevel = node.floorLevel;
    }
}