using UnityEngine;
using UnityEngine.UI;

public class PartnerHud : MonoBehaviour
{
    [Header("Partner Info")]
    [SerializeField] private GameObject partnerPanel;
    [SerializeField] private Image portraitImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Image hpFill;
    [SerializeField] private Text hpText;
    [SerializeField] private Image expFill;

    [Header("Area & Potions Info")]
    [SerializeField] private GameObject areaPanel;
    [SerializeField] private Text areaNameText;
    [SerializeField] private Text potionsText;

    private PlayerParty party;

    public void Construct(PlayerParty playerParty)
    {
        party = playerParty;
        if (party != null)
        {
            party.PartnerChanged += RefreshPartner;
            party.PotionsChanged += RefreshPotions;
            if (party.Partner != null)
            {
                party.Partner.Changed += RefreshHpAndExp;
            }
        }
        RefreshPartner();
        RefreshPotions();
    }

    private void OnDestroy()
    {
        if (party != null)
        {
            party.PartnerChanged -= RefreshPartner;
            party.PotionsChanged -= RefreshPotions;
            if (party.Partner != null)
            {
                party.Partner.Changed -= RefreshHpAndExp;
            }
        }
    }

    public void SetAreaName(string areaName)
    {
        if (areaNameText != null) areaNameText.text = areaName;
    }

    private void RefreshPotions()
    {
        if (potionsText != null && party != null)
        {
            potionsText.text = $"Ramuan ×{party.Potions}";
        }
    }

    private void RefreshPartner()
    {
        if (partnerPanel == null) return;

        MonsterInstance m = party != null ? party.Partner : null;
        partnerPanel.SetActive(m != null);

        if (m == null) return;

        m.Changed -= RefreshHpAndExp;
        m.Changed += RefreshHpAndExp;

        if (nameText != null) nameText.text = m.Data.DisplayName;
        if (levelText != null) levelText.text = $"Lv. {m.Level:D2}";

        if (portraitImage != null && m.Data.Frames != null && m.Data.Frames.Length > 0)
        {
            portraitImage.sprite = m.Data.Frames[0];
        }

        RefreshHpAndExp();
    }

    private void RefreshHpAndExp()
    {
        MonsterInstance m = party != null ? party.Partner : null;
        if (m == null) return;

        if (nameText != null) nameText.text = m.Data.DisplayName;
        if (levelText != null) levelText.text = $"Lv. {m.Level:D2}";

        float hpFrac = (float)m.CurrentHP / Mathf.Max(1, m.MaxHP);
        if (hpFill != null)
        {
            hpFill.fillAmount = hpFrac;
            hpFill.color = HpColors.For(hpFrac);
        }

        if (hpText != null)
        {
            hpText.text = $"HP {m.CurrentHP} / {m.MaxHP}";
        }

        float expFrac = (float)m.Exp / Mathf.Max(1, m.ExpToNext);
        if (expFill != null)
        {
            expFill.fillAmount = expFrac;
        }
    }
}
