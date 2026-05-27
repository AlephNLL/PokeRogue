using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject itemDescriptionBox;
    public string itemDescription;

    private GameObject openedToolTip;
    private GameObject item;

    private bool spawned = false;
    private Vector2 tooltipOffset = new Vector2(0, 100);

    private void Update()
    {
        if (item == null & spawned) DestroyTooltip();
    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        Debug.Log("Cursor Entering " + name + " GameObject");

        item = pointerEventData.pointerCurrentRaycast.gameObject;

        if (spawned) return;
        else
        {
            spawned = true;

            GameObject tooltip = Instantiate(itemDescriptionBox, transform.root);
            RectTransform rt = tooltip.GetComponent<RectTransform>();
            rt.position = pointerEventData.position + tooltipOffset;
            tooltip.transform.SetParent(GetComponentInParent<Canvas>().transform);

            tooltip.GetComponentInChildren<TMP_Text>().SetText(itemDescription);

            openedToolTip = tooltip;
        }
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        //Output the following message with the GameObject's name
        Debug.Log("Cursor Exiting " + name + " GameObject");
        DestroyTooltip();
    }

    public void DestroyTooltip()
    {
        spawned = false;
        Destroy(openedToolTip);
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (!spawned) return;
        DestroyTooltip();
    }

}
