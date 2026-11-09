using System;
using System.Collections.Generic;
using UnityEngine;

public enum GrowthType
{
    LevelUp,
    Evolve
}

public class GrowthEvent
{
    public GrowthType Type { get; set; }
    public MonsterData From { get; set; }
    public MonsterData To { get; set; }
    public string Message { get; set; }
}

public class MonsterInstance : IDamageable
{
    public MonsterData Data { get; private set; }
    public int Level { get; private set; }
    public int CurrentHP { get; private set; }
    public int Exp { get; private set; }

    public event Action Changed;

    public float Scale => 1f + (Level - 1) * 0.1f;
    public int MaxHP => Mathf.RoundToInt(Data.BaseHP * Scale) + Level * 2;
    public int Attack => Mathf.RoundToInt(Data.BaseATK * Scale);
    public int Defense => Mathf.RoundToInt(Data.BaseDEF * Scale);
    public int Speed => Mathf.RoundToInt(Data.BaseSPD * Scale);
    public int ExpToNext => Level * 10;
    public bool IsFainted => CurrentHP <= 0;

    public MonsterInstance(MonsterData data, int level, int exp = 0, int currentHp = -1)
    {
        Data = data;
        Level = Mathf.Max(1, level);
        Exp = exp;
        CurrentHP = (currentHp >= 0 && currentHp <= MaxHP) ? currentHp : MaxHP;
    }

    public void TakeDamage(int damage)
    {
        CurrentHP = Mathf.Clamp(CurrentHP - damage, 0, MaxHP);
        Changed?.Invoke();
    }

    public int Heal(int amount)
    {
        int before = CurrentHP;
        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, MaxHP);
        int healed = CurrentHP - before;
        if (healed > 0) Changed?.Invoke();
        return healed;
    }

    public void HealFull()
    {
        CurrentHP = MaxHP;
        Changed?.Invoke();
    }

    public IEnumerable<GrowthEvent> GainExp(int expGain)
    {
        Exp += expGain;
        while (Exp >= ExpToNext)
        {
            Exp -= ExpToNext;
            Level++;
            MonsterData fromData = Data;
            yield return new GrowthEvent
            {
                Type = GrowthType.LevelUp,
                From = fromData,
                To = fromData,
                Message = $"{fromData.DisplayName} naik ke Lv {Level}!"
            };

            if (Data.EvolvesTo != null && Data.EvolveLevel > 0 && Level >= Data.EvolveLevel)
            {
                MonsterData toData = Data.EvolvesTo;
                Data = toData;
                yield return new GrowthEvent
                {
                    Type = GrowthType.Evolve,
                    From = fromData,
                    To = toData,
                    Message = $"{fromData.DisplayName} berevolusi menjadi {toData.DisplayName}!"
                };
            }
        }
        Changed?.Invoke();
    }
}
