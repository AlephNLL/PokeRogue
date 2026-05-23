using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TutorialState
{
    Welcome,
    ExplainAttack,
    ExplainItem,
    ExplainDefend,
    ExplainStats,
    WaitForAttackClick,
    ShowSurprise,
    Finished
}

[Serializable]
public class TutorialStep
{
    [TextArea(2, 4)] public string message;
    public bool waitForPlayerAction;
    public HandPose handPose;
    public string targetKey; // "None", "Stats", o "Attack"
    public Vector3 handOffset;
}

public class BattleTutorialManager : MonoBehaviour
{
    public static BattleTutorialManager instance;

    public TutorialState currentState = TutorialState.Welcome;

    [Header("Referencias de la Burbuja UI")]
    [SerializeField] private GameObject dialogueBubbleParent;
    [SerializeField] private TMP_Text bubbleText;

    [Header("Configuración Typewriter")]
    [SerializeField] private float typingSpeed = 0.03f;
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

    // Aquí almacenamos el guión
    private Dictionary<TutorialState, TutorialStep> tutorialSteps = new Dictionary<TutorialState, TutorialStep>();

    // Referencias dinámicas de la UI del bicho
    private RectTransform cachedPlayerStatsUI;
    private RectTransform cachedPlayerAttackButtonUI;
    private RectTransform cachedPlayerItemButtonUI;
    private RectTransform cachedPlayerDefendButtonUI;
    private RectTransform cachedPlayerStatsButtonUI;

    private void Awake()
    {
        instance = this;
        SetupSteps();
    }

    private void Start()
    {
        // Forzamos que la burbuja empiece oculta por si acaso
        dialogueBubbleParent.SetActive(false);

        // Iniciamos el primer paso
        SetState(TutorialState.Welcome);
    }

    // El bicho llama a esta función cuando aparece en combate
    public void RegisterPlayerUI(RectTransform stats, RectTransform attackButton)
    {
        cachedPlayerStatsUI = stats;
        cachedPlayerAttackButtonUI = attackButton;

        // Actualizamos la mano por si estaba esperando que apareciera el bicho
        UpdateHandActor(tutorialSteps[currentState]);
    }

    // --- EL GUIÓN DEL TUTORIAL ---
    private void SetupSteps()
    {
        tutorialSteps[TutorialState.Welcome] = new TutorialStep
        {
            message = "Hello! I wil be your <color=#FFD700>right hand</color> this fight. Lets cover the basics.",
            waitForPlayerAction = false,
            handPose = HandPose.Idle,
            targetKey = "None"
        };

        tutorialSteps[TutorialState.ExplainAttack] = new TutorialStep
        {
            message = "This button opens de attack menu, where you can choose an ability",
            waitForPlayerAction = false,
            handPose = HandPose.Point,
            targetKey = "ExplainAttack",
            handOffset = new Vector3(400f, -10f, 0f) 
        };

        tutorialSteps[TutorialState.ExplainItem] = new TutorialStep
        {
            message = "Here you can use your consumables",
            waitForPlayerAction = false,
            handPose = HandPose.Point,
            targetKey = "ExplainItem",
            handOffset = new Vector3(400f, -10f, 0f)
        };

        tutorialSteps[TutorialState.ExplainDefend] = new TutorialStep
        {
            message = "The <color=#FFD700>Defend</color> button makes your unit take half damage until its next turn",
            waitForPlayerAction = false,
            handPose = HandPose.Point,
            targetKey = "ExplainDefend",
            handOffset = new Vector3(400f, -10f, 0f)
        };

        tutorialSteps[TutorialState.ExplainStats] = new TutorialStep
        {
            message = "You can also access you stats pressing tab",
            waitForPlayerAction = true,
            handPose = HandPose.Idle,
            targetKey = "ExplainStats",
        };

        tutorialSteps[TutorialState.WaitForAttackClick] = new TutorialStep
        {
            message = "Try choosing the attack button!",
            waitForPlayerAction = true, // Espera que el jugador ataque, oculta la flecha
            handPose = HandPose.Point,
            targetKey = "ExplainAttack",
            handOffset = new Vector3(400f, -10, 0f) 
        };

        tutorialSteps[TutorialState.ShowSurprise] = new TutorialStep
        {
            message = "Wow! You hand-led that very well!",
            waitForPlayerAction = false,
            handPose = HandPose.Idle,
            targetKey = "None"
        };
    }

