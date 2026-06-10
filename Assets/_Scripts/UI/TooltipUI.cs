using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI instance;
    public GameObject tooltipBorder;

    [Header("Prefabs")]
    public GameObject titleTextPrefab;
    public GameObject effectTextPrefab;

    [Header("Time Settings")]
    public float baseDuration = 1.5f;
    public float durationPerEffect = 0.5f;

    private class TooltipAction
    {
        public string title;
        public List<string> effects = new List<string>();
        public bool isClosed = false;
        public bool isForceClosed = false;
    }

    private List<TooltipAction> actionQueue = new List<TooltipAction>();
    private List<GameObject> activeUIElements = new List<GameObject>();

    private TooltipAction currentBuildingAction = null; 
    public bool isProcessing;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNewAction(string title)
    {
        currentBuildingAction = new TooltipAction { title = title };
        actionQueue.Add(currentBuildingAction);

        if (!isProcessing)
        {
            StartCoroutine(ProcessActionQueue());
        }
    }

    public void AddEffectToCurrentAction(string effectDescription)
    {
        if (currentBuildingAction == null)
        {
            StartNewAction("Efecto Activado:");
        }

        currentBuildingAction.effects.Add(effectDescription);

        if (isProcessing && actionQueue.Count > 0 && actionQueue[0] == currentBuildingAction)
        {
            SpawnEffectText(effectDescription);
        }
    }

    public void EndCurrentAction(bool inmediatly = false)
    {
        if (currentBuildingAction != null)
        {
            if (inmediatly)
            {
                currentBuildingAction.isForceClosed = true;
                currentBuildingAction.isClosed = true;
            }
            else
            {
                currentBuildingAction.isClosed = true;
            }

            currentBuildingAction = null;
        }
    }

    IEnumerator ProcessActionQueue()
    {
        isProcessing = true;

        while (actionQueue.Count > 0)
        {
            tooltipBorder.SetActive(true);
            TooltipAction activeAction = actionQueue[0];

            GameObject titleObj = Instantiate(titleTextPrefab, tooltipBorder.transform);
            titleObj.GetComponent<TMP_Text>().text = activeAction.title;
            activeUIElements.Add(titleObj);

            foreach (string fx in activeAction.effects)
            {
                SpawnEffectText(fx);
            }

            float timer = 0f;
            int lastRenderedEffectCount = activeAction.effects.Count;

            float requiredDuration = baseDuration + (lastRenderedEffectCount * durationPerEffect);

            while ((timer < requiredDuration || !activeAction.isClosed) && !activeAction.isForceClosed)
            {
                timer += Time.deltaTime;

                if (activeAction.effects.Count > lastRenderedEffectCount)
                {
                    requiredDuration += (activeAction.effects.Count - lastRenderedEffectCount) * durationPerEffect;
                    lastRenderedEffectCount = activeAction.effects.Count;
                }

                yield return null;
            }

            ClearActiveUI();
            tooltipBorder.SetActive(false);
            actionQueue.RemoveAt(0);

            if (!activeAction.isForceClosed) yield return new WaitForSeconds(0.15f); 
        }
        isProcessing = false;
    }

    private void SpawnEffectText(string text)
    {
        GameObject effectObj = Instantiate(effectTextPrefab, tooltipBorder.transform);
        effectObj.GetComponent<TMP_Text>().text = $"    {text}";
        activeUIElements.Add(effectObj);

        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipBorder.GetComponent<RectTransform>());
    }

    private void ClearActiveUI()
    {
        foreach (GameObject obj in activeUIElements)
        {
            if (obj != null) Destroy(obj);
        }
        activeUIElements.Clear();
    }
}