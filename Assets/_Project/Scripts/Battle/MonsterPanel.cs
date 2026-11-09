using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MonsterPanel : MonoBehaviour
{
    [SerializeField] private Text nameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text elementText;
    [SerializeField] private Image hpFill;
    [SerializeField] private Text hpText;
    [SerializeField] private Image expFill;
    [SerializeField] private SpriteRenderer monsterWorldSprite;

    private MonsterInstance currentMonster;
    private MonsterData displayData;
    private float targetHpFraction = 1f;
    private float currentHpFraction = 1f;
    private float targetExpFraction = 0f;
    private float currentExpFraction = 0f;
    private Vector3 baseWorldPos;

    private void Awake()
    {
        if (monsterWorldSprite != null)
        {
            baseWorldPos = monsterWorldSprite.transform.position;
        }
    }

    public void Show(MonsterInstance monster, bool isBoss = false, MonsterData displayAs = null)
    {
        currentMonster = monster;
        displayData = displayAs != null ? displayAs : monster.Data;

        if (nameText != null)
        {
            string bossStar = isBoss ? "★ " : "";
            nameText.text = $"{bossStar}{displayData.DisplayName}";
        }

        if (levelText != null)
        {
            levelText.text = $"Lv. {currentMonster.Level:D2}";
        }

        if (elementText != null)
        {
            elementText.text = displayData.Element.ToString().ToUpper();
        }

        targetHpFraction = (float)currentMonster.CurrentHP / Mathf.Max(1, currentMonster.MaxHP);
        currentHpFraction = targetHpFraction;
        if (hpFill != null)
        {
            hpFill.fillAmount = currentHpFraction;
            hpFill.color = HpColors.For(currentHpFraction);
        }

        if (hpText != null)
        {
            hpText.text = $"HP {currentMonster.CurrentHP} / {currentMonster.MaxHP}";
        }

        targetExpFraction = (float)currentMonster.Exp / Mathf.Max(1, currentMonster.ExpToNext);
        currentExpFraction = targetExpFraction;
        if (expFill != null)
        {
            expFill.fillAmount = currentExpFraction;
        }

        UpdateSpriteFrame(0);
    }

    public void RefreshStats(MonsterInstance monster)
    {
        currentMonster = monster;
        displayData = monster.Data;

        if (nameText != null) nameText.text = displayData.DisplayName;
        if (levelText != null) levelText.text = $"Lv. {currentMonster.Level:D2}";
        if (elementText != null) elementText.text = displayData.Element.ToString().ToUpper();

        targetHpFraction = (float)currentMonster.CurrentHP / Mathf.Max(1, currentMonster.MaxHP);
        targetExpFraction = (float)currentMonster.Exp / Mathf.Max(1, currentMonster.ExpToNext);

        if (hpText != null)
        {
            hpText.text = $"HP {currentMonster.CurrentHP} / {currentMonster.MaxHP}";
        }
    }

    private void Update()
    {
        // Smooth HP bar approach: Lerp with k = 1 - 0.85^(dt * 60)
        float k = 1f - Mathf.Pow(0.85f, Time.deltaTime * 60f);
        if (Mathf.Abs(currentHpFraction - targetHpFraction) > 0.001f)
        {
            currentHpFraction = Mathf.Lerp(currentHpFraction, targetHpFraction, k);
            if (hpFill != null)
            {
                hpFill.fillAmount = currentHpFraction;
                hpFill.color = HpColors.For(currentHpFraction);
            }
            if (hpText != null && currentMonster != null)
            {
                int displayHp = Mathf.Clamp(Mathf.RoundToInt(currentHpFraction * currentMonster.MaxHP), 0, currentMonster.MaxHP);
                hpText.text = $"HP {displayHp} / {currentMonster.MaxHP}";
            }
        }
        else if (currentHpFraction != targetHpFraction)
        {
            currentHpFraction = targetHpFraction;
            if (hpFill != null)
            {
                hpFill.fillAmount = currentHpFraction;
                hpFill.color = HpColors.For(currentHpFraction);
            }
            if (hpText != null && currentMonster != null)
            {
                hpText.text = $"HP {currentMonster.CurrentHP} / {currentMonster.MaxHP}";
            }
        }

        if (Mathf.Abs(currentExpFraction - targetExpFraction) > 0.001f)
        {
            currentExpFraction = Mathf.Lerp(currentExpFraction, targetExpFraction, k);
            if (expFill != null)
            {
                expFill.fillAmount = currentExpFraction;
            }
        }

        // Idle 2-frame bobbing
        if (monsterWorldSprite != null && displayData != null && displayData.Frames != null)
        {
            int frame = Mathf.FloorToInt(Time.time / 0.35f) % 2;
            UpdateSpriteFrame(frame);
        }
    }

    private void UpdateSpriteFrame(int frameIndex)
    {
        if (monsterWorldSprite == null || displayData == null || displayData.Frames == null) return;
        if (frameIndex >= 0 && frameIndex < displayData.Frames.Length)
        {
            monsterWorldSprite.sprite = displayData.Frames[frameIndex];
        }
    }

    public void PlayLunge(bool forwardRight)
    {
        StartCoroutine(LungeRoutine(forwardRight));
    }

    private IEnumerator LungeRoutine(bool forwardRight)
    {
        if (monsterWorldSprite == null) yield break;
        float duration = 0.26f;
        float distance = 14f / 16f; // 14 demo px
        Vector3 start = baseWorldPos;
        Vector3 dir = forwardRight ? Vector3.right : Vector3.left;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float offset = Mathf.Sin(t * Mathf.PI) * distance;
            monsterWorldSprite.transform.position = start + dir * offset;
            yield return null;
        }
        monsterWorldSprite.transform.position = start;
    }

    public void PlayHurt()
    {
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        if (monsterWorldSprite == null || displayData == null || displayData.Frames == null) yield break;
        Sprite normal = displayData.Frames[0];
        Sprite hurt = displayData.Frames.Length > 3 ? displayData.Frames[3] : normal;

        float elapsed = 0f;
        while (elapsed < 0.4f)
        {
            elapsed += Time.deltaTime;
            monsterWorldSprite.sprite = (Mathf.FloorToInt(elapsed / 0.08f) % 2 == 0) ? hurt : normal;
            yield return null;
        }
        monsterWorldSprite.sprite = normal;
    }

    public void PlayFaint()
    {
        StartCoroutine(FaintRoutine());
    }

    private IEnumerator FaintRoutine()
    {
        if (monsterWorldSprite == null) yield break;
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 start = monsterWorldSprite.transform.position;
        Color startCol = monsterWorldSprite.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            monsterWorldSprite.transform.position = start + Vector3.down * (20f / 16f) * t;
            monsterWorldSprite.color = new Color(startCol.r, startCol.g, startCol.b, 1f - t);
            yield return null;
        }
        monsterWorldSprite.gameObject.SetActive(false);
    }

    public void PlayEvolve(MonsterData from, MonsterData to, float duration = 2.4f)
    {
        StartCoroutine(EvolveRoutine(from, to, duration));
    }

    private IEnumerator EvolveRoutine(MonsterData from, MonsterData to, float duration)
    {
        if (monsterWorldSprite == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // frequency increases towards end
            float speed = Mathf.Lerp(4f, 20f, t);
            bool showTo = Mathf.FloorToInt(elapsed * speed) % 2 == 1;
            MonsterData curr = showTo ? to : from;
            monsterWorldSprite.sprite = curr.Frames[0];
            yield return null;
        }
        displayData = to;
        monsterWorldSprite.sprite = to.Frames[0];
    }
}
