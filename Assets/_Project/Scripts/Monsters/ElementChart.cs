public static class ElementChart
{
    public static float Multiplier(Element attacker, Element defender)
    {
        // Strong against (x1.5): Pyro->Sprout, Aqua->Pyro, Sprout->Aqua, Volt->Aqua
        if (attacker == Element.Pyro && defender == Element.Sprout) return 1.5f;
        if (attacker == Element.Aqua && defender == Element.Pyro) return 1.5f;
        if (attacker == Element.Sprout && defender == Element.Aqua) return 1.5f;
        if (attacker == Element.Volt && defender == Element.Aqua) return 1.5f;

        // Weak against (x0.75)
        if (attacker == Element.Sprout && defender == Element.Pyro) return 0.75f;
        if (attacker == Element.Pyro && defender == Element.Aqua) return 0.75f;
        if (attacker == Element.Aqua && defender == Element.Sprout) return 0.75f;
        if (attacker == Element.Aqua && defender == Element.Volt) return 0.75f;

        return 1.0f;
    }

    public static UnityEngine.Color GetColor(Element element)
    {
        switch (element)
        {
            case Element.Pyro: return new UnityEngine.Color32(230, 90, 32, 255);
            case Element.Aqua: return new UnityEngine.Color32(74, 179, 255, 255);
            case Element.Sprout: return new UnityEngine.Color32(92, 214, 114, 255);
            case Element.Volt: return new UnityEngine.Color32(255, 214, 36, 255);
            default: return UnityEngine.Color.white;
        }
    }
}