    public void SetState(TutorialState newState)
    {
        currentState = newState;

        if (currentState == TutorialState.Finished)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[currentState];

        // 1. Efecto "Pop" Cartoon
        dialogueBubbleParent.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(PopBubbleAnimation());

        // 2. Typewriter Effect
        if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
        typewriterCoroutine = StartCoroutine(TypeTextRoutine(step.message, step.waitForPlayerAction));

        // 3. Posicionar la mano
        UpdateHandActor(step);
    }

    private IEnumerator TypeTextRoutine(string text, bool waitingForAction)
    {
        isTyping = true;

        bubbleText.text = text;
        bubbleText.ForceMeshUpdate();

        int totalCharacters = bubbleText.textInfo.characterCount;
        bubbleText.maxVisibleCharacters = 0;

        for (int i = 0; i <= totalCharacters; i++)
        {
            bubbleText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }

        FinishTyping(waitingForAction);
    }

    private void FinishTyping(bool waitingForAction)
    {
        isTyping = false;
        bubbleText.maxVisibleCharacters = bubbleText.textInfo.characterCount;
    }

    public void OnScreenClicked()
    {
        if (currentState == TutorialState.Finished) return;

        TutorialStep step = tutorialSteps[currentState];

        if (isTyping)
        {
            // Autocompletar texto instantáneamente
            if (typewriterCoroutine != null) StopCoroutine(typewriterCoroutine);
            FinishTyping(step.waitForPlayerAction);
        }
        else if (!step.waitForPlayerAction)
        {
            // Avanzar al siguiente diálogo
            SetState(currentState + 1);
        }
    }

    public void PlayerPerformedAction(TutorialState expectedState)
    {
        // Solo avanza si la acción coincide con lo que el tutorial pedía
        if (currentState == expectedState)
        {
            SetState(currentState + 1);
        }
    }

    private IEnumerator PopBubbleAnimation()
    {
        Transform bubbleTransform = dialogueBubbleParent.transform;
        bubbleTransform.localScale = Vector3.zero;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 6f; // Velocidad del pop
            // Fórmula simple de rebote elástico
            float scale = Mathf.Lerp(0f, 1.05f, Mathf.Sin(t * Mathf.PI / 2));
            if (t > 0.8f) scale = Mathf.Lerp(1.05f, 1f, (t - 0.8f) * 5f);

            bubbleTransform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }

        bubbleTransform.localScale = Vector3.one;
    }

    private void UpdateHandActor(TutorialStep step)
    {
        if (TutorialHandActor.instance == null) return;

        if (step.targetKey == "None")
        {
            TutorialHandActor.instance.gameObject.SetActive(true);
            TutorialHandActor.instance.ChangePose(step.handPose);
        }
        else if (step.targetKey == "ExplainAttack" && cachedPlayerAttackButtonUI != null)
        {
            TutorialHandActor.instance.PointAt(cachedPlayerAttackButtonUI, step.handOffset, step.handPose);
        }
        else if (step.targetKey == "ExplainItem" && cachedPlayerItemButtonUI != null)
        {
            TutorialHandActor.instance.PointAt(cachedPlayerItemButtonUI, step.handOffset, step.handPose);
        }
        else if (step.targetKey == "ExplainDefend" && cachedPlayerDefendButtonUI != null)
        {
            TutorialHandActor.instance.PointAt(cachedPlayerDefendButtonUI, step.handOffset, step.handPose);
        }
        else if (step.targetKey == "ExplainStats" && cachedPlayerStatsButtonUI != null)
        {
            TutorialHandActor.instance.PointAt(cachedPlayerStatsButtonUI, step.handOffset, step.handPose);
        }
        else if (step.targetKey == "Attack" && cachedPlayerAttackButtonUI != null)
        {
            TutorialHandActor.instance.PointAt(cachedPlayerAttackButtonUI, step.handOffset, step.handPose);
        }
        else
        {
            // Si la UI aún no ha cargado, la ocultamos por seguridad
            TutorialHandActor.instance.Hide();
        }

        dialogueBubbleParent.transform.position = TutorialHandActor.instance.transform.position + new Vector3(0, -50f, 0f);
    }

    private void EndTutorial()
    {
        dialogueBubbleParent.SetActive(false);
        if (TutorialHandActor.instance != null) TutorialHandActor.instance.Hide();
    }
}