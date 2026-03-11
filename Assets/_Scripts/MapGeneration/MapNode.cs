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

public class MapNode : ScriptableObject
{
    public int id;
    public Vector3 position = Vector3.zero;
    public Vector2Int gridPosition = Vector2Int.zero;
    public List<MapNode> connectedNodes = new List<MapNode>();
    public RoomType roomType;
    public GameObject roomPrefab;
    public int floorLevel;

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