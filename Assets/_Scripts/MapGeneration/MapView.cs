using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;

public class MapView : MonoBehaviour
{
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private Material lineMaterial;


    public void DrawNode(MapNode node)
    {
        GameObject map = GameObject.Find("Map");
        GameObject connections = GameObject.Find("Connections");

        GameObject mapNode = Instantiate(nodePrefab, node.position, Quaternion.identity);
        mapNode.transform.parent = map.transform;
    }

    public void DrawConnection(MapNode nodeA, MapNode nodeB)
    {
        GameObject map = GameObject.Find("Map");
        GameObject connections = GameObject.Find("Connections");

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
}