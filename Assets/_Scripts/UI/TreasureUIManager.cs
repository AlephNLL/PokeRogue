using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasureUIManager : MonoBehaviour
{
    public static TreasureUIManager instance;
    [SerializeField] GameObject itemDescBox;
    [SerializeField] GameObject itemIcon;
    [SerializeField] TMP_Text itemText;

    public float itemVerticalStartPos;
    private void Awake()
    {
        instance = this;
    }
    public void ShowItem(Item item)
    {
        StartCoroutine(ItemAnimation(item));
    }

    IEnumerator ItemAnimation(Item item)
    {
        Instantiate(item.icon, itemIcon.transform);

        GridLayoutGroup itemSlotGrid = itemIcon.GetComponent<GridLayoutGroup>();

        itemIcon.GetComponent<RectTransform>().position = new Vector3 (960, itemVerticalStartPos, 0);
        Vector3 startPos = itemIcon.GetComponent<RectTransform>().position;
        Vector3 target = new Vector3(960, 940, 0);

        itemSlotGrid.cellSize = new Vector2 (40, 40);
        Vector2 startSize = itemSlotGrid.cellSize;
        Vector3 targetSize = new Vector2(400, 400);

        Destroy(itemIcon.GetComponent<InventoryTooltip>());

        float t = 0;
        while (t < 1)
        {
            itemSlotGrid.cellSize = Vector2.Lerp(startSize,targetSize,t);
            itemIcon.GetComponent<RectTransform>().position = Vector3.Lerp (startPos, target, t);
            t += 2*Time.deltaTime;
            yield return null;
        }

        itemDescBox.SetActive(true);

        itemIcon.GetComponent<RectTransform>().position = target;
        itemSlotGrid.cellSize = targetSize;

        itemText.text = item.description;
    }
}
