using System.Collections.Generic;
using UnityEngine;
using GameData;

[System.Serializable]
public class MapNode : ScriptableObject
{
    public int id;
    public Vector3 position = Vector3.zero;
    public Vector2Int gridPosition = Vector2Int.zero;
    public List<MapNode> connectedNodes = new List<MapNode>();
    public RoomType roomType;
    public GameObject roomPrefab;
    public int floorLevel;
    public string sceneName;

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