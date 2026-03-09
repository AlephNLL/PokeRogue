using System.Collections.Generic;
using UnityEngine;

public enum RoomType
{
    NOT_ASSIGNED,
    Normal,
    Entrance,
    Shop,
    Enemy,
    Treasure
}

public class MapNode
{
    public int id;
    public Vector3 position = Vector3.zero;
    public List<MapNode> connectedNodes = new List<MapNode>();
    public RoomType roomType;
    public GameObject roomPrefab;
    public int floorLevel;

    void Start()
    {

    }

    public void AddConnection(MapNode node)
    {
        if (!connectedNodes.Contains(node))
        {
            connectedNodes.Add(node);
        }
    }

    public void RemoveConnection(MapNode node)
    {
        connectedNodes.Remove(node);
    }
}