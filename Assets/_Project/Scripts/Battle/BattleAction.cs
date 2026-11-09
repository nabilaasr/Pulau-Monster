using System.Collections;
using UnityEngine;

public abstract class BattleAction
{
    public virtual int Priority => 0;
    public abstract IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target);
}

public class AttackAction : BattleAction
{
    public override int Priority => 0;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        string prefix = (user == battle.Enemy && !battle.IsBoss) ? "Lawan " : "";
        yield return battle.Say($"{prefix}{user.Data.DisplayName} menyerang!", 0.7f);

        battle.RaiseAttacked(user);
        yield return new WaitForSeconds(0.2f);

        // 6% miss chance
        if (Random.value < 0.06f)
        {
            battle.RaiseMissed();
            yield return battle.Say("Serangan meleset!", 0.8f);
            yield break;
        }

        float mult = battle.DamageScale(user);
        bool critical;
        int damage = DamageFormula.Calculate(user, target, 14, mult, out critical);

        target.TakeDamage(damage);
        battle.RaiseHit(target, damage, critical, null);

        yield return new WaitForSeconds(0.45f);
        if (critical)
        {
            yield return battle.Say("Serangan kritis!", 0.8f);
        }
    }
}

public class SkillAction : BattleAction
{
    public override int Priority => 0;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        string prefix = (user == battle.Enemy && !battle.IsBoss) ? "Lawan " : "";
        string skill = user.Data.SkillName;
        yield return battle.Say($"{prefix}{user.Data.DisplayName} memakai {skill}!", 0.7f);

        battle.RaiseAttacked(user);
        yield return new WaitForSeconds(0.2f);

        // 6% miss chance
        if (Random.value < 0.06f)
        {
            battle.RaiseMissed();
            yield return battle.Say("Serangan meleset!", 0.8f);
            yield break;
        }

        float elemMult = ElementChart.Multiplier(user.Data.Element, target.Data.Element);
        float mult = elemMult * battle.DamageScale(user);
        bool critical;
        int damage = DamageFormula.Calculate(user, target, 22, mult, out critical);

        target.TakeDamage(damage);
        battle.RaiseHit(target, damage, critical, user.Data.Element);

        yield return new WaitForSeconds(0.45f);
        if (critical)
        {
            yield return battle.Say("Serangan kritis!", 0.8f);
        }

        if (elemMult > 1.05f)
        {
            yield return battle.Say("Sangat efektif!", 0.8f);
        }
        else if (elemMult < 0.95f)
        {
            yield return battle.Say("Kurang efektif...", 0.8f);
        }
    }
}

public class PotionAction : BattleAction
{
    public override int Priority => 1;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (user == battle.Player)
        {
            if (battle.Party != null && battle.Party.ConsumePotion())
            {
                int healAmount = Mathf.Max(1, Mathf.RoundToInt(user.MaxHP * 0.4f));
                int actual = user.Heal(healAmount);
                battle.RaiseHealed(user);
                yield return battle.Say($"{user.Data.DisplayName} meminum Ramuan dan pulih {actual} HP!", 0.9f);
            }
            else
            {
                yield return battle.Say("Tidak ada Ramuan tersisa!", 0.8f);
            }
        }
    }
}

public class RunAction : BattleAction
{
    public override int Priority => 2;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        if (battle.IsBoss)
        {
            yield return battle.Say("Tidak bisa kabur dari pertarungan bos!", 0.8f);
            yield break;
        }

        yield return battle.Say("Mencoba kabur...", 0.6f);
        if (Random.value < 0.7f)
        {
            yield return battle.Say("Berhasil kabur!", 0.8f);
            battle.SetResult(BattleResult.Fled);
        }
        else
        {
            yield return battle.Say("Gagal kabur!", 0.8f);
        }
    }
}

public class GuardAction : BattleAction
{
    public override int Priority => 1;

    public override IEnumerator Execute(BattleSystem battle, MonsterInstance user, MonsterInstance target)
    {
        string prefix = (user == battle.Enemy && !battle.IsBoss) ? "Lawan " : "";
        yield return battle.Say($"{prefix}{user.Data.DisplayName} bersiap bertahan!", 0.7f);
    }
}
