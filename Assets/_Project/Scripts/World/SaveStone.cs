using System.Collections;
using UnityEngine;

public class SaveStone : MonoBehaviour, IInteractable
{
    [SerializeField] private AudioClip saveClip;
    [SerializeField] private PixelBurst burst;

    private GameServices services;

    public void Construct(GameServices gameServices, PixelBurst pixelBurst = null)
    {
        services = gameServices;
        burst = pixelBurst;
    }

    public void Interact(PlayerInteractor who)
    {
        if (services == null || (services.Dialogue != null && services.Dialogue.IsRunning)) return;
        StartCoroutine(UseRoutine(who));
    }

    private IEnumerator UseRoutine(PlayerInteractor who)
    {
        if (services.Audio != null && saveClip != null)
        {
            services.Audio.PlaySfx(saveClip, 0f);
        }

        if (services.Party.Partner != null)
        {
            services.Party.Partner.HealFull();
        }

        burst?.Play(who.transform.position + Vector3.up * 0.5f, new Color32(110, 231, 242, 255), 16, 50f, 0.7f);

        bool saved = services.Save.Save(SaveMapper.Capture(services, who.transform.position));

        if (services.Dialogue != null)
        {
            yield return services.Dialogue.Say("", saved
                ? "Batu Simpan bersinar. HP partner pulih dan perjalanan tersimpan."
                : "Batu Simpan bersinar. HP partner pulih, tetapi penyimpanan gagal.");
            services.Dialogue.ForceClose();
        }
    }
}
