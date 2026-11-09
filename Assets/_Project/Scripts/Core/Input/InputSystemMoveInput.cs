using UnityEngine;
using UnityEngine.InputSystem;

public class InputSystemMoveInput : IMoveInput
{
    private readonly InputAction move;
    private readonly InputAction sprint;

    public InputSystemMoveInput()
    {
        move = InputSystem.actions?.FindAction("Move");
        sprint = InputSystem.actions?.FindAction("Sprint");
    }

    public Vector2 Move => move != null ? move.ReadValue<Vector2>() : Vector2.zero;
    public bool IsRunning => sprint != null && sprint.IsPressed();
}
