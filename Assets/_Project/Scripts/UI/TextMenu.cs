using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TextMenu : MonoBehaviour
{
    [SerializeField] private int columns = 1;
    [SerializeField] private List<Text> itemTexts = new List<Text>();
    [SerializeField] private List<Button> itemButtons = new List<Button>();

    public event Action<int> Picked;
    public event Action<int> Selected;

    private string[] optionLabels;
    private bool[] optionEnabled;
    private int currentIndex;
    private bool isOpen;
    private bool acceptInput;
    private AudioService audioService;

    public int CurrentIndex => currentIndex;
    public bool IsOpen => isOpen;

    public void SetAudio(AudioService audio) => audioService = audio;

    public void SetupItems(List<Text> texts, List<Button> buttons, int cols = 1)
    {
        itemTexts = texts;
        itemButtons = buttons;
        columns = cols;
        BindButtons();
    }

    private void Awake()
    {
        BindButtons();
    }

    private void BindButtons()
    {
        for (int i = 0; i < itemButtons.Count; i++)
        {
            int index = i;
            Button btn = itemButtons[i];
            if (btn == null) continue;

            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnButtonClick(index));

            EventTrigger trigger = btn.GetComponent<EventTrigger>();
            if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();
            trigger.triggers.Clear();

            var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            entry.callback.AddListener((data) => OnButtonHover(index));
            trigger.triggers.Add(entry);
        }
    }

    public void Open(string[] options, bool[] enabled = null, int start = 0)
    {
        optionLabels = options ?? new string[0];
        if (enabled == null || enabled.Length != optionLabels.Length)
        {
            optionEnabled = new bool[optionLabels.Length];
            for (int i = 0; i < optionEnabled.Length; i++) optionEnabled[i] = true;
        }
        else
        {
            optionEnabled = enabled;
        }

        currentIndex = Mathf.Clamp(start, 0, Mathf.Max(0, optionLabels.Length - 1));
        isOpen = true;
        gameObject.SetActive(true);

        RefreshVisuals();
        Selected?.Invoke(currentIndex);

        acceptInput = false;
        StopAllCoroutines();
        StartCoroutine(EnableInputNextFrame());
    }

    public void Close()
    {
        isOpen = false;
        acceptInput = false;
        gameObject.SetActive(false);
    }

    private IEnumerator EnableInputNextFrame()
    {
        yield return null;
        acceptInput = true;
    }

    private void Update()
    {
        if (!isOpen || !acceptInput || optionLabels == null || optionLabels.Length == 0) return;

        int total = optionLabels.Length;
        int prev = currentIndex;

        if (columns == 1)
        {
            if (MenuInput.Up) currentIndex = (currentIndex - 1 + total) % total;
            else if (MenuInput.Down) currentIndex = (currentIndex + 1) % total;
        }
        else // 2 columns
        {
            int row = currentIndex / columns;
            int col = currentIndex % columns;
            int rows = Mathf.CeilToInt((float)total / columns);

            if (MenuInput.Up) row = (row - 1 + rows) % rows;
            else if (MenuInput.Down) row = (row + 1) % rows;
            else if (MenuInput.Left) col = (col - 1 + columns) % columns;
            else if (MenuInput.Right) col = (col + 1) % columns;

            int next = row * columns + col;
            if (next < total) currentIndex = next;
        }

        if (currentIndex != prev)
        {
            RefreshVisuals();
            Selected?.Invoke(currentIndex);
            // Play UI move sound
            // audioService?.PlaySfx(...)
        }

        if (MenuInput.Confirm)
        {
            ConfirmSelection();
        }
    }

    public void ConfirmSelection()
    {
        if (!isOpen || optionLabels == null || currentIndex < 0 || currentIndex >= optionLabels.Length) return;
        if (optionEnabled != null && !optionEnabled[currentIndex]) return;

        Picked?.Invoke(currentIndex);
    }

    private void OnButtonClick(int index)
    {
        if (!isOpen || !acceptInput) return;
        if (index < 0 || index >= optionLabels.Length) return;
        if (optionEnabled != null && !optionEnabled[index]) return;

        currentIndex = index;
        RefreshVisuals();
        Picked?.Invoke(currentIndex);
    }

    private void OnButtonHover(int index)
    {
        if (!isOpen || !acceptInput) return;
        if (index < 0 || index >= optionLabels.Length) return;

        if (currentIndex != index)
        {
            currentIndex = index;
            RefreshVisuals();
            Selected?.Invoke(currentIndex);
        }
    }

    private void RefreshVisuals()
    {
        for (int i = 0; i < itemTexts.Count; i++)
        {
            if (itemTexts[i] == null) continue;

            if (i < optionLabels.Length)
            {
                itemTexts[i].gameObject.SetActive(true);
                bool isSelected = (i == currentIndex);
                bool isItemEnabled = (optionEnabled == null || optionEnabled[i]);

                string prefix = isSelected ? "▶ " : "  ";
                itemTexts[i].text = prefix + optionLabels[i];

                if (!isItemEnabled)
                {
                    itemTexts[i].color = UiTheme.Disabled;
                }
                else if (isSelected)
                {
                    itemTexts[i].color = UiTheme.Gold;
                }
                else
                {
                    itemTexts[i].color = UiTheme.Text;
                }
            }
            else
            {
                itemTexts[i].gameObject.SetActive(false);
            }
        }
    }
}
