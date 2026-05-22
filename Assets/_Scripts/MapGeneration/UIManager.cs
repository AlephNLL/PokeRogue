using System.Collections;
using System.Collections.Generic;
using GameData;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Canvas canvas;

    [Header("Inventario")]
    public GameObject inventory;
    public GameObject slot;
    public GameObject abilitySlot;
    
    public GameObject consumables;
    public GameObject abilities;

    public List<GameObject> instantiatedItems;

    [Header("Stats")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI luckText;

    public int lookAtIndex = 0;

    private void Start()
    {
        Instance = this;
    }

    public void ShowCanvas(bool state, float delay = 2f)
    {
        if (canvas != null)
        {
            if (state)
            {
                StartCoroutine(WaitAndShow(delay, canvas));
            } else
            {
                canvas.enabled = state;
            }
        }
    }

    private IEnumerator WaitAndShow(float seconds, Canvas canvas)
    {
        yield return new WaitForSeconds(seconds);
        canvas.enabled = true;
    }

    private void UpdateInventory()
    {
        if (instantiatedItems != null)
        {
            foreach (GameObject item in instantiatedItems)
            {
                Destroy(item);
            }
            instantiatedItems.Clear();
        }

        instantiatedItems = new List<GameObject>();

        foreach (Item item in PlayerData.Instance.p_items)
        {
            Debug.Log(item.ToString());
            if (item.isConsumible == true) { continue; }

            GameObject newSlot = Instantiate(slot);
            newSlot.transform.parent = inventory.transform;
            newSlot.transform.localScale = Vector3.one;

            GameObject itemIcon = Instantiate(item.icon);
            itemIcon.transform.parent = newSlot.transform;
            itemIcon.transform.localScale = Vector3.one;

            instantiatedItems.Add(newSlot);
        }
    }

    private void UpdateConsumables()
    {
        if (instantiatedItems != null)
        {
            foreach (GameObject item in instantiatedItems)
            {
                Destroy(item);
            }
            instantiatedItems.Clear();
        }

        instantiatedItems = new List<GameObject>();

        foreach (Item item in PlayerData.Instance.p_items)
        {
            if (item.isConsumible == false) { continue; }

            GameObject newSlot = Instantiate(slot);
            newSlot.transform.parent = consumables.transform;
            newSlot.transform.localScale = Vector3.one;

            GameObject itemIcon = Instantiate(item.icon);
            itemIcon.transform.parent = newSlot.transform;
            itemIcon.transform.localScale = Vector3.one;

            instantiatedItems.Add(newSlot);
        }
    }

    public void UpdateAbilities(UnitData mon, int index = 0)
    {
        if (instantiatedItems != null)
        {
            foreach (GameObject item in instantiatedItems)
            {
                Destroy(item);
            }
            instantiatedItems.Clear();
        }

        instantiatedItems = new List<GameObject>();

        UnitData viewMon = mon;

        if (!viewMon) 
        {
            if (PlayerData.teamData.Count > 0)
            {
                viewMon = PlayerData.teamData[index];
            }
            else return;
        } 

        foreach (Abilities ability in viewMon.knownAbilities)
        {
            GameObject newAbility = Instantiate(abilitySlot);
            newAbility.transform.parent = abilities.transform;
            newAbility.transform.localScale = Vector3.one;

            // Magia negra para sacar los gameobject hijos
            foreach (Transform child in newAbility.transform)
            {
                switch (child.gameObject.name)
                {
                    case "Name":
                        child.gameObject.GetComponent<TextMeshProUGUI>().text = ability.name;
                        break;
                    case "Description":
                        child.gameObject.GetComponent<TextMeshProUGUI>().text = ability.description;
                        break;
                    case "Power":
                        child.gameObject.GetComponent<TextMeshProUGUI>().text = ability.power.ToString();
                        break;
                    case "Stance":
                        child.gameObject.GetComponent<TextMeshProUGUI>().text = ability.stance.ToString();
                        break;
                    case "Accuracy":
                        child.gameObject.GetComponent<TextMeshProUGUI>().text = ability.accuracy.ToString();
                        break;
                    case "Stance Lock":
                        child.gameObject.GetComponent<Image>().enabled = ability.mustUseStance;
                        break;
                }
            }

            instantiatedItems.Add(newAbility);
        }
    }

    public void UpdateStats(UnitData mon, int index = 0)
    {
        UnitData viewMon = mon;

        if (!viewMon)
        {
            if (PlayerData.teamData.Count > 0)
            {
                viewMon = PlayerData.teamData[index];
            }
            else return;
        }

        string health = viewMon.prefab.GetComponent<Unit>().GetRawStat(Stats.HP, viewMon.level).ToString();
        string defense = viewMon.prefab.GetComponent<Unit>().GetRawStat(Stats.DEF, viewMon.level).ToString();
        string attack = viewMon.prefab.GetComponent<Unit>().GetRawStat(Stats.ATK, viewMon.level).ToString();
        string speed = viewMon.prefab.GetComponent<Unit>().GetRawStat(Stats.SPEED, viewMon.level).ToString();
        string luck = viewMon.prefab.GetComponent<Unit>().luck.ToString();
        string name = viewMon.name.ToUpper();
        string level = viewMon.level.ToString();

        nameText.text = name;
        levelText.text = "LVL: " + level;
        defenseText.text = "DEF: " + defense;
        attackText.text = "ATK: " + attack;
        healthText.text = "HP: " + health;
        speedText.text = "SPD: " + speed;
        luckText.text = "LCK: " + luck;
    }

    public void ShowInventory()
    {
        inventory.SetActive(true);

        consumables.SetActive(false);
        abilities.SetActive(false);

        UpdateInventory();
    }

    public void ShowConsumables()
    {
        consumables.SetActive(true);

        inventory.SetActive(false);
        abilities.SetActive(false);

        UpdateConsumables();
    }

    public void ShowAbilities()
    {
        abilities.SetActive(true);

        consumables.SetActive(false);
        inventory.SetActive(false);

        UpdateAbilities(null,lookAtIndex);
    }
}
