using UnityEngine;

public static class UiTheme
{
    public static readonly Color Ink = new Color32(18, 16, 39, 255);        // #121027
    public static readonly Color Panel = new Color32(28, 21, 64, 255);      // #1C1540
    public static readonly Color Gold = new Color32(255, 198, 26, 255);     // #FFC61A
    public static readonly Color Text = new Color32(237, 233, 247, 255);    // #EDE9F7
    public static readonly Color Muted = new Color32(169, 163, 201, 255);   // #A9A3C9
    public static readonly Color Disabled = new Color32(92, 86, 128, 255);  // #5C5680
    public static readonly Color Exp = new Color32(111, 155, 255, 255);     // #6F9BFF
}

public static class HpColors
{
    public static readonly Color High = new Color32(67, 160, 71, 255);  // Green
    public static readonly Color Mid = new Color32(255, 198, 26, 255);  // Gold
    public static readonly Color Low = new Color32(229, 57, 53, 255);   // Red

    public static Color For(float fraction)
    {
        if (fraction > 0.5f) return High;
        if (fraction > 0.2f) return Mid;
        return Low;
    }
}
