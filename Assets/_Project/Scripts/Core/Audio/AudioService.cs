using System.Collections;
using UnityEngine;

public class AudioService : MonoBehaviour
{
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfxSource;

    private bool isUsingSourceA = true;
    private AudioClip lastWorldMusic;
    private Coroutine fadeRoutine;
    private bool isMuted;

    public bool IsMuted => isMuted;

    private void Awake()
    {
        if (musicSourceA == null) musicSourceA = gameObject.AddComponent<AudioSource>();
        if (musicSourceB == null) musicSourceB = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSourceA.loop = true;
        musicSourceB.loop = true;
        musicSourceA.playOnAwake = false;
        musicSourceB.playOnAwake = false;
        sfxSource.playOnAwake = false;
    }

    private void Update()
    {
        if (MenuInput.Mute)
        {
            ToggleMute();
        }
    }

    public void ToggleMute()
    {
        isMuted = !isMuted;
        AudioListener.volume = isMuted ? 0f : 1f;
    }

    public void PlayWorldMusic(AudioClip clip)
    {
        if (clip == null) return;
        lastWorldMusic = clip;
        CrossfadeMusic(clip);
    }

    public void PlayBattleMusic(AudioClip clip)
    {
        if (clip == null) return;
        CrossfadeMusic(clip);
    }

    public void ResumeWorldMusic()
    {
        if (lastWorldMusic != null)
        {
            CrossfadeMusic(lastWorldMusic);
        }
    }

    public void PlaySfx(AudioClip clip, float pitchJitter = 0.05f)
    {
        if (clip == null || sfxSource == null) return;
        if (pitchJitter > 0f)
        {
            sfxSource.pitch = Random.Range(1f - pitchJitter, 1f + pitchJitter);
        }
        else
        {
            sfxSource.pitch = 1f;
        }
        sfxSource.PlayOneShot(clip);
    }

    private void CrossfadeMusic(AudioClip newClip)
    {
        AudioSource current = isUsingSourceA ? musicSourceA : musicSourceB;
        AudioSource next = isUsingSourceA ? musicSourceB : musicSourceA;

        if (current.clip == newClip && current.isPlaying) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(CrossfadeRoutine(current, next, newClip));
        isUsingSourceA = !isUsingSourceA;
    }

    private IEnumerator CrossfadeRoutine(AudioSource current, AudioSource next, AudioClip newClip)
    {
        next.clip = newClip;
        next.volume = 0f;
        next.Play();

        float duration = 1.0f;
        float elapsed = 0f;
        float startVol = current.isPlaying ? current.volume : 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            current.volume = Mathf.Lerp(startVol, 0f, t);
            next.volume = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }

        current.volume = 0f;
        current.Stop();
        next.volume = 1f;
        fadeRoutine = null;
    }
}
