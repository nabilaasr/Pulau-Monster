using System.Collections;
using UnityEngine;

public class WorldInstaller : SceneInstaller
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private PlayerInteractor interactor;
    [SerializeField] private PartnerFollower partnerFollower;
    [SerializeField] private GrassRustle grassRustle;
    [SerializeField] private PartnerHud partnerHud;
    [SerializeField] private PixelBurst burst;
    [SerializeField] private Vector2 respawnPoint = new Vector2(21.5f, 6.0f);

    private GameServices services;

    public override void Install(GameServices s)
    {
        services = s;

        if (player != null) player.Construct(s.Input);
        if (interactor != null) interactor.Construct(s.Input);
        if (partnerFollower != null) partnerFollower.Construct(s.Party, player);
        if (partnerHud != null) partnerHud.Construct(s.Party);

        foreach (WildMonster wild in FindInScene<WildMonster>())
        {
            wild.Construct(s.Battle, s.Scenes, s.Party, player.transform, s.Dialogue, s.Input, s.Flags);
        }

        foreach (Npc npc in FindInScene<Npc>())
        {
            npc.Construct(s);
        }

        foreach (FlagToggle ft in FindInScene<FlagToggle>())
        {
            ft.Construct(s.Flags);
        }

        foreach (AreaZone az in FindInScene<AreaZone>())
        {
            az.Construct(s.Audio, partnerHud);
        }

        foreach (SaveStone ss in FindInScene<SaveStone>())
        {
            ss.Construct(s, burst);
        }

        if (s.Battle != null)
        {
            s.Battle.Ended -= OnBattleEnded;
            s.Battle.Ended += OnBattleEnded;
        }

        if (s.Save != null && s.Save.PendingLoad != null)
        {
            player.Teleport(s.Save.PendingLoad.playerPosition, Vector2.down);
            s.Save.PendingLoad = null;
            s.Toast?.Show("Perjalanan dilanjutkan");
        }
        else if (s.Flags != null && !s.Flags.Has(StoryFlags.IntroSeen))
        {
            s.Scenes.HoldFade = true;
            StartCoroutine(IntroRoutine());
        }
    }

    private void OnDestroy()
    {
        if (services != null && services.Battle != null)
        {
            services.Battle.Ended -= OnBattleEnded;
        }
    }

    private void OnBattleEnded(BattleResult result, bool wasBoss)
    {
        if (result == BattleResult.Lose)
        {
            if (player != null) player.Teleport(respawnPoint, Vector2.up);
            if (services != null && services.Party != null && services.Party.Partner != null)
            {
                services.Party.Partner.HealFull();
            }
        }
        else if (result == BattleResult.Win && wasBoss)
        {
            StartCoroutine(EndingRoutine());
        }
    }

    private IEnumerator IntroRoutine()
    {
        services.Dialogue.Run(IntroLines());
        yield return null;
        while (services.Dialogue.IsRunning) yield return null;

        services.Flags.Set(StoryFlags.IntroSeen);
        yield return services.Scenes.ReleaseFade();
        services.Toast?.Show("Temui Tetua Rahmat (kanan jalan desa)");
    }

    private IEnumerator IntroLines()
    {
        yield return services.Dialogue.Narrate("Di ujung Pulau Batam ada desa kecil yang hidup berdampingan dengan monster.");
        yield return services.Dialogue.Narrate("Kini giliranmu membimbing monster pertamamu dan menjelajahi pulau ini!");
    }

    private IEnumerator EndingRoutine()
    {
        while (services.Scenes.IsBusy) yield return null;

        services.Dialogue.Run(EndingLines());
        yield return null;
        while (services.Dialogue.IsRunning) yield return null;

        Vector3 pos = player != null ? player.transform.position : Vector3.zero;
        services.Save.Save(SaveMapper.Capture(services, pos));
        services.Scenes.GoTo(SceneFlow.EndScene);
    }

    private IEnumerator EndingLines()
    {
        yield return services.Dialogue.Narrate("Raijugar menunduk dan membiarkanmu lewat...");
        yield return services.Dialogue.Narrate("Kedamaian telah kembali ke Gua Petir berkat keberanianmu dan partnermu!");
    }
}
