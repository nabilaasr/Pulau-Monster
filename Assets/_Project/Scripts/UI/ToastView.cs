using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToastView : MonoBehaviour
{
    [SerializeField] private CanvasGroup group;
    [SerializeField] private Text messageText;

    private Coroutine showRoutine;

    public void Construct()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        if (messageText == null) messageText = GetComponentInChildren<Text>();
        if (group != null) group.alpha = 0f;
    }

    public void Show(string message)
    {
        if (showRoutine != null) StopCoroutine(showRoutine);
        showRoutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        if (messageText != null) messageText.text = message;
        gameObject.SetActive(true);

        // Fade in
        float elapsed = 0f;
        while (elapsed < 0.25f)
        {
            elapsed += Time.deltaTime;
            if (group != null) group.alpha = elapsed / 0.25f;
            yield return null;
        }
        if (group != null) group.alpha = 1f;

        yield return new WaitForSeconds(2.0f);

        // Fade out
        elapsed = 0f;
        while (elapsed < 0.35f)
        {
            elapsed += Time.deltaTime;
            if (group != null) group.alpha = 1f - (elapsed / 0.35f);
            yield return null;
        }
        if (group != null) group.alpha = 0f;
        gameObject.SetActive(false);
        showRoutine = null;
    }
}
