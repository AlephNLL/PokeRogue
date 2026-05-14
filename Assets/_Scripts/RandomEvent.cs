using GameData;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "RandomEvent", menuName = "Scriptable Objects/RandomEvent")]
public class RandomEvent : ScriptableObject
{
    public string eventText;
    public Sprite defaultIcon;
    public Events[] eventConfirmEffect;
    public string confirmText;
    public Sprite confirmIcon;
    public Events[] eventCancelEffect;
    public string cancelText;
    public Sprite cancelIcon;

    public int goldToGive;
    public bool checkGoldToConfirm;
    public Item[] itemsToGive;
    public bool giveRandomItem;
    public Status statusToApply;
}
