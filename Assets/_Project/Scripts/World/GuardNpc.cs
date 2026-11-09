using System.Collections;
using UnityEngine;

public class GuardNpc : Npc
{
    [SerializeField] private Vector2 asidePosition = new Vector2(22.5f, 10.4f);

    protected override void OnConstructed()
    {
        if (Flags != null)
        {
            Flags.Changed += OnFlag;
            if (Flags.Has(StoryFlags.HasStarter))
            {
                StepAside();
            }
        }
    }

    private void OnDestroy()
    {
        if (Flags != null)
        {
            Flags.Changed -= OnFlag;
        }
    }

    private void OnFlag(string key)
    {
        if (key == StoryFlags.HasStarter)
        {
            StepAside();
        }
    }

    private void StepAside()
    {
        transform.position = asidePosition;
    }

    protected override IEnumerator Talk()
    {
        if (Flags == null || !Flags.Has(StoryFlags.HasStarter))
        {
            yield return Dialogue.Say(NpcName, "Berhenti! Hutan terlalu berbahaya tanpa monster partner. Temui Tetua dulu.");
        }
        else
        {
            yield return Dialogue.Say(NpcName, "Jalan terbuka. Monster di rumput tinggi akan mengejarmu kalau terlalu dekat.");
        }
    }
}
