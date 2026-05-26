using UnityEngine;

public class BattleScene : MonoBehaviour
{
    [SerializeField] GameObject[] roomPrefabs;
    void Start()
    {
        ClearPreviousChildren();
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
        int chosenRoom = Random.Range(0, roomPrefabs.Length);
        GameObject newRoom = Instantiate(roomPrefabs[chosenRoom], transform.position, transform.rotation, transform);
        print(newRoom);
    }
}
