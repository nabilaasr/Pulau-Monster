using System;
using System.Collections;
using UnityEngine;

public class SpriteVfx : MonoBehaviour
{
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float frameDuration = 0.05f;

    private Action<SpriteVfx> onFinished;

    public void Construct(Sprite[] animationFrames, Action<SpriteVfx> finishCallback)
    {
        frames = animationFrames;
        onFinished = finishCallback;
        if (sr == null) sr = GetComponent<SpriteRenderer>();
    }

    public void Play(Vector3 position)
    {
        transform.position = position;
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(PlayRoutine());
    }

    private IEnumerator PlayRoutine()
    {
        if (frames != null && frames.Length > 0 && sr != null)
        {
            for (int i = 0; i < frames.Length; i++)
            {
                sr.sprite = frames[i];
                yield return new WaitForSeconds(frameDuration);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        gameObject.SetActive(false);
        onFinished?.Invoke(this);
    }
}
