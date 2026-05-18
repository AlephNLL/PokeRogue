using UnityEngine;

public class AudioLibrary : MonoBehaviour
{
    public static AudioLibrary instance;
    public SoundData combatMusic;
    public SoundData daycareMusic;
    public SoundData shopMusic;

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
}
