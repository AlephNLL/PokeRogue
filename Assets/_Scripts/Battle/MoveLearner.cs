using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveLearner : MonoBehaviour
{
    public static MoveLearner instance;
    public GameObject moveLearnUI;
    public TMP_Text nameText;
    public Image icon;
    public Button[] abilityButtons;
    public GameObject newAbility;
    public List<LearnPrompt> prompts = new List<LearnPrompt>();

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
    public void LearnMove(UnitData unitData, int level)
    {
        if(level > 10) return;
        Unit unit = unitData.prefab.GetComponent<Unit>();

        if (unitData.knownAbilities.Count < 4)
        {
            unitData.knownAbilities.Add(unit.abilityPool[level]);
        }
        else if (!unitData.knownAbilities.Contains(unit.abilityPool[level]))
        {
            PromptChangeMoves(new LearnPrompt(unitData,level));
        }
    }

    void PromptChangeMoves(LearnPrompt prompt)
    {
        prompts.Add(prompt);

        if (!moveLearnUI.activeSelf)
        {
            ShowNextPrompt();
        }
    }

    void ShowNextPrompt()
    {
        if (prompts.Count == 0)
        {
            moveLearnUI.SetActive(false);
            return;
        }

        moveLearnUI.SetActive(true);

        LearnPrompt currentPrompt = prompts[0];
        Unit unit = currentPrompt.unitData.prefab.GetComponent<Unit>();
        nameText.text = unit.name;
        icon.sprite = unit.icon;
        Abilities abilityToLearn = unit.abilityPool[currentPrompt.level];

        // Actualizamos los textos de la NUEVA habilidad
        newAbility.transform.GetChild(0).GetComponent<TMP_Text>().text = abilityToLearn.description;
        newAbility.transform.GetChild(1).GetComponent<TMP_Text>().text = abilityToLearn.stance.ToString();
        newAbility.transform.GetChild(2).GetComponent<TMP_Text>().text = abilityToLearn.accuracy.ToString();
        newAbility.transform.GetChild(3).GetComponent<TMP_Text>().text = abilityToLearn.power.ToString();
        newAbility.transform.GetChild(4).GetComponent<TMP_Text>().text = abilityToLearn.name;
        newAbility.transform.GetChild(5).GetComponent<Image>().enabled = abilityToLearn.mustUseStance;

        // Actualizamos los botones de las habilidades EXISTENTES
        for (int i = 0; i < currentPrompt.unitData.knownAbilities.Count; i++)
        {
            abilityButtons[i].gameObject.SetActive(true);
            Abilities knownAbility = currentPrompt.unitData.knownAbilities[i];

            abilityButtons[i].transform.GetChild(0).GetComponent<TMP_Text>().text = knownAbility.description;
            abilityButtons[i].transform.GetChild(1).GetComponent<TMP_Text>().text = knownAbility.stance.ToString();
            abilityButtons[i].transform.GetChild(2).GetComponent<TMP_Text>().text = knownAbility.accuracy.ToString();
            abilityButtons[i].transform.GetChild(3).GetComponent<TMP_Text>().text = knownAbility.power.ToString();
            abilityButtons[i].transform.GetChild(4).GetComponent<TMP_Text>().text = knownAbility.name;
            abilityButtons[i].transform.GetChild(5).GetComponent<Image>().enabled = knownAbility.mustUseStance;
        }
    }

    public void ReplaceMove(int buttonSlotIndex)
    {
        if (prompts.Count == 0) return;

        LearnPrompt currentPrompt = prompts[0];
        Unit unit = currentPrompt.unitData.prefab.GetComponent<Unit>();
        Abilities abilityToLearn = unit.abilityPool[currentPrompt.level];

        currentPrompt.unitData.knownAbilities[buttonSlotIndex] = abilityToLearn;

        prompts.RemoveAt(0);

        ShowNextPrompt();
    }

    public void CancelLearning()
    {
        if (prompts.Count == 0) return;

        prompts.RemoveAt(0);
        ShowNextPrompt();
    }
}

public class LearnPrompt
{
    public UnitData unitData;
    public int level;

    public LearnPrompt(UnitData unitData, int level)
    {
        this.unitData = unitData;
        this.level = level;
    }
}