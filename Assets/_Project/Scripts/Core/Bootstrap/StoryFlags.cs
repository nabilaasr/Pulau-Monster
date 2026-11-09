using System;
using System.Collections.Generic;

public class StoryFlags
{
    public const string HasStarter = "has_starter";
    public const string Gift = "gift";
    public const string IntroSeen = "intro_seen";
    public const string BossDefeated = "boss_defeated";

    private readonly HashSet<string> flags = new HashSet<string>();

    public event Action<string> Changed;

    public bool Has(string key) => flags.Contains(key);

    public void Set(string key)
    {
        if (flags.Add(key))
        {
            Changed?.Invoke(key);
        }
    }

    public void Clear(string key)
    {
        if (flags.Remove(key))
        {
            Changed?.Invoke(key);
        }
    }

    public List<string> List() => new List<string>(flags);

    public void Load(IEnumerable<string> keys)
    {
        flags.Clear();
        if (keys != null)
        {
            foreach (string k in keys) flags.Add(k);
        }
    }
}
