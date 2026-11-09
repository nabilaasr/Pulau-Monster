using UnityEngine;

public class BattlePresenter
{
    private readonly BattleSystem battle;
    private readonly BattleView view;

    public BattlePresenter(BattleSystem battleSystem, BattleView battleView)
    {
        battle = battleSystem;
        view = battleView;

        battle.Started += OnStarted;
        battle.MessageShown += OnMessageShown;
        battle.ChoiceRequested += OnChoiceRequested;
        battle.Attacked += OnAttacked;
        battle.Hit += OnHit;
        battle.Fainted += OnFainted;
        battle.LevelUp += OnLevelUp;
        battle.Evolving += OnEvolving;
        battle.Evolved += OnEvolved;
        battle.Healed += OnHealed;

        view.ActionClicked += OnActionClicked;
    }

    private void OnStarted()
    {
        view.EnemyPanel.Show(battle.Enemy, battle.IsBoss);
        view.PlayerPanel.Show(battle.Player, false);
        view.PlayIntro();
    }

    private void OnMessageShown(string message)
    {
        view.ShowMessage(message);
    }

    private void OnChoiceRequested()
    {
        int skillsLeft = battle.PlayerSkillUsesRemaining;
        int potionsLeft = battle.Party != null ? battle.Party.Potions : 0;

        string[] labels = new string[]
        {
            "Serang",
            $"Jurus ({skillsLeft})",
            $"Ramuan ({potionsLeft})",
            "Kabur"
        };

        bool[] usable = new bool[]
        {
            true,
            skillsLeft > 0,
            potionsLeft > 0,
            !battle.IsBoss
        };

        string prompt = $"Apa yang akan {battle.Player.Data.DisplayName} lakukan?";
        string hint = $"Jurus elemen: {battle.Player.Data.SkillName}";
        view.ShowMenu(labels, usable, prompt, hint);
    }

    private void OnActionClicked(int index)
    {
        view.HideMenu();
        switch (index)
        {
            case 0:
                battle.SelectAction(new AttackAction());
                break;
            case 1:
                battle.SelectAction(new SkillAction());
                break;
            case 2:
                battle.SelectAction(new PotionAction());
                break;
            case 3:
                battle.SelectAction(new RunAction());
                break;
        }
    }

    private void OnAttacked(MonsterInstance user)
    {
        if (user == battle.Player)
        {
            view.PlayerPanel.PlayLunge(true);
        }
        else
        {
            view.EnemyPanel.PlayLunge(false);
        }
    }

    private void OnHit(MonsterInstance target, int damage)
    {
        if (target == battle.Player)
        {
            view.PlayerPanel.RefreshStats(battle.Player);
            view.PlayerPanel.PlayHurt();
        }
        else
        {
            view.EnemyPanel.RefreshStats(battle.Enemy);
            view.EnemyPanel.PlayHurt();
        }
    }

    private void OnFainted(MonsterInstance m)
    {
        if (m == battle.Player)
        {
            view.PlayerPanel.PlayFaint();
        }
        else
        {
            view.EnemyPanel.PlayFaint();
        }
    }

    private void OnLevelUp(MonsterData from)
    {
        view.PlayerPanel.RefreshStats(battle.Player);
    }

    private void OnEvolving(MonsterData from, MonsterData to)
    {
        view.PlayerPanel.PlayEvolve(from, to, 2.4f);
        view.PlayEvolveRings(Vector3.zero);
    }

    private void OnEvolved(MonsterInstance m)
    {
        view.PlayFlash();
        view.PlayerPanel.Show(m, false);
    }

    private void OnHealed(MonsterInstance m)
    {
        if (m == battle.Player)
        {
            view.PlayerPanel.RefreshStats(battle.Player);
        }
    }
}
