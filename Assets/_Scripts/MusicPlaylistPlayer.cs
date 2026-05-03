using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlaylistPlayer : MonoBehaviour
{
    [Header("Playlist")]
    [SerializeField] private AudioClip[] tracks;
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool loopPlaylist = true;
    [SerializeField] private bool shuffle = false;

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)] private float volume = 0.7f;

    private AudioSource audioSource;
    private int currentTrackIndex;
    private bool isPlayingPlaylist;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.volume = volume;
    }

    private void Start()
    {
        if (playOnStart)
            PlayPlaylist();
    }

    private void Update()
    {
        if (!isPlayingPlaylist)
            return;

        if (audioSource.isPlaying)
            return;

        if (audioSource.clip == null)
            return;

        PlayNextTrack();
    }

    public void PlayPlaylist()
    {
        if (tracks == null || tracks.Length == 0)
        {
            Debug.LogWarning("MusicPlaylistPlayer: tracks list is empty.", this);
            return;
        }

        isPlayingPlaylist = true;
        currentTrackIndex = 0;

        if (shuffle)
            currentTrackIndex = Random.Range(0, tracks.Length);

        PlayTrack(currentTrackIndex);
    }

    public void PlayNextTrack()
    {
        if (tracks == null || tracks.Length == 0)
            return;

        if (shuffle)
        {
            currentTrackIndex = GetRandomDifferentTrackIndex();
        }
        else
        {
            currentTrackIndex++;

            if (currentTrackIndex >= tracks.Length)
            {
                if (!loopPlaylist)
                {
                    isPlayingPlaylist = false;
                    return;
                }

                currentTrackIndex = 0;
            }
        }

        PlayTrack(currentTrackIndex);
    }

    public void StopPlaylist()
    {
        isPlayingPlaylist = false;
        audioSource.Stop();
    }

    private void PlayTrack(int index)
    {
        if (index < 0 || index >= tracks.Length)
            return;

        AudioClip clip = tracks[index];

        if (clip == null)
        {
            Debug.LogWarning($"MusicPlaylistPlayer: track at index {index} is null.", this);
            PlayNextTrack();
            return;
        }

        audioSource.clip = clip;
        audioSource.volume = volume;
        audioSource.Play();

        Debug.Log($"Now playing: {clip.name}");
    }

    private int GetRandomDifferentTrackIndex()
    {
        if (tracks.Length <= 1)
            return 0;

        int newIndex;

        do
        {
            newIndex = Random.Range(0, tracks.Length);
        }
        while (newIndex == currentTrackIndex);

        return newIndex;
    }
}