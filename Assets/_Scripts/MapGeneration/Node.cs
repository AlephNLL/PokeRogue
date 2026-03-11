using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public int id;
    public Vector3 position = Vector3.zero;
    public Vector2Int gridPosition = Vector2Int.zero;
    public List<MapNode> connectedNodes = new List<MapNode>();
    public RoomType roomType;
    public GameObject roomPrefab;
    public int floorLevel;
}
