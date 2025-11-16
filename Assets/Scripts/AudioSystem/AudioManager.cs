using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// USAGE EXAMPLES:
// ===========================
//AudioManager.instance.PlayBGM("DreamTheme");
//AudioManager.instance.PlaySFX("Click");
//AudioManager.instance.PlayVoice("Dulcinea_Happy");

//AudioManager.instance.BGMVolume = 0.5f;
//AudioManager.instance.MasterVolume = 0.8f;
//AudioManager.instance.StopBGMWithFade(1f);

// CROSSFADE DEMO:
// ===========================
//AudioManager.instance.PlayBGM("RainyDay");
//yield return new WaitForSeconds(10);
//AudioManager.instance.PlayBGM("Sunrise"); // Smooth crossfade


public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Audio Mixer")]
    public AudioMixer masterMixer;
    public AudioMixerGroup bgmGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup characterGroup;

    [Header("Audio Lists")]
    public List<AudioClipData> BGMList;
    public List<AudioClipData> SFXList;
    public List<AudioClipData> CharacterVoiceList;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float MasterVolume = 1f;
    [Range(0f, 1f)] public float BGMVolume = 1f;
    [Range(0f, 1f)] public float CharacterVolume = 1f;
    [Range(0f, 1f)] public float SFXVolume = 1f;

    private AudioSource currentBGMSource;
    private AudioSource nextBGMSource;

    private readonly List<AudioSource> activeSFX = new();
    private readonly List<AudioSource> activeVoices = new();

    private Coroutine crossfadeRoutine;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Setup sources
        currentBGMSource = gameObject.AddComponent<AudioSource>();
        nextBGMSource = gameObject.AddComponent<AudioSource>();

        currentBGMSource.outputAudioMixerGroup = bgmGroup;
        nextBGMSource.outputAudioMixerGroup = bgmGroup;
    }

    void Update()
    {
        // Continuously sync volume
        masterMixer.SetFloat("MasterVol", LinearToDecibel(MasterVolume));
        masterMixer.SetFloat("BGMVol", LinearToDecibel(BGMVolume));
        masterMixer.SetFloat("SFXVol", LinearToDecibel(SFXVolume));
        masterMixer.SetFloat("CharacterVol", LinearToDecibel(CharacterVolume));
    }

    // ===========================
    // BGM with Crossfade
    // ===========================
    public void PlayBGM(string name)
    {
        var data = FindClipData(name, BGMList);
        if (data == null) return;

        if (crossfadeRoutine != null)
            StopCoroutine(crossfadeRoutine);

        crossfadeRoutine = StartCoroutine(CrossfadeBGM(data));
    }

    private IEnumerator CrossfadeBGM(AudioClipData newClip)
    {
        // Prepare next source
        nextBGMSource.clip = newClip.clip;
        nextBGMSource.pitch = newClip.pitch;
        nextBGMSource.loop = newClip.loop;
        nextBGMSource.volume = 0f;
        nextBGMSource.Play();

        float duration = Mathf.Max(newClip.fadeInTime, 0.1f);
        float time = 0f;
        float startVol = currentBGMSource.volume;

        // Fade between sources
        while (time < duration)
        {
            float t = time / duration;
            currentBGMSource.volume = Mathf.Lerp(startVol, 0f, t);
            nextBGMSource.volume = Mathf.Lerp(0f, newClip.defaultVolume, t);
            time += Time.deltaTime;
            yield return null;
        }

        currentBGMSource.Stop();

        // Swap sources
        var temp = currentBGMSource;
        currentBGMSource = nextBGMSource;
        nextBGMSource = temp;
    }

    public void StopBGMWithFade(float fadeOut = 0.5f)
    {
        StartCoroutine(FadeOutBGM(fadeOut));
    }

    private IEnumerator FadeOutBGM(float duration)
    {
        float startVol = currentBGMSource.volume;
        float time = 0f;

        while (time < duration)
        {
            currentBGMSource.volume = Mathf.Lerp(startVol, 0f, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        currentBGMSource.Stop();
    }

    // ===========================
    // SFX
    // ===========================
    public void PlaySFX(string name)
    {
        var data = FindClipData(name, SFXList);
        if (data == null) return;

        var src = CreateTempSource(sfxGroup);
        src.clip = data.clip;
        src.pitch = data.pitch;
        src.volume = data.defaultVolume;
        src.loop = data.loop;
        src.Play();

        activeSFX.Add(src);
        Destroy(src.gameObject, data.clip.length + 0.1f);
    }

    // ===========================
    // Character Voice
    // ===========================
    public void PlayCharacter(string name)
    {
        var data = FindClipData(name, CharacterVoiceList);
        if (data == null) return;

        var src = CreateTempSource(characterGroup);
        src.clip = data.clip;
        src.pitch = data.pitch;
        src.volume = data.defaultVolume;
        src.loop = data.loop;
        src.Play();

        activeVoices.Add(src);
        Destroy(src.gameObject, data.clip.length + 0.1f);
    }

    // ===========================
    // Utility
    // ===========================
    private AudioClipData FindClipData(string name, List<AudioClipData> list)
    {
        var data = list.Find(x => x.audioName == name);
        if (data == null)
            Debug.LogWarning($"AudioManager: No clip found for '{name}'");
        return data;
    }

    private AudioSource CreateTempSource(AudioMixerGroup outputGroup)
    {
        var go = new GameObject("TempAudio");
        go.transform.SetParent(transform);
        var src = go.AddComponent<AudioSource>();
        src.outputAudioMixerGroup = outputGroup;
        return src;
    }

    private float LinearToDecibel(float linear)
    {
        return linear <= 0.0001f ? -80f : 20f * Mathf.Log10(linear);
    }
}
