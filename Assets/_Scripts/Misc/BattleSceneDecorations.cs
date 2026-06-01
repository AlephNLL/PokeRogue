using UnityEngine;

public class BattleScene : MonoBehaviour
{
    [SerializeField] GameObject[] roomPrefabs;
    [SerializeField] GameObject[] room2Prefabs;
    void Start()
    {
        SpawnRandomRoom();
    }

    private void ClearPreviousChildren()
    {
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    private void SpawnRandomRoom()
    {
        int chosenRoom;
        GameObject newRoom;

        print(MapManager.instance.currentNode.floorLevel);
        if (MapManager.instance.currentNode.floorLevel < 10)
        {
            chosenRoom = Random.Range(0, room2Prefabs.Length - 1);
            newRoom = Instantiate(room2Prefabs[chosenRoom], transform.position, transform.rotation, transform);
        }
        else
        {
            chosenRoom = Random.Range(0, roomPrefabs.Length - 1);
            newRoom = Instantiate(roomPrefabs[chosenRoom], transform.position, transform.rotation, transform);
        }
    }
}


