using GameData;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "RandomEvent", menuName = "Scriptable Objects/RandomEvent")]
public class RandomEvent : ScriptableObject
{
    public string eventText;
    public Sprite icon;
    public Events eventConfirmEffect;
    public string confirmText;
    public Events eventCancelEffect;
    public string cancelText;

    public int goldToGive;
    public Item itemToGive;
    public Status statusToApply;
}
