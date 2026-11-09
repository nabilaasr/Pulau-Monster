using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class DialogueView : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject root;
    [SerializeField] private Image boxImage;
    [SerializeField] private GameObject speakerTagRoot;
    [SerializeField] private Text speakerText;
    [SerializeField] private Text bodyText;
    [SerializeField] private Text continueHint;
    [SerializeField] private TextMenu choiceMenu;
    [SerializeField] private Button clickCatcher;

    [SerializeField] private Sprite navyPanel;
    [SerializeField] private Sprite narrPanel;

    private Coroutine typeRoutine;
    private bool isTyping;
    private bool skipRequested;

    public bool IsTyping => isTyping;
    public TextMenu ChoiceMenu => choiceMenu;

    public event Action Clicked;

    private void Awake()
    {
        if (canvas == null) canvas = GetComponent<Canvas>();
        if (clickCatcher != null)
        {
            clickCatcher.onClick.AddListener(() =>
            {
                if (isTyping) skipRequested = true;
                else Clicked?.Invoke();
            });
        }
    }

    public void Show(bool isNarration)
    {
        root.SetActive(true);
        if (canvas != null) canvas.sortingOrder = isNarration ? 110 : 20;

        if (boxImage != null)
        {
            boxImage.sprite = isNarration ? narrPanel : navyPanel;
        }

        if (speakerTagRoot != null)
        {
            speakerTagRoot.SetActive(!isNarration);
        }

        if (continueHint != null) continueHint.gameObject.SetActive(false);
        if (choiceMenu != null) choiceMenu.Close();
    }

    public void Hide()
    {
        StopAllCoroutines();
        isTyping = false;
        skipRequested = false;
        if (root != null) root.SetActive(false);
        if (continueHint != null) continueHint.gameObject.SetActive(false);
        if (choiceMenu != null) choiceMenu.Close();
    }

    public void SetSpeaker(string name)
    {
        if (speakerText != null) speakerText.text = name;
        if (speakerTagRoot != null)
        {
            speakerTagRoot.SetActive(!string.IsNullOrEmpty(name));
        }
    }

    public IEnumerator TypeText(string text, AudioService audio, AudioClip blipClip)
    {
        isTyping = true;
        skipRequested = false;
        bodyText.text = "";
        if (continueHint != null) continueHint.gameObject.SetActive(false);

        float delayPerChar = 1f / 55f; // 55 chars/sec
        int charCount = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (skipRequested || MenuInput.Confirm)
            {
                bodyText.text = text;
                break;
            }

            bodyText.text += text[i];
            charCount++;
            if (charCount % 3 == 0 && audio != null && blipClip != null)
            {
                audio.PlaySfx(blipClip, 0.02f);
            }

            yield return new WaitForSeconds(delayPerChar);
        }

        isTyping = false;
        skipRequested = false;
    }

    public void SetContinueHint(bool visible)
    {
        if (continueHint != null)
        {
            continueHint.gameObject.SetActive(visible);
            if (visible)
            {
                StopCoroutine("BlinkHintRoutine");
                StartCoroutine("BlinkHintRoutine");
            }
        }
    }

    private IEnumerator BlinkHintRoutine()
    {
        while (continueHint != null && continueHint.gameObject.activeSelf)
        {
            continueHint.enabled = !continueHint.enabled;
            yield return new WaitForSeconds(0.3f);
        }
    }

    public void ShowChoices(string[] options, Action<int> onPicked)
    {
        if (choiceMenu == null) return;
        choiceMenu.Open(options);
        Action<int> handler = null;
        handler = (index) =>
        {
            choiceMenu.Picked -= handler;
            onPicked?.Invoke(index);
        };
        choiceMenu.Picked += handler;
    }

    public void HideChoices()
    {
        if (choiceMenu != null) choiceMenu.Close();
    }
}
