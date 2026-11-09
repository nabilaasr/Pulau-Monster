using UnityEngine;

public class DisabledMoveInput : IMoveInput
{
    public Vector2 Move => Vector2.zero;
    public bool IsRunning => false;
}
