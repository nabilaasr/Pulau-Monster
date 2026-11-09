using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleSystem : MonoBehaviour
{
    public event Action Started;
    public event Action<string> MessageShown;
    public event Action ChoiceRequested;
    public event Action<MonsterInstance> Attacked;
    public event Action<MonsterInstance, int> Hit;
    public event Action Missed;
    public event Action<MonsterInstance> Fainted;
    public event Action Victory;
    public event Action<MonsterData> LevelUp;
    public event Action<MonsterData, MonsterData> Evolving;
    public event Action<MonsterInstance> Evolved;
    public event Action<MonsterInstance> Healed;
    public event Action<BattleResult> Ended;

    public MonsterInstance Player { get; private set; }
    public MonsterInstance Enemy { get; private set; }
    public bool IsBoss { get; private set; }
    public PlayerParty Party { get; private set; }

    public Element? LastHitElement { get; private set; }
    public bool LastHitCritical { get; private set; }

    private BattleSession session;
    private SceneFlow scenes;
    private BattleAction chosenAction;
    private BattleResult result = BattleResult.None;
    private int playerSkillUses = 0;
    private int enemySkillUses = 0;

    private const float SecondsPerChar = 0.018f;
    private const float EvolveSeconds = 2.4f;

    public int PlayerSkillUsesRemaining => Mathf.Max(0, 3 - playerSkillUses);

    public void Construct(PlayerParty playerParty, BattleSession battleSession, SceneFlow sceneFlow)
    {
        Party = playerParty;
        session = battleSession;
        scenes = sceneFlow;

        Player = Party != null ? Party.Partner : null;
        Enemy = session != null ? session.Enemy : null;
        IsBoss = session != null && session.IsBoss;
    }

    public void Begin()
    {
        result = BattleResult.None;
        playerSkillUses = 0;
        enemySkillUses = 0;
        StartCoroutine(RunRoutine());
    }

    public void SelectAction(BattleAction action)
    {
        if (action is SkillAction)
        {
            playerSkillUses++;
        }
        chosenAction = action;
    }

    public void SetResult(BattleResult res) => result = res;

    public float DamageScale(MonsterInstance user)
    {
        if (user == Enemy && !IsBoss) return 0.8f;
        return 1.0f;
    }

    public void RaiseAttacked(MonsterInstance user) => Attacked?.Invoke(user);

    public void RaiseHit(MonsterInstance target, int damage, bool critical, Element? elem)
    {
        LastHitCritical = critical;
        LastHitElement = elem;
        Hit?.Invoke(target, damage);
    }

    public void RaiseMissed() => Missed?.Invoke();
    public void RaiseHealed(MonsterInstance target) => Healed?.Invoke(target);

    public IEnumerator Say(string text, float hold = 0.8f)
    {
        MessageShown?.Invoke(text);
        float wait = hold + text.Length * SecondsPerChar;
        for (float t = 0f; t < wait; t += Time.deltaTime)
        {
            if (t > text.Length * SecondsPerChar && MenuInput.Confirm) break;
            yield return null;
        }
    }

    private IEnumerator RunRoutine()
    {
        Started?.Invoke();
        yield return new WaitForSeconds(0.5f);

        yield return Say(IsBoss
            ? $"{Enemy.Data.DisplayName}, penguasa Gua Petir, muncul!"
            : $"{Enemy.Data.DisplayName} liar Lv{Enemy.Level} muncul!", 0.9f);

        yield return Say($"Maju, {Player.Data.DisplayName}!", 0.7f);

        while (result == BattleResult.None)
        {
            chosenAction = null;
            ChoiceRequested?.Invoke();
            yield return new WaitUntil(() => chosenAction != null);

            if (chosenAction is RunAction && IsBoss)
            {
                yield return chosenAction.Execute(this, Player, Enemy);
                continue;
            }

            BattleAction enemyAction = PickEnemyAction();

            bool playerFirst = chosenAction.Priority > enemyAction.Priority ||
                               (chosenAction.Priority == enemyAction.Priority &&
                                (Player.Speed > Enemy.Speed ||
                                 (Player.Speed == Enemy.Speed && UnityEngine.Random.value < 0.5f)));

            if (playerFirst)
            {
                yield return Turn(chosenAction, Player, Enemy);
                if (result == BattleResult.None)
                {
                    yield return Turn(enemyAction, Enemy, Player);
                }
            }
            else
            {
                yield return Turn(enemyAction, Enemy, Player);
                if (result == BattleResult.None)
                {
                    yield return Turn(chosenAction, Player, Enemy);
                }
            }
        }

        Ended?.Invoke(result);
        yield return new WaitForSeconds(0.3f);
        scenes?.EndBattle(result);
    }

    private BattleAction PickEnemyAction()
    {
        if (enemySkillUses < 2 && UnityEngine.Random.value < 0.35f)
        {
            enemySkillUses++;
            return new SkillAction();
        }
        return new AttackAction();
    }

    private IEnumerator Turn(BattleAction action, MonsterInstance actor, MonsterInstance receiver)
    {
        if (actor.IsFainted || receiver.IsFainted || result != BattleResult.None) yield break;

        yield return action.Execute(this, actor, receiver);

        if (Enemy.IsFainted)
        {
            yield return Win();
        }
        else if (Player.IsFainted)
        {
            yield return Lose();
        }
    }

    private IEnumerator Win()
    {
        result = BattleResult.Win;
        Fainted?.Invoke(Enemy);
        yield return new WaitForSeconds(0.6f);

        yield return Say(IsBoss
            ? $"{Enemy.Data.DisplayName} kalah!"
            : $"{Enemy.Data.DisplayName} liar kalah!", 0.8f);

        Victory?.Invoke();

        int exp = (10 + Enemy.Level * 8) * (IsBoss ? 2 : 1);
        yield return Say($"{Player.Data.DisplayName} mendapat {exp} EXP.", 0.9f);

        foreach (GrowthEvent growth in Player.GainExp(exp))
        {
            if (growth.Type == GrowthType.LevelUp)
            {
                LevelUp?.Invoke(growth.From);
                yield return Say(growth.Message, 0.9f);
            }
            else if (growth.Type == GrowthType.Evolve)
            {
                yield return Say($"Apa? {growth.From.DisplayName} mulai bersinar...", 0.9f);
                Evolving?.Invoke(growth.From, growth.To);
                yield return new WaitForSeconds(EvolveSeconds);
                Evolved?.Invoke(Player);
                yield return Say(growth.Message, 1.2f);
            }
        }

        if (!IsBoss)
        {
            int heal = Player.Heal(Mathf.RoundToInt(Player.MaxHP * 0.25f));
            if (heal > 0)
            {
                yield return Say($"{Player.Data.DisplayName} beristirahat sejenak dan pulih {heal} HP.", 0.8f);
            }
        }
    }

    private IEnumerator Lose()
    {
        result = BattleResult.Lose;
        Fainted?.Invoke(Player);
        yield return new WaitForSeconds(0.6f);
        yield return Say($"{Player.Data.DisplayName} pingsan... Kamu berlari kembali ke desa.", 1.2f);
    }
}
