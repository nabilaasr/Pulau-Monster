using UnityEngine;

public class BattleAudio : MonoBehaviour
{
    [SerializeField] private AudioClip sfxHit;
    [SerializeField] private AudioClip sfxHitCritical;
    [SerializeField] private AudioClip sfxMiss;
    [SerializeField] private AudioClip sfxLevelUp;
    [SerializeField] private AudioClip sfxEvolve;
    [SerializeField] private AudioClip sfxVictory;
    [SerializeField] private AudioClip sfxFaint;
    [SerializeField] private AudioClip sfxHeal;

    private BattleSystem battle;
    private AudioService audio;

    public void Construct(BattleSystem battleSystem, AudioService audioService)
    {
        battle = battleSystem;
        audio = audioService;

        if (battle != null)
        {
            battle.Hit += OnHit;
            battle.Missed += OnMissed;
            battle.LevelUp += OnLevelUp;
            battle.Evolving += OnEvolving;
            battle.Victory += OnVictory;
            battle.Fainted += OnFainted;
            battle.Healed += OnHealed;
        }
    }

    private void OnDestroy()
    {
        if (battle != null)
        {
            battle.Hit -= OnHit;
            battle.Missed -= OnMissed;
            battle.LevelUp -= OnLevelUp;
            battle.Evolving -= OnEvolving;
            battle.Victory -= OnVictory;
            battle.Fainted -= OnFainted;
            battle.Healed -= OnHealed;
        }
    }

    private void OnHit(MonsterInstance target, int damage)
    {
        if (battle.LastHitCritical)
        {
            audio?.PlaySfx(sfxHitCritical, 0.05f);
        }
        else
        {
            audio?.PlaySfx(sfxHit, 0.05f);
        }
    }

    private void OnMissed() => audio?.PlaySfx(sfxMiss, 0.05f);
    private void OnLevelUp(MonsterData from) => audio?.PlaySfx(sfxLevelUp, 0f);
    private void OnEvolving(MonsterData from, MonsterData to) => audio?.PlaySfx(sfxEvolve, 0f);
    private void OnVictory() => audio?.PlaySfx(sfxVictory, 0f);
    private void OnFainted(MonsterInstance m) => audio?.PlaySfx(sfxFaint, 0f);
    private void OnHealed(MonsterInstance m) => audio?.PlaySfx(sfxHeal, 0f);
}
