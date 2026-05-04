using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] GameObject[] slots;
    [SerializeField] Item[] itemPool;
    [SerializeField] List<Item> items;

    public Item selectedItem;

    int lastIndex = 0;
    private void Start()
    {
        HandAnimatorHelper.onAnimationEnd += OpenShop;
    }
    private void Update()
    {
        UpdateSelectedItem();
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
            items.Add(item);
        }
    }

    int GetItemSlotIndex(GameObject item)
    {
        int index = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (item.transform.IsChildOf(slots[i].transform))
            {
                index = i;
                break;
            }
        }

        return index;
    }

    void UpdateSelectedItem()
    {
        GameObject itemUnderMouse = GraphicRaycasting.instance.GetObjectUnderMouse();

        if (itemUnderMouse != null) 
        {
            StopAllCoroutines();
            int itemSlotIndex = GetItemSlotIndex(itemUnderMouse);
            float handXPos = itemSlotIndex - 3.75f;
            if (itemSlotIndex >= 3) handXPos += 2f;
            if (!HandAnimatorHelper.instance.IsHandAtXPos(handXPos))
            {
                selectedItem = items[itemSlotIndex];
                HandAnimatorHelper.instance.MoveToPosition(new Vector3(handXPos, 4, HandAnimatorHelper.instance.transform.position.z));
                HandAnimatorHelper.instance.SetHandBoolParameter("point", true);
                lastIndex = itemSlotIndex;
            }
        }
        else
        {
            selectedItem = null;
            if (!HandAnimatorHelper.instance.IsHandAtXPos(0)) StartCoroutine(ReturnHandToDefautlPos());
        }
    }

    IEnumerator ReturnHandToDefautlPos()
    {
        yield return new WaitForSeconds(.2f);
        HandAnimatorHelper.instance.MoveToDefaultPosition();
        HandAnimatorHelper.instance.SetHandBoolParameter("point", false);
    }
}
