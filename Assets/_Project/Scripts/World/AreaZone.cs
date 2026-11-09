using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class AreaZone : MonoBehaviour
{
    [SerializeField] private string areaName = "Desa Awal";
    [SerializeField] private AudioClip areaMusic;

    private AudioService audio;
    private PartnerHud hud;

    public string AreaName => areaName;

    public void Construct(AudioService audioService, PartnerHud partnerHud)
    {
        audio = audioService;
        hud = partnerHud;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        hud?.SetAreaName(areaName);
        if (audio != null && areaMusic != null)
        {
            audio.PlayWorldMusic(areaMusic);
        }
    }
}
