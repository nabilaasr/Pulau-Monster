using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TitleInstaller : SceneInstaller
{
    [SerializeField] private RectTransform backgroundRect;
    [SerializeField] private Image[] starterImages;
    [SerializeField] private MonsterData[] starterData;
    [SerializeField] private TextMenu menu;
    [SerializeField] private AudioClip titleMusic;

    private GameServices services;
    private readonly List<string> options = new List<string>();
    private float bgBaseX;

    private void Awake()
    {
        if (backgroundRect != null) bgBaseX = backgroundRect.anchoredPosition.x;
    }

    public override void Install(GameServices s)
    {
        services = s;
        options.Clear();
        if (s.Save.HasSave()) options.Add("Lanjutkan");
        options.Add("Mulai baru");
        options.Add("Keluar");

        menu.Picked += OnMenuPicked;
        menu.Open(options.ToArray());

        s.Audio?.PlayWorldMusic(titleMusic);
    }

    private void OnDestroy()
    {
        if (menu != null) menu.Picked -= OnMenuPicked;
    }

    private void Update()
    {
        // Background smooth horizontal parallax drift
        if (backgroundRect != null)
        {
            float offset = Mathf.Sin(Time.unscaledTime / 6f) * 60f; // drift within canvas
            backgroundRect.anchoredPosition = new Vector2(bgBaseX + offset, backgroundRect.anchoredPosition.y);
        }

        // 4 starters idle animation
        if (starterImages != null && starterData != null)
        {
            int frame = Mathf.FloorToInt(Time.unscaledTime / 0.35f) % 2;
            for (int i = 0; i < starterImages.Length && i < starterData.Length; i++)
            {
                if (starterImages[i] != null && starterData[i] != null && starterData[i].Frames != null)
                {
                    if (frame < starterData[i].Frames.Length)
                    {
                        starterImages[i].sprite = starterData[i].Frames[frame];
                    }
                }
            }
        }
    }

    private void OnMenuPicked(int index)
    {
        string choice = options[index];
        if (choice == "Lanjutkan")
        {
            ContinueGame();
        }
        else if (choice == "Mulai baru")
        {
            NewGame();
        }
        else if (choice == "Keluar")
        {
            QuitGame();
        }
    }

    private void ContinueGame()
    {
        SaveData data = services.Save.Load();
        if (data == null)
        {
            NewGame();
            return;
        }

        SaveMapper.Apply(data, services);
        services.Save.PendingLoad = data;
        services.Scenes.GoTo(SceneFlow.WorldScene);
    }

    private void NewGame()
    {
        services.Party.Reset();
        services.Flags.Load(new List<string>());
        services.Save.PendingLoad = null;
        services.Scenes.GoTo(SceneFlow.WorldScene);
    }

    private void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
