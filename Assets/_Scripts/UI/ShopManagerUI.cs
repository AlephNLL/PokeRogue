using TMPro;
using UnityEngine;

public class ShopManagerUI : MonoBehaviour
{
    public static ShopManagerUI instance;
    [SerializeField] GameObject itemTooltipBorder;
    [SerializeField] TMP_Text itemName;
    [SerializeField] TMP_Text itemCost;
    [SerializeField] TMP_Text itemDesc;
    [SerializeField] TMP_Text playerGoldText;
    private void Awake()
    {
        instance = this;
    }
    public void ShowItemDescription(Item selectedItem)
    {
        itemTooltipBorder.SetActive(true);
        itemTooltipBorder.transform.position = new Vector3(ShopManager.instance.slots[ShopManager.instance.GetItemSlotIndex(GraphicRaycasting.instance.GetObjectUnderMouse())].transform.position.x, itemTooltipBorder.transform.position.y, itemTooltipBorder.transform.position.z);
        itemName.text = selectedItem.name;
        itemCost.text = selectedItem.cost.ToString() + "g";
        itemDesc.text = selectedItem.description;
    }

    public void HideItemDescription()
    {
        itemTooltipBorder.SetActive(false);
    }

    public void UpdatePlayerGold()
    {
        playerGoldText.text = PlayerData.Instance.gold.ToString() + "g";
    }
}
