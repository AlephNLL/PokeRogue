using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class SelectUnit : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private GameObject selectedUnit;
    private bool spawned = false;

    private DaycareManager DaycareManager;
    private TBBS TBBS;

    private void OnEnable()
    {
        DaycareManager = null;
        TBBS = null;

        if (SceneManager.GetActiveScene().name == "Daycare") DaycareManager = FindFirstObjectByType<DaycareManager>();
        if (SceneManager.GetActiveScene().name == "BattleScene") TBBS = FindFirstObjectByType<TBBS>();
    }

    private void Update()
    {
        if (selectedUnit) print(selectedUnit.name);
    }
    public void OnPointerEnter(PointerEventData pointerEventData)
    {
        if (spawned) return;
        Unit unit = pointerEventData.pointerCurrentRaycast.gameObject.GetComponentInParent<Unit>();
        if (!unit) return;

        spawned = true;

        if (SceneManager.GetActiveScene().name == "Daycare")
        {
            DaycareManager.hoveredPrefab = pointerEventData.pointerCurrentRaycast.gameObject;
        }

        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            TBBS.hoveredUnit = pointerEventData.pointerCurrentRaycast.gameObject.GetComponent<Unit>();
        }

        print("Cursor Entering " + pointerEventData.pointerCurrentRaycast.gameObject);
    }

    //Detect when Cursor leaves the GameObject
    public void OnPointerExit(PointerEventData pointerEventData)
    {
        if (SceneManager.GetActiveScene().name == "Daycare")
        {
            DaycareManager.hoveredPrefab = null;
        }

        if (SceneManager.GetActiveScene().name == "BattleScene")
        {
            TBBS.hoveredUnit = null;
        }

        spawned = false;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        if (!spawned) return;
    }

}
