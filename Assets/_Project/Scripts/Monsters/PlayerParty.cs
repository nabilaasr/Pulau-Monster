using System;

public class PlayerParty
{
    public MonsterInstance Partner { get; private set; }
    public int Potions { get; private set; } = 3;
    public float Playtime { get; set; }

    public event Action PotionsChanged;
    public event Action PartnerChanged;

    public void SetPartner(MonsterInstance monster)
    {
        Partner = monster;
        PartnerChanged?.Invoke();
    }

    public void SetPotions(int count)
    {
        Potions = count;
        PotionsChanged?.Invoke();
    }

    public void AddPotions(int count)
    {
        Potions += count;
        PotionsChanged?.Invoke();
    }

    public bool ConsumePotion()
    {
        if (Potions <= 0) return false;
        Potions--;
        PotionsChanged?.Invoke();
        return true;
    }

    public void Reset()
    {
        Partner = null;
        Potions = 3;
        Playtime = 0f;
        PotionsChanged?.Invoke();
        PartnerChanged?.Invoke();
    }
}
