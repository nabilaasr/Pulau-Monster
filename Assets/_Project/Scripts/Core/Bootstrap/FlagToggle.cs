using UnityEngine;

public class FlagToggle : MonoBehaviour
{
    [SerializeField] private string flagKey = StoryFlags.HasStarter;
    [SerializeField] private bool hideWhenSet = true;

    private StoryFlags flags;

    public void Construct(StoryFlags storyFlags)
    {
        flags = storyFlags;
        if (flags != null)
        {
            flags.Changed += OnFlagChanged;
            Apply();
        }
    }

    private void OnDestroy()
    {
        if (flags != null)
        {
            flags.Changed -= OnFlagChanged;
        }
    }

    private void OnFlagChanged(string key)
    {
        if (key == flagKey) Apply();
    }

    private void Apply()
    {
        if (flags == null) return;
        bool has = flags.Has(flagKey);
        gameObject.SetActive(hideWhenSet ? !has : has);
    }
}
