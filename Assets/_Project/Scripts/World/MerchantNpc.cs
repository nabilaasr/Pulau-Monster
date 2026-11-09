using System.Collections;
using UnityEngine;

public class MerchantNpc : Npc
{
    [SerializeField] private AudioClip healClip;

    protected override IEnumerator Talk()
    {
        if (Flags != null && !Flags.Has(StoryFlags.Gift))
        {
            yield return Dialogue.Say(NpcName, "Petualang baru? Bawa 3 Ramuan ini. Gratis untuk hari pertama!");
            Services.Party.AddPotions(3);
            Flags.Set(StoryFlags.Gift);
            if (healClip != null) Services.Audio?.PlaySfx(healClip, 0f);
            Services.Toast?.Show("Mendapat 3 Ramuan!");
        }
        else
        {
            yield return Dialogue.Say(NpcName, "Gunakan Ramuan saat HP partnermu kritis di tengah pertarungan!");
        }
    }
}
