using UnityEngine;

public class BattleInstaller : SceneInstaller
{
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private Sprite caveBackground;
    [SerializeField] private Sprite forestBackground;
    [SerializeField] private BattleSystem battleSystem;
    [SerializeField] private BattleView battleView;
    [SerializeField] private BattleAudio battleAudio;
    [SerializeField] private BattleFeel battleFeel;
    [SerializeField] private AudioClip normalBattleMusic;
    [SerializeField] private AudioClip bossBattleMusic;

    private BattlePresenter presenter;

    public override void Install(GameServices s)
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = (s.Battle != null && s.Battle.IsBoss)
                ? caveBackground
                : forestBackground;
        }

        if (battleSystem != null)
        {
            battleSystem.Construct(s.Party, s.Battle, s.Scenes);
            presenter = new BattlePresenter(battleSystem, battleView);

            if (battleAudio != null)
            {
                battleAudio.Construct(battleSystem, s.Audio);
            }

            if (battleFeel != null)
            {
                battleFeel.Construct(battleSystem);
            }

            AudioClip music = (s.Battle != null && s.Battle.IsBoss) ? bossBattleMusic : normalBattleMusic;
            s.Audio?.PlayBattleMusic(music);

            battleSystem.Begin();
        }
    }
}
