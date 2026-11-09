using UnityEngine;
using UnityEngine.InputSystem;

public static class MenuInput
{
    private static Keyboard kb => Keyboard.current;
    private static Gamepad gp => Gamepad.current;

    public static bool Up =>
        (kb != null && (kb.upArrowKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame)) ||
        (gp != null && (gp.dpad.up.wasPressedThisFrame || gp.leftStick.up.wasPressedThisFrame));

    public static bool Down =>
        (kb != null && (kb.downArrowKey.wasPressedThisFrame || kb.sKey.wasPressedThisFrame)) ||
        (gp != null && (gp.dpad.down.wasPressedThisFrame || gp.leftStick.down.wasPressedThisFrame));

    public static bool Left =>
        (kb != null && (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame)) ||
        (gp != null && (gp.dpad.left.wasPressedThisFrame || gp.leftStick.left.wasPressedThisFrame));

    public static bool Right =>
        (kb != null && (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame)) ||
        (gp != null && (gp.dpad.right.wasPressedThisFrame || gp.leftStick.right.wasPressedThisFrame));

    public static bool Confirm =>
        (kb != null && (kb.eKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame ||
                        kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame ||
                        kb.zKey.wasPressedThisFrame)) ||
        (gp != null && gp.buttonSouth.wasPressedThisFrame);

    public static bool Cancel =>
        (kb != null && (kb.escapeKey.wasPressedThisFrame || kb.xKey.wasPressedThisFrame ||
                        kb.backspaceKey.wasPressedThisFrame)) ||
        (gp != null && gp.buttonEast.wasPressedThisFrame);

    public static bool Mute =>
        kb != null && kb.mKey.wasPressedThisFrame;
}
