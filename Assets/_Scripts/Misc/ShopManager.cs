using System.Collections;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] GameObject[] slots;
    [SerializeField] Item[] itemPool;

    private void Start()
    {
        HandAnimatorHelper.onAnimationEnd += OpenShop;
    }
    public void OpenShop()
    {
        GenerateStock();
        StartCoroutine(ShowSlots());
    }

    IEnumerator ShowSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetActive(true);
            yield return new WaitForSeconds(.2f);
        }
    }

    void GenerateStock()
    {
        for (int i = 3; i < slots.Length; i++)
        {
            Item item = itemPool[Random.Range(0, itemPool.Length)];
            Instantiate(item.icon, slots[i].transform);
        }
    }
}
