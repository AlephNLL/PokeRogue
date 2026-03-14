using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Node : MonoBehaviour
{
    public int id;
    public Vector3 position = Vector3.zero;
    public Vector2Int gridPosition = Vector2Int.zero;
    public List<GameObject> connectedNodes = new List<GameObject>();
    public RoomType roomType;
    public GameObject roomPrefab;
    public int floorLevel;
}
