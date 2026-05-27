using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject itemDescriptionBox;
    private GameObject openedToolTip;
    
    //Detect if the Cursor starts to pass over the GameObject
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        Debug.Log("Cursor Entering " + name + " GameObject");
        //GameObject tooltip = Instantiate(itemDescriptionBox, transform.root);
        //RectTransform rt = tooltip.GetComponent<RectTransform>();
        //rt.anchoredPosition = pointerEventData.position;
        //tooltip.transform.parent = GetComponentInParent<Canvas>().transform;

        //openedToolTip = tooltip;



    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        //Output the following message with the GameObject's name
        Debug.Log("Cursor Exiting " + name + " GameObject");
        Destroy(openedToolTip);
    }
}
