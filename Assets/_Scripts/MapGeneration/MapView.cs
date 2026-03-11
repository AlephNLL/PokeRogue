using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;

public class MapView : MonoBehaviour
{
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private Material lineMaterial;
    GameObject map;
    GameObject connections;
    GameObject nodes;

    public void CreateEmptyMap()
    {
        map = new("Map");
        connections = new("Conections");
        connections.transform.parent = map.transform;
        nodes = new("Nodes");
        nodes.transform.parent = map.transform;
    }

    public void DrawNode(MapNode node)
    {
        GameObject mapNode = Instantiate(nodePrefab, node.position, Quaternion.identity);
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

    public void DrawConnection(MapNode nodeA, MapNode nodeB)
    {
        GameObject connectionGO = new GameObject("Connection", typeof(LineRenderer));
        LineRenderer lr = connectionGO.GetComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.positionCount = 2;
        lr.SetPosition(0, nodeA.position);
        lr.SetPosition(1, nodeB.position);
        connectionGO.transform.parent = connections.transform;
    }

    public void ClearMap()
    {
        if (map != null) { Destroy(map); }
    }
}