using System.Collections;
using UnityEngine;

public class BattleFeel : MonoBehaviour
{
    [SerializeField] private PixelBurst burst;
    [SerializeField] private VfxPool hitVfx;
    [SerializeField] private SpriteRenderer playerBody;
    [SerializeField] private SpriteRenderer enemyBody;
    [SerializeField] private Transform cameraRig;

    private BattleSystem battle;
    private Vector3 cameraHome;

    public void Construct(BattleSystem battleSystem)
    {
        battle = battleSystem;
        if (cameraRig != null) cameraHome = cameraRig.localPosition;

        if (battle != null)
        {
            battle.Hit += OnHit;
            battle.Healed += OnHealed;
        }
    }

    private void OnDestroy()
    {
        if (battle != null)
        {
            battle.Hit -= OnHit;
            battle.Healed -= OnHealed;
        }
        Time.timeScale = 1f;
    }

    private void OnHit(MonsterInstance target, int damage)
    {
        SpriteRenderer body = (target == battle.Player) ? playerBody : enemyBody;
        Vector3 center = (body != null) ? body.bounds.center : transform.position;

        hitVfx?.Play(center);

        Color c = battle.LastHitElement.HasValue
            ? ElementChart.GetColor(battle.LastHitElement.Value)
            : Color.white;

        burst?.Play(center, c, battle.LastHitElement.HasValue ? 22 : 12, 90f, 0.5f);

        StartCoroutine(Shake(battle.LastHitCritical ? 0.35f : 0.2f));
        StartCoroutine(HitStop());
    }

    private void OnHealed(MonsterInstance t)
    {
        Vector3 center = (playerBody != null) ? playerBody.bounds.center : transform.position;
        burst?.Play(center, HpColors.High, 16, 50f, 0.6f);
    }

    private IEnumerator Shake(float duration)
    {
        if (cameraRig == null) yield break;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float mag = Mathf.Lerp(0.15f, 0f, elapsed / duration);
            Vector3 offset = (Vector3)(Random.insideUnitCircle * mag);
            cameraRig.localPosition = cameraHome + offset;
            yield return null;
        }
        cameraRig.localPosition = cameraHome;
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(0.06f);
        Time.timeScale = 1.0f;
    }
}
