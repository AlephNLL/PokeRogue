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
    WaitForAttackClick,
    ExplainAbilities,
    ExplainStats,
    ExplainPassives,
    ExplainActives,
    WaitForAbility,
    Targeting,
    ShowSurprise,
    ExplainColors,
    ExplainStance,
    ExplainWeakness,
    StanceLock,
    Ending,
    Finished
}

[Serializable]
public class TutorialStep
{
    [TextArea(2, 4)] public string message;
    public bool waitForPlayerAction;
    public bool disableBackgroundButton = false;
    public bool pauseGame = false;
    public HandPose handPose;
    public string targetKey;
    public Vector3 handOffset;
}

public class BattleTutorialManager : MonoBehaviour
{
    public static BattleTutorialManager instance;

    public TutorialState currentState = TutorialState.Welcome;

    [Header("Referencias de UI")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private GameObject dialogueBubbleParent;
    [SerializeField] private TMP_Text bubbleText;

    [Header("Configuración Typewriter")]
    [SerializeField] private float typingSpeed = 0.03f;
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;

    [Header("Invisible button")]
    [SerializeField] private GameObject invisibleButton;

    // Aquí almacenamos el guión
    private Dictionary<TutorialState, TutorialStep> tutorialSteps = new Dictionary<TutorialState, TutorialStep>();

    // Referencias dinámicas de la UI del bicho
    private RectTransform cachedPlayerAttackButtonUI;
    private RectTransform cachedPlayerItemButtonUI;
    private RectTransform cachedPlayerDefendButtonUI;

    private RectTransform cachedPlayerActiveButtonUI;
    private RectTransform cachedPlayerPassiveButtonUI;

    private void Awake()
    {
        instance = this;
        SetupSteps();
    }

    private void Start()
    {
        if (!PlayerData.tutorial) Destroy(gameObject);
    }
    public void StartTutorial()
    {
        if (currentState != TutorialState.Welcome) return;

        tutorialCanvas.SetActive(true);
        // Forzamos que la burbuja empiece oculta por si acaso
        dialogueBubbleParent.SetActive(false);

        // Iniciamos el primer paso
        SetState(TutorialState.Welcome);
    }

    // El bicho llama a esta función cuando aparece en combate
    public void RegisterPlayerUI(RectTransform[] buttons)
    {
        if (currentState == TutorialState.Finished) return;
        cachedPlayerAttackButtonUI = buttons[0];
        cachedPlayerItemButtonUI = buttons[1];
        cachedPlayerDefendButtonUI = buttons[2];
        cachedPlayerActiveButtonUI = buttons[3];
        cachedPlayerPassiveButtonUI = buttons[4];

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
            message = "This button opens the <color=#FF0000>Attack</color> menu, where you can choose an ability",
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
            message = "The <color=#0000FF>Defend</color> button makes your unit take half damage until its next turn",
            waitForPlayerAction = false,
            handPose = HandPose.Point,
            targetKey = "ExplainDefend",
            handOffset = new Vector3(400f, -10f, 0f)
        };

        tutorialSteps[TutorialState.WaitForAttackClick] = new TutorialStep
        {
            message = "Try choosing the <color=#FF0000>Attack</color> button!",
            waitForPlayerAction = true, // Espera que el jugador ataque, oculta la flecha
            disableBackgroundButton = true,
            handPose = HandPose.Point,
            targetKey = "ExplainAttack",
            handOffset = new Vector3(400f, -10, 0f)
        };

        tutorialSteps[TutorialState.ExplainAbilities] = new TutorialStep
        {
            message = "These are your abilities",
            waitForPlayerAction = false, // Espera que el jugador ataque, oculta la flecha
            handPose = HandPose.Idle,
            targetKey = "None",
            handOffset = new Vector3(400f, -10, 0f)
        };

        tutorialSteps[TutorialState.ExplainStats] = new TutorialStep
        {
            message = "You can access a more detailed explanation of your abilities and stats with TAB.",
            waitForPlayerAction = false,
            handPose = HandPose.Idle,
            targetKey = "None",
        };

        tutorialSteps[TutorialState.ExplainPassives] = new TutorialStep
        {
            message = "Abilities can be <color=#00FFFF>passive</color>, like this one. <color=#00FFFF>Passives</color> trigger automatically on battle.",
            waitForPlayerAction = false,
            handPose = HandPose.Point,
            targetKey = "Passives",
            handOffset = new Vector3(400f, -10, 0f)
        };

        tutorialSteps[TutorialState.ExplainActives] = new TutorialStep
        {
            message = "Or <color=#FF0000>active</color> like this one. <color=#FF0000>Actives</color> are your main damage output.",
            waitForPlayerAction = false,
            handPose = HandPose.Point,
            targetKey = "Actives",
            handOffset = new Vector3(400f, -10, 0f)
        };

        tutorialSteps[TutorialState.WaitForAbility] = new TutorialStep
        {
            message = "Choose your <color=#FF0000>active</color> ability!",
            waitForPlayerAction = true,
            disableBackgroundButton = true,
            handPose = HandPose.Point,
            targetKey = "Actives",
            handOffset = new Vector3(400f, -10, 0f)
        };

        tutorialSteps[TutorialState.Targeting] = new TutorialStep
        {
            message = "Now you have to chose your target, press space once you're ready!",
            waitForPlayerAction = true,
            handPose = HandPose.Idle,
            targetKey = "None",
        };

        tutorialSteps[TutorialState.ShowSurprise] = new TutorialStep
        {
            message = "Wow! You <color=#FFD700>hand-led</color> that very well!",
            waitForPlayerAction = false,
            pauseGame = true,
            handPose = HandPose.Idle,
            targetKey = "None"
        };

        tutorialSteps[TutorialState.ExplainColors] = new TutorialStep
        {
            message = "One last thing. See those colors on the rim of the base? Those colors indicate stances.",
            waitForPlayerAction = false,
            pauseGame = true,
            handPose = HandPose.Idle,
            targetKey = "None"
        };

        tutorialSteps[TutorialState.ExplainStance] = new TutorialStep
        {
            message = "There are 5 stances: <color=#FF0000>Agressive</color> (raises attack), <color=#0000FF>Defensive</color> (raises defense), " +
            "<color=#00FF00>Agile</color> (raises speed), <color=#00FFFF>Cautious</color> (user is inmune to abilities with 0 power), <color=#FF00FF>Tricky</color> (increases secondary effect chance)",
            waitForPlayerAction = false,
            pauseGame = true,
            handPose = HandPose.Idle,
            targetKey = "None"
        };

        tutorialSteps[TutorialState.ExplainWeakness] = new TutorialStep
        {
            message = "<color=#FF0000>Agressive</color> stance is weak to <color=#0000FF>Defensive</color> abilities. <color=#0000FF>Defensive</color> stance is weak to <color=#00FF00>Agile</color> abilities" +
            ". And <color=#00FF00>Agile</color> stance is weak to <color=#FF0000>Agressive</color> abilities.",
            waitForPlayerAction = false,
            pauseGame = true,
            handPose = HandPose.Idle,
            targetKey = "None"
        };

        tutorialSteps[TutorialState.StanceLock] = new TutorialStep
        {
            message = "Some abilities can only be used in their stance. Stance locked abilities are marked with a lock on the summary.",
            waitForPlayerAction = false,
            pauseGame = true,
            handPose = HandPose.Idle,
            targetKey = "None"
        };

        tutorialSteps[TutorialState.Ending] = new TutorialStep
        {
            message = "That covers all the basics, have fun!",
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

        if(step.disableBackgroundButton) invisibleButton.SetActive(false);
        else invisibleButton.SetActive(true);

        if (step.pauseGame) PauseMenuUI.instance.PauseGame();
        else PauseMenuUI.instance.Resume();
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
            yield return new WaitForSecondsRealtime(typingSpeed);
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
            t += Time.unscaledDeltaTime * 6f; // Velocidad del pop
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
        else if (step.targetKey == "Attack" && cachedPlayerAttackButtonUI != null)
        {
            TutorialHandActor.instance.PointAt(cachedPlayerAttackButtonUI, step.handOffset, step.handPose);
        }
        else if (step.targetKey == "Passives" && cachedPlayerPassiveButtonUI != null)
        {
            TutorialHandActor.instance.PointAt(cachedPlayerPassiveButtonUI, step.handOffset, step.handPose);
        }
        else if (step.targetKey == "Actives" && cachedPlayerActiveButtonUI != null)
        {
            TutorialHandActor.instance.PointAt(cachedPlayerActiveButtonUI, step.handOffset, step.handPose);
        }
        else
        {
            // Si la UI aún no ha cargado, la ocultamos por seguridad
            TutorialHandActor.instance.Hide();
        }

        dialogueBubbleParent.transform.position = TutorialHandActor.instance.transform.position + new Vector3(0, -200f, 0f);
    }

    private void EndTutorial()
    {
        dialogueBubbleParent.SetActive(false);
        invisibleButton.SetActive(false);
        if (TutorialHandActor.instance != null) TutorialHandActor.instance.Hide();
        PlayerData.tutorial = false;

        Destroy(gameObject);
    }
}