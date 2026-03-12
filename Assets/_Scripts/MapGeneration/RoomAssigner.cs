using UnityEngine;
using System.Collections.Generic;
using System;

public class RoomAssigner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    public void AssignRoomTypes(List<MapNode> path)
    {
        AssignStartRooms(path);
    }

    private void AssignStartRooms(List<MapNode> path)
    {
        foreach (MapNode room in path)
        {
            if (room.floorLevel == 0)
            {
                room.roomType = RoomType.Enemy;
            }
            if (room.floorLevel == mapGenerator.gridHeight - 1)
            {
                room.roomType = RoomType.Heal;
            }

        }
    }
}
    
