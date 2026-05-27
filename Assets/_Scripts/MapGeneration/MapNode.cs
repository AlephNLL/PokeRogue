using System.Collections.Generic;
using UnityEngine;
using GameData;
using System.Linq;

[System.Serializable]
public class MapNode : ScriptableObject
{
    public int id;
    public Vector3 position = Vector3.zero;
    public Vector2Int gridPosition = Vector2Int.zero;
    public List<int> connectedNodesIds = new List<int>();
    public RoomType roomType;
    public GameObject roomPrefab;
    public int floorLevel;
    public string sceneName;

    public void AddConnection(MapNode node)
    {
        if (!connectedNodesIds.Contains(node.id))
        {
            connectedNodesIds.Add(node.id);
        }
    }

    public void RemoveConnection(MapNode node)
    {
        connectedNodesIds.Remove(node.id);
    }

    public MapNodeData LoadData()
    {
        MapNodeData data = new MapNodeData();
        data.id = id;
        data.position = position;
        data.gridPosition = gridPosition;
        data.roomType = roomType;
        data.roomPrefab = roomPrefab;
        data.floorLevel = floorLevel;
        data.sceneName = sceneName;

        data.connectedNodesNames = new();
        foreach (int mapNodeId in connectedNodesIds)
        {
            data.connectedNodesNames.Add(mapNodeId.ToString());
        }
        return data;
    }

    public MapNode SaveData(MapNodeData data, GameSaveData gameData)
    {
        id = data.id;
        position = data.position;
        gridPosition = data.gridPosition;
        roomType = data.roomType;
        roomPrefab = data.roomPrefab;
        floorLevel = data.floorLevel;
        sceneName = data.sceneName;

        connectedNodesIds = new();
        foreach (string name in data.connectedNodesNames)
        {
            MapNode empty = (MapNode)ScriptableObject.CreateInstance(typeof(MapNode));
            MapNodeData connectedData = gameData.mapData.FirstOrDefault(x => x.id.ToString() == name);
            connectedNodesIds.Add(connectedData.id);
        }
        return this;
    }
}