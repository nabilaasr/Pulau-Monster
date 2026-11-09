using UnityEngine;

public static class SaveMapper
{
    public static SaveData Capture(GameServices s, Vector3 playerPos)
    {
        var data = new SaveData
        {
            playerPosition = new Vector2(playerPos.x, playerPos.y),
            potions = s.Party.Potions,
            playtime = s.Party.Playtime,
            flags = s.Flags.List()
        };

        if (s.Party.Partner != null)
        {
            data.partnerId = s.Party.Partner.Data.Id;
            data.partnerLevel = s.Party.Partner.Level;
            data.partnerExp = s.Party.Partner.Exp;
            data.partnerHp = s.Party.Partner.CurrentHP;
        }

        return data;
    }

    public static void Apply(SaveData data, GameServices s)
    {
        if (data == null) return;

        s.Party.Reset();
        s.Party.SetPotions(data.potions);
        s.Party.Playtime = data.playtime;
        s.Flags.Load(data.flags);

        if (!string.IsNullOrEmpty(data.partnerId))
        {
            MonsterData species = s.Database.GetById(data.partnerId);
            if (species != null)
            {
                var partner = new MonsterInstance(species, data.partnerLevel, data.partnerExp, data.partnerHp);
                s.Party.SetPartner(partner);
            }
        }
    }
}
