using UnityEngine;
using System.Collections.Generic;
using System;

public class RoomAssigner : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;

    [Header("Probabilidad de salas")]
    public float enemyProb = 0.7f;
    public float healProb = 0.8f;
    public float treasureProb = 0.9f;
    public float shopProb = 1;

    public void AssignRoomTypes(List<MapNode> path)
    {
        AssignStartRooms(path);
        AssignRandomRooms(path);
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
                else if (randomValue <= shopProb)
                {
                    room.roomType = RoomType.Shop;
                }
                else
                {
                    room.roomType = RoomType.NOT_ASSIGNED;
                }
            }
        }
    }
}
    
