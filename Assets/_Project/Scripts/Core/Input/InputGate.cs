using UnityEngine;

public class InputGate : IMoveInput, IActionInput
{
    private readonly IMoveInput move;
    private readonly IActionInput action;
    private int lockCount;

    public InputGate(IMoveInput moveInput, IActionInput actionInput)
    {
        move = moveInput;
        action = actionInput;
    }

    public bool IsLocked => lockCount > 0;

    public void Lock() => lockCount++;
    public void Unlock() => lockCount = Mathf.Max(0, lockCount - 1);
    public void ForceUnlock() => lockCount = 0;

    public Vector2 Move => IsLocked ? Vector2.zero : (move != null ? move.Move : Vector2.zero);
    public bool IsRunning => !IsLocked && move != null && move.IsRunning;
    public bool InteractPressed => !IsLocked && action != null && action.InteractPressed;
}
