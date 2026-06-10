using GameData;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapNodeData
{
    public int id;
    public Vector3 position;
    public Vector2Int gridPosition;
    public List<string> connectedNodesNames;
    public RoomType roomType;
    public string roomPrefabId;
    public int floorLevel;
    public string sceneName;
}
