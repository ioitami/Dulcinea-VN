using UnityEngine;

[CreateAssetMenu(fileName = "NewAudioClipData", menuName = "Audio/Audio Clip Data")]
public class AudioClipData : ScriptableObject
{
    [Header("Audio Metadata")]
    public string audioName;
    public AudioClip clip;

    [Header("Playback Settings")]
    [Range(0f, 2f)] public float defaultVolume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;
    public bool loop = false;

    [Header("Fade Options")]
    public float fadeInTime = 0.5f;
    public float fadeOutTime = 0.5f;
}
