using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager instance;
    public GameObject[] slots;
    [SerializeField] Item[] itemPool;
    [SerializeField] List<Item> items;

    public Item selectedItem;

    private Coroutine returnHandCoroutine;

    int lastIndex = 0;
    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        HandAnimatorHelper.onAnimationEnd -= OpenShop;
        HandAnimatorHelper.onAnimationEnd += OpenShop;

        ShopManagerUI.instance.UpdatePlayerGold();
        AudioManager.instance.PlayMusic(AudioLibrary.instance.shopMusic);
    }
    private void Update()
    {
        UpdateSelectedItem();
        if (Input.GetMouseButtonDown(0))
        {
            BuyItem();
        }
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
    void BuyItem()
    {
        if (!selectedItem) return;
        if (selectedItem.cost < PlayerData.Instance.gold)
        {
            GraphicRaycasting.instance.GetObjectUnderMouse().SetActive(false);
            PlayerData.items.Add(selectedItem);
            PlayerData.Instance.p_items.Add(selectedItem);
            PlayerData.Instance.gold -= selectedItem.cost;
            if (PlayerData.Instance.gold < 0) PlayerData.Instance.gold = 0;
            ShopManagerUI.instance.UpdatePlayerGold();
            HandAnimatorHelper.instance.SetHandTriggerParameter("Sold");
        }
    }
    public int GetItemSlotIndex(GameObject item)
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
            if (itemUnderMouse.GetComponentInParent<InventoryTooltip>() != null)
            {
                itemUnderMouse.GetComponentInParent<InventoryTooltip>().DestroyTooltip();
                Destroy(itemUnderMouse.GetComponentInParent<InventoryTooltip>());
            }

            if (itemUnderMouse != null && itemUnderMouse.layer != 2)
            {
                if (returnHandCoroutine != null)
                {
                    StopCoroutine(returnHandCoroutine);
                    returnHandCoroutine = null;
                }

                int itemSlotIndex = GetItemSlotIndex(itemUnderMouse);
                float handXPos = itemSlotIndex - 3.75f;
                if (itemSlotIndex >= 3) handXPos += 2f;
                if (!HandAnimatorHelper.instance.IsHandAtXPos(handXPos))
                {
                    selectedItem = items[itemSlotIndex];
                    HandAnimatorHelper.instance.MoveToPosition(new Vector3(handXPos, 4.5f, HandAnimatorHelper.instance.transform.position.z), .2f);
                    HandAnimatorHelper.instance.SetHandBoolParameter("point", true);
                    lastIndex = itemSlotIndex;
                    ShopManagerUI.instance.ShowItemDescription(selectedItem);
                }
            }
            else
            {
                selectedItem = null;
                if (!HandAnimatorHelper.instance.IsHandAtXPos(0) && returnHandCoroutine == null)
                {
                    returnHandCoroutine = StartCoroutine(ReturnHandToDefautlPos());
                }
            }
        }

        IEnumerator ReturnHandToDefautlPos()
        {
            yield return new WaitForSeconds(.2f);
            HandAnimatorHelper.instance.MoveToDefaultPosition(.5f);
            HandAnimatorHelper.instance.SetHandBoolParameter("point", false);
            ShopManagerUI.instance.HideItemDescription();
            returnHandCoroutine = null;
        }
    }
    public void ExitShop()
    {
        AudioManager.instance.StopMusic();
        if (MapManager.instance != null)
        {
            MapManager.instance.LoadMapScene();
        }
        else
        {
            SceneManager.LoadSceneAsync("MapGeneration");
        }
    }

    private void OnDestroy()
    {
        HandAnimatorHelper.onAnimationEnd -= OpenShop;
    }
}
