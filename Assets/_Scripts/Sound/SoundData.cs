using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "NuevoSonido", menuName = "Audio/Sound Data")]
public class SoundData : ScriptableObject
{
    public string soundName;
    public AudioClip[] clips;

    public AudioMixerGroup mixerGroup;

    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;

    [Range(0f, 0.5f)] public float randomVolume = 0.05f;
    [Range(0f, 0.5f)] public float randomPitch = 0.1f;

    public bool loop = false;

    public AudioClip GetRandomClip()
    {
        if (clips == null || clips.Length == 0) return null;
        return clips[Random.Range(0, clips.Length)];
    }
}
