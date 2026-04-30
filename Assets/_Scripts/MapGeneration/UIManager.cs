using System.Collections;
using System.Collections.Generic;
using GameData;
using NUnit.Framework;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Canvas canvas;

    [Header("Inventory")]
    public GameObject inventory;
    public GameObject slot;

    public List<GameObject> instantiatedItems;

    [Header("Stats")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI healthText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI attackText;
    public TextMeshProUGUI speedText;

    private void Start()
    {
        Instance = this;
    }

    public void ShowCanvas(bool state)
    {
        if (canvas != null)
        {
            if (state)
            {
                StartCoroutine(WaitAndShow(2f, canvas));
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
        ShowItems();
    }

    private void ShowItems()
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
            GameObject newSlot = Instantiate(slot);
            newSlot.transform.parent = inventory.transform;
            newSlot.transform.localScale = Vector3.one;

            GameObject itemIcon = Instantiate(item.icon);
            itemIcon.transform.parent = newSlot.transform;
            itemIcon.transform.localScale = Vector3.one;

            instantiatedItems.Add(newSlot);
        }
    }

    public void UpdateStats(int index)
    {
        string health = PlayerData.teamData[index].prefab.GetComponent<Unit>().GetRawStat(Stats.HP, PlayerData.teamData[index].level).ToString();
        string defense = PlayerData.teamData[index].prefab.GetComponent<Unit>().GetRawStat(Stats.DEF, PlayerData.teamData[index].level).ToString();
        string attack = PlayerData.teamData[index].prefab.GetComponent<Unit>().GetRawStat(Stats.ATK, PlayerData.teamData[index].level).ToString();
        string speed = PlayerData.teamData[index].prefab.GetComponent<Unit>().GetRawStat(Stats.SPEED, PlayerData.teamData[index].level).ToString();
        string name = PlayerData.teamData[index].name;
        string level = PlayerData.teamData[index].level.ToString();

        nameText.text = name;
        levelText.text = "Lvl: " + level;
        defenseText.text = "Defense: " + defense;
        attackText.text = "Attack: " + attack;
        healthText.text = "Health: " + health;
        speedText.text = "Speed: " + speed;
    }
}
