using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [SerializeField] private AudioSource emitterPrefab;
    [SerializeField] private int poolSize = 20;

    [Header("Configuración de Música")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private float tiempoTransicionDefecto = 1.5f;

    private AudioSource activeMusicSource;
    private Coroutine musicFadeCoroutine;

    private List<AudioSource> pool = new List<AudioSource>();

    void Awake()
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

        for (int i = 0; i < poolSize; i++)
        {
            AudioSource newEmitter = Instantiate(emitterPrefab, transform);
            newEmitter.gameObject.SetActive(false);
            pool.Add(newEmitter);
        }

        musicSourceA = pool[0];
        musicSourceB = pool[1];
        musicSourceA.gameObject.SetActive(true);
        musicSourceB.gameObject.SetActive(true);
    }

    // Para sonidos 2D (Música, UI, interfaces)
    public void PlaySound2D(SoundData sound)
    {
        AudioSource emitter = GetAvailableEmitter();
        if (emitter == null) return;

        ConfigureEmitter(emitter, sound);
        emitter.spatialBlend = 0f; // 2D absoluto
        emitter.Play();

        if (!sound.loop) StartCoroutine(DisableEmitterAfterPlay(emitter, sound.GetRandomClip().length));
    }

    // Para sonidos 3D (Golpes, explosiones en el mundo)
    public void PlaySound3D(SoundData sound, Vector3 position)
    {
        AudioSource emitter = GetAvailableEmitter();
        if (emitter == null) return;

        emitter.transform.position = position;
        ConfigureEmitter(emitter, sound);
        emitter.spatialBlend = 1f; // 3D absoluto (atenuación por distancia)
        emitter.Play();

        if (!sound.loop) StartCoroutine(DisableEmitterAfterPlay(emitter, sound.GetRandomClip().length));
    }

    private AudioSource GetAvailableEmitter()
    {
        for (int i = 2; i < pool.Count; i++)
        {
            if (!pool[i].gameObject.activeSelf)
            {
                pool[i].gameObject.SetActive(true);
                return pool[i];
            }
        }
        return null;
    }

    private void ConfigureEmitter(AudioSource emitter, SoundData sound)
    {
        emitter.clip = sound.GetRandomClip();
        emitter.outputAudioMixerGroup = sound.mixerGroup;
        emitter.loop = sound.loop;

        emitter.volume = sound.volume + Random.Range(-sound.randomVolume, sound.randomVolume);
        emitter.pitch = sound.pitch + Random.Range(-sound.randomPitch, sound.randomPitch);
    }

    private System.Collections.IEnumerator DisableEmitterAfterPlay(AudioSource emitter, float duration)
    {
        yield return new WaitForSeconds(duration);
        emitter.gameObject.SetActive(false);
    }

    public void PlayMusic(SoundData newMusic, float fadeDuration = -1f)
    {
        if (newMusic == null || newMusic.clips.Length == 0) return;

        AudioClip clipToPlay = newMusic.clips[0]; 

        if (activeMusicSource != null && activeMusicSource.clip == clipToPlay) return;

        float duration = fadeDuration < 0 ? tiempoTransicionDefecto : fadeDuration;

        AudioSource targetSource = (activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;

        targetSource.clip = clipToPlay;
        targetSource.outputAudioMixerGroup = newMusic.mixerGroup;
        targetSource.loop = newMusic.loop;

        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(CrossfadeMusic(targetSource, newMusic.volume, duration));
    }

    private System.Collections.IEnumerator CrossfadeMusic(AudioSource targetSource, float targetVolume, float duration)
    {
        if (targetSource != null)
        {
            targetSource.volume = 0f;
            targetSource.Play();
        }

        float time = 0f;
        float startSourceVolume = activeMusicSource != null ? activeMusicSource.volume : 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            if (targetSource != null)
            {
                targetSource.volume = Mathf.Lerp(0f, targetVolume, t);
            }

            if (activeMusicSource != null)
            {
                activeMusicSource.volume = Mathf.Lerp(startSourceVolume, 0f, t);
            }

            yield return null;
        }

        if (targetSource != null)
        {
            targetSource.volume = targetVolume;
        }

        if (activeMusicSource != null)
        {
            activeMusicSource.Stop();
            activeMusicSource.volume = 0f;
        }

        activeMusicSource = targetSource;
    }
    public void StopMusic(float fadeDuration = 1f)
    {
        if (activeMusicSource == null) return;
        if (musicFadeCoroutine != null) StopCoroutine(musicFadeCoroutine);

        musicFadeCoroutine = StartCoroutine(CrossfadeMusic(null, 0f, fadeDuration));
    }
}