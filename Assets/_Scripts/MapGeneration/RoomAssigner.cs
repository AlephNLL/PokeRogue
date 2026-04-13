using UnityEngine;
using System.Collections.Generic;
using System;
using GameData;

public class RoomAssigner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;

    [Header("Probabilidad de salas")]
    public float enemyProb = 0.8f;
    public float healProb = 0.95f;
    public float treasureProb = 1f;

    public void AssignRoomTypes(List<MapNode> path)
    {
        AssignStartRooms(path);
        AssignRandomRooms(path);
        CheckPath(path);
    }

    private void CheckPath(List<MapNode> path)
    {
        foreach (MapNode room in path)
        {
            RoomType currentType = room.roomType;

            if (currentType == RoomType.Enemy) 
            {
                continue;
            }

            foreach (MapNode connection in room.connectedNodes)
            {
                RoomType connectionType = connection.roomType;
                if (currentType == connectionType)
                {
                    connection.roomType = RoomType.Enemy;
                }
            }
        }
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
            if (room.floorLevel == mapGenerator.gridHeight - 2)
            {
                room.roomType = RoomType.Shop;
            }
            if (room.floorLevel == mapGenerator.gridHeight - 4)
            {
                room.roomType = RoomType.Treasure;
            }
        }
    }

    private void AssignRandomRooms(List<MapNode> path)
    {
        foreach(MapNode room in path)
        {
            if (room.roomType == RoomType.NOT_ASSIGNED)
            {
                float randomValue = UnityEngine.Random.value;

                if (randomValue <= enemyProb)
                {
                    room.roomType = RoomType.Enemy;
                }
                else if (randomValue <= healProb)
                {
                    room.roomType = RoomType.Heal;
                }
                else if (randomValue <= treasureProb)
                {
                    room.roomType = RoomType.Treasure;
                }
                else
                {
                    room.roomType = RoomType.NOT_ASSIGNED;
                }
            }
        }
    }
}
    
