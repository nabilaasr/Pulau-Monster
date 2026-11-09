using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image blackImage;

    private void Awake()
    {
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
        if (blackImage == null) blackImage = GetComponentInChildren<Image>();
    }

    public void SetAlpha(float alpha)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
    }

    public IEnumerator FadeIn(float duration = 0.4f)
    {
        yield return Fade(1f, 0f, duration);
    }

    public IEnumerator FadeOut(float duration = 0.4f)
    {
        yield return Fade(0f, 1f, duration);
    }

    private IEnumerator Fade(float start, float end, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (canvasGroup != null) canvasGroup.alpha = Mathf.Lerp(start, end, t);
            yield return null;
        }
        if (canvasGroup != null) canvasGroup.alpha = end;
    }
}
