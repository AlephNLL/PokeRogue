using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TreasureRoomManager : MonoBehaviour
{
    [SerializeField] Item[] itemPool;
    [SerializeField] Animator chestController;

    bool chestOpened;
    private void Start()
    {
        chestOpened = false;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!chestOpened)
            {
                chestOpened = true;
                StartCoroutine(OpenChest());
            }
        }
    }

    IEnumerator OpenChest()
    {
        chestController.SetTrigger("Open");

        Item itemToGive = itemPool[Random.Range(0, itemPool.Length)];

        yield return new WaitForSeconds(1);

        TreasureUIManager.instance.ShowItem(itemToGive);

        PlayerData.items.Add(itemToGive);

        yield return new WaitForSeconds(.5f);

        StartCoroutine(ReturnToMap());
    }
    IEnumerator ReturnToMap()
    {
        while (true) 
        {
            if (Input.anyKey)
            {
                if (MapManager.instance != null)
                {
                    MapManager.instance.LoadMapScene();
                }
                else
                {
                    SceneManager.LoadSceneAsync("MapGeneration");
                }
                yield break;
            }

            yield return null;
        }
    }
}
