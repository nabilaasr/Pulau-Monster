using System.Collections;
using UnityEngine;

public class StarterGiver : Npc
{
    [SerializeField] private MonsterData[] starters;
    [SerializeField] private int starterLevel = 3;

    public static Color ElementColor(Element element)
    {
        return ElementChart.GetColor(element);
    }

    protected override IEnumerator Talk()
    {
        if (Flags != null && Flags.Has(StoryFlags.HasStarter))
        {
            yield return Dialogue.Say(NpcName, "Bawalah partnermu menjelajahi pulau. Hati-hati di Gua Petir di timur laut!");
            yield break;
        }

        yield return Dialogue.Say(NpcName, "Ah, kamu datang juga! Hutan di utara penuh monster liar.");
        yield return Dialogue.Say(NpcName, "Tidak ada yang boleh masuk hutan tanpa partner. Pilih satu dari tiga ini.");

        if (starters == null || starters.Length < 3) yield break;

        string[] options = new string[]
        {
            $"{starters[0].DisplayName} ({starters[0].Element})",
            $"{starters[1].DisplayName} ({starters[1].Element})",
            $"{starters[2].DisplayName} ({starters[2].Element})"
        };

        MonsterData pick = null;
        while (pick == null)
        {
            int choice = 0;
            yield return Dialogue.Choose(NpcName, "Siapa yang akan menjadi partnermu?", options, c => choice = c);

            MonsterData m = starters[choice];
            int sure = 0;
            yield return Dialogue.Choose(NpcName, $"{m.DisplayName} tipe {m.Element}. Yakin?",
                new[] { "Ya", "Pilih lagi" }, c => sure = c);

            if (sure == 0)
            {
                pick = m;
            }
        }

        Services.Party.SetPartner(new MonsterInstance(pick, starterLevel));
        Flags.Set(StoryFlags.HasStarter);

        yield return Dialogue.Say("", $"Kamu mendapat {pick.DisplayName}!");
        yield return Dialogue.Say(NpcName, "Rawat dia baik-baik. Menang bertarung membuatnya naik level, lalu berevolusi.");
        yield return Dialogue.Say(NpcName, "Batu Simpan di sebelahku menyimpan perjalananmu dan memulihkan partnermu.");

        Services.Toast?.Show("Penjaga Batas sudah membuka jalan ke hutan");
    }
}
