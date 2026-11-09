using UnityEngine;
using UnityEngine.UI;

public class EndInstaller : SceneInstaller
{
    [SerializeField] private Image partnerImage;
    [SerializeField] private Text partnerText;
    [SerializeField] private Text timeText;
    [SerializeField] private Button clickCatcher;

    private GameServices services;
    private MonsterInstance partner;
    private float readyTime;
    private bool leaving;

    public override void Install(GameServices s)
    {
        services = s;
        partner = s.Party.Partner;

        int totalSeconds = Mathf.FloorToInt(s.Party.Playtime);
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;

        if (partnerText != null)
        {
            partnerText.text = partner != null ? $"{partner.Data.DisplayName} · Lv {partner.Level}" : "";
        }

        if (timeText != null)
        {
            timeText.text = $"Waktu bermain {minutes} menit {seconds} detik";
        }

        if (clickCatcher != null)
        {
            clickCatcher.onClick.RemoveAllListeners();
            clickCatcher.onClick.AddListener(Leave);
        }

        readyTime = Time.unscaledTime + 0.5f;
    }

    private void Update()
    {
        if (partner != null && partnerImage != null && partner.Data != null && partner.Data.Frames != null)
        {
            int frame = Mathf.FloorToInt(Time.unscaledTime / 0.35f) % 2;
            if (frame < partner.Data.Frames.Length)
            {
                partnerImage.sprite = partner.Data.Frames[frame];
            }
        }

        if (MenuInput.Confirm)
        {
            Leave();
        }
    }

    private void Leave()
    {
        if (leaving || Time.unscaledTime < readyTime) return;
        leaving = true;
        services.Scenes.GoTo(SceneFlow.TitleScene);
    }
}
