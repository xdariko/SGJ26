using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TextTypingAudio : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] typingClips;

    [SerializeField] private float minInterval = 0.03f;
    [SerializeField] private float pitchMin = 0.96f;
    [SerializeField] private float pitchMax = 1.04f;
    [SerializeField] private bool skipWhitespace = true;

    private float nextPlayTime;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void PlayChar(char c)
    {
        if (audioSource == null) return;
        if (typingClips == null || typingClips.Length == 0) return;
        if (skipWhitespace && char.IsWhiteSpace(c)) return;
        if (Time.unscaledTime < nextPlayTime) return;

        AudioClip clip = GetRandomClip();
        if (clip == null) return;

        audioSource.pitch = Random.Range(pitchMin, pitchMax);
        audioSource.PlayOneShot(clip);

        nextPlayTime = Time.unscaledTime + minInterval;
    }

    private AudioClip GetRandomClip()
    {
        if (typingClips == null || typingClips.Length == 0)
            return null;

        int index = Random.Range(0, typingClips.Length);
        return typingClips[index];
    }
}