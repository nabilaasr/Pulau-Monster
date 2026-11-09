using System;
using System.Collections;
using UnityEngine;

public class DialogueRunner : MonoBehaviour
{
    [SerializeField] private AudioClip blipClip;

    private InputGate gate;
    private DialogueView view;
    private AudioService audio;
    private bool clicked;

    public bool IsRunning { get; private set; }

    public void Construct(InputGate inputGate, DialogueView dialogueView, AudioService audioService)
    {
        gate = inputGate;
        view = dialogueView;
        audio = audioService;

        if (view != null)
        {
            view.Clicked += () => clicked = true;
        }
    }

    public void Run(IEnumerator sequence)
    {
        if (!IsRunning && sequence != null)
        {
            StartCoroutine(RunRoutine(sequence));
        }
    }

    private IEnumerator RunRoutine(IEnumerator sequence)
    {
        IsRunning = true;
        gate?.Lock();
        yield return null; // skip initial trigger button
        clicked = false;

        yield return sequence;

        view?.Hide();
        yield return null; // prevent immediate re-trigger
        gate?.Unlock();
        IsRunning = false;
    }

    public void ForceClose()
    {
        StopAllCoroutines();
        if (view != null)
        {
            view.Hide();
        }
        gate?.Unlock();
        IsRunning = false;
    }

    public IEnumerator Say(string speaker, string text)
    {
        view.Show(false);
        view.SetSpeaker(speaker);
        yield return TypeLine(text, true);
    }

    public IEnumerator Narrate(string text)
    {
        view.Show(true);
        view.SetSpeaker("");
        yield return TypeLine(text, true);
    }

    public IEnumerator Choose(string speaker, string prompt, string[] options, Action<int> result)
    {
        view.Show(false);
        view.SetSpeaker(speaker);
        yield return TypeLine(prompt, false);

        int picked = -1;
        view.ShowChoices(options, idx => picked = idx);
        yield return new WaitUntil(() => picked >= 0);
        view.HideChoices();
        result?.Invoke(picked);
    }

    public IEnumerator PlayData(DialogueData data)
    {
        if (data == null || data.Lines == null) yield break;
        for (int i = 0; i < data.Lines.Length; i++)
        {
            yield return Say(data.Speaker, data.Lines[i]);
        }
    }

    private IEnumerator TypeLine(string text, bool waitForConfirm)
    {
        yield return view.TypeText(text, audio, blipClip);
        if (waitForConfirm)
        {
            view.SetContinueHint(true);
            yield return null; // let frame pass
            clicked = false;
            yield return new WaitUntil(AdvancePressed);
            view.SetContinueHint(false);
        }
    }

    private bool AdvancePressed()
    {
        bool pressed = clicked || MenuInput.Confirm;
        clicked = false;
        return pressed;
    }
}
