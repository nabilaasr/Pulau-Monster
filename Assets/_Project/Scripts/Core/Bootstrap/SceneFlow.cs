using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneFlow
{
    public const string BootstrapScene = "Scn_Bootstrap";
    public const string TitleScene = "Scn_Title";
    public const string WorldScene = "Scn_World";
    public const string BattleScene = "Scn_Battle";
    public const string EndScene = "Scn_End";

    private readonly MonoBehaviour host;
    private readonly ScreenFader fader;
    private readonly GameServices services;

    private string currentSceneName;
    private readonly List<GameObject> disabledWorldRoots = new List<GameObject>();

    public bool IsBusy { get; private set; }
    public bool HoldFade { get; set; }
    public string CurrentScene => currentSceneName;

    public SceneFlow(MonoBehaviour coroutineHost, ScreenFader screenFader, GameServices gameServices)
    {
        host = coroutineHost;
        fader = screenFader;
        services = gameServices;
    }

    public void GoTo(string sceneName)
    {
        if (IsBusy) return;
        host.StartCoroutine(GoToRoutine(sceneName));
    }

    public void StartBattle()
    {
        if (IsBusy) return;
        host.StartCoroutine(StartBattleRoutine());
    }

    public void EndBattle(BattleResult result)
    {
        if (IsBusy) return;
        host.StartCoroutine(EndBattleRoutine(result));
    }

    public IEnumerator ReleaseFade()
    {
        HoldFade = false;
        yield return fader.FadeIn(0.4f);
    }

    private IEnumerator GoToRoutine(string targetScene)
    {
        IsBusy = true;
        yield return fader.FadeOut(0.35f);

        // Unload previous scene if loaded
        if (!string.IsNullOrEmpty(currentSceneName))
        {
            Scene prev = SceneManager.GetSceneByName(currentSceneName);
            if (prev.isLoaded)
            {
                yield return SceneManager.UnloadSceneAsync(prev);
            }
        }

        // Load new scene additive
        yield return SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Additive);
        currentSceneName = targetScene;
        Scene next = SceneManager.GetSceneByName(targetScene);
        SceneManager.SetActiveScene(next);

        // Install scene dependencies
        GameObject[] roots = next.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            SceneInstaller installer = roots[i].GetComponentInChildren<SceneInstaller>();
            if (installer != null)
            {
                installer.Install(services);
                break;
            }
        }

        if (!HoldFade)
        {
            yield return fader.FadeIn(0.4f);
        }

        IsBusy = false;
    }

    private IEnumerator StartBattleRoutine()
    {
        IsBusy = true;
        services.Dialogue?.ForceClose();
        services.Input.Lock();
        yield return fader.FadeOut(0.35f);

        // Freeze world scene roots
        disabledWorldRoots.Clear();
        Scene worldScene = SceneManager.GetSceneByName(WorldScene);
        if (worldScene.isLoaded)
        {
            foreach (GameObject root in worldScene.GetRootGameObjects())
            {
                if (root.activeSelf)
                {
                    root.SetActive(false);
                    disabledWorldRoots.Add(root);
                }
            }
        }

        yield return SceneManager.LoadSceneAsync(BattleScene, LoadSceneMode.Additive);
        Scene battle = SceneManager.GetSceneByName(BattleScene);
        SceneManager.SetActiveScene(battle);

        foreach (GameObject root in battle.GetRootGameObjects())
        {
            SceneInstaller installer = root.GetComponentInChildren<SceneInstaller>();
            if (installer != null)
            {
                installer.Install(services);
                break;
            }
        }

        yield return fader.FadeIn(0.35f);
        IsBusy = false;
    }

    private IEnumerator EndBattleRoutine(BattleResult result)
    {
        IsBusy = true;
        yield return fader.FadeOut(0.35f);

        Scene battle = SceneManager.GetSceneByName(BattleScene);
        if (battle.isLoaded)
        {
            yield return SceneManager.UnloadSceneAsync(battle);
        }

        // Restore world scene roots
        Scene worldScene = SceneManager.GetSceneByName(WorldScene);
        if (worldScene.isLoaded)
        {
            SceneManager.SetActiveScene(worldScene);
            foreach (GameObject root in disabledWorldRoots)
            {
                if (root != null) root.SetActive(true);
            }
        }
        disabledWorldRoots.Clear();

        services.Input.Unlock();
        services.Battle.Finish(result);
        services.Audio?.ResumeWorldMusic();

        yield return fader.FadeIn(0.4f);
        IsBusy = false;
    }
}
