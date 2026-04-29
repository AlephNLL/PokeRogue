using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Canvas canvas;

    public GameObject inventory;
    public GameObject slot;

    public List<GameObject> instantiatedItems;

    private void Start()
    {
        Instance = this;
    }

    public void ShowCanvas(bool state)
    {
        if (canvas != null)
        {
            if (state)
            {
                StartCoroutine(WaitAndShow(2f, canvas));
            } else
            {
                canvas.enabled = state;
            }
        }
    }

    private IEnumerator WaitAndShow(float seconds, Canvas canvas)
    {
        yield return new WaitForSeconds(seconds);
        canvas.enabled = true;
        ShowItems();
    }

    private void ShowItems()
    {
        if (instantiatedItems != null)
        {
            foreach (GameObject item in instantiatedItems)
            {
                Destroy(item);
            }
            instantiatedItems.Clear();
        }

        instantiatedItems = new List<GameObject>();

        foreach (Item item in PlayerData.Instance.p_items)
        {
            GameObject newSlot = Instantiate(slot);
            newSlot.transform.parent = inventory.transform;
            newSlot.transform.localScale = Vector3.one;

            GameObject itemIcon = Instantiate(item.icon);
            itemIcon.transform.parent = newSlot.transform;
            itemIcon.transform.localScale = Vector3.one;

            instantiatedItems.Add(newSlot);
        }
    }
}
