using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleView : MonoBehaviour
{
    [SerializeField] private MonsterPanel enemyPanel;
    [SerializeField] private MonsterPanel playerPanel;
    [SerializeField] private Text messageText;
    [SerializeField] private Text skillHintText;
    [SerializeField] private TextMenu battleMenu;
    [SerializeField] private Image flashOverlay;
    [SerializeField] private RectTransform wipeTop;
    [SerializeField] private RectTransform wipeBottom;
    [SerializeField] private RectTransform[] rings;

    public MonsterPanel EnemyPanel => enemyPanel;
    public MonsterPanel PlayerPanel => playerPanel;
    public event Action<int> ActionClicked;

    private Coroutine messageRoutine;

    private void Awake()
    {
        if (battleMenu != null)
        {
            battleMenu.Picked += (index) => ActionClicked?.Invoke(index);
        }
        if (flashOverlay != null) flashOverlay.color = new Color(1, 1, 1, 0);
    }

    public void ShowMessage(string text)
    {
        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(TypeMessageRoutine(text));
    }

    private IEnumerator TypeMessageRoutine(string text)
    {
        if (messageText == null) yield break;
        messageText.text = "";
        float delay = 1f / 55f;
        for (int i = 0; i < text.Length; i++)
        {
            messageText.text += text[i];
            yield return new WaitForSeconds(delay);
        }
    }

    public void ShowMenu(string[] labels, bool[] usable, string prompt, string hint)
    {
        if (skillHintText != null) skillHintText.text = hint ?? "";
        if (battleMenu != null)
        {
            battleMenu.Open(labels, usable);
        }
    }

    public void HideMenu()
    {
        if (battleMenu != null) battleMenu.Close();
        if (skillHintText != null) skillHintText.text = "";
    }

    public void PlayIntro()
    {
        StartCoroutine(IntroRoutine());
    }

    private IEnumerator IntroRoutine()
    {
        if (wipeTop == null || wipeBottom == null) yield break;
        float duration = 0.48f;
        float elapsed = 0f;
        wipeTop.gameObject.SetActive(true);
        wipeBottom.gameObject.SetActive(true);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            wipeTop.anchorMin = new Vector2(0, Mathf.Lerp(0.5f, 1f, t));
            wipeBottom.anchorMax = new Vector2(1, Mathf.Lerp(0.5f, 0f, t));
            yield return null;
        }
        wipeTop.gameObject.SetActive(false);
        wipeBottom.gameObject.SetActive(false);
    }

    public void PlayFlash()
    {
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (flashOverlay == null) yield break;
        float duration = 0.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = 1f - (elapsed / duration);
            flashOverlay.color = new Color(1, 1, 1, alpha);
            yield return null;
        }
        flashOverlay.color = new Color(1, 1, 1, 0);
    }

    public void PlayEvolveRings(Vector3 center)
    {
        if (rings == null) return;
        for (int i = 0; i < rings.Length; i++)
        {
            if (rings[i] != null)
            {
                StartCoroutine(RingExpandRoutine(rings[i], i * 0.3f));
            }
        }
    }

    private IEnumerator RingExpandRoutine(RectTransform ring, float delay)
    {
        yield return new WaitForSeconds(delay);
        ring.gameObject.SetActive(true);
        float elapsed = 0f;
        float duration = 1.2f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float s = Mathf.Lerp(0.2f, 2.5f, elapsed / duration);
            ring.localScale = new Vector3(s, s, 1f);
            yield return null;
        }
        ring.gameObject.SetActive(false);
    }
}
