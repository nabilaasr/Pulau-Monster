using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CrystalSparkEffect : MonoBehaviour
{
    private Light2D crystalLight;
    private float baseIntensity;
    private float phase;
    private float nextSparkTime;
    private PixelBurst burst;

    private void Awake()
    {
        crystalLight = GetComponent<Light2D>();
        if (crystalLight != null)
        {
            baseIntensity = crystalLight.intensity;
        }
        phase = Random.Range(0f, 10f);
        nextSparkTime = Time.time + Random.Range(1f, 3f);
        burst = FindFirstObjectByType<PixelBurst>();
    }

    private void Update()
    {
        if (crystalLight == null) return;

        // Gentle electric pulse
        float pulse = Mathf.Sin(Time.time * 3.5f + phase) * 0.08f;
        float jitter = (Random.value < 0.08f) ? Random.Range(-0.06f, 0.06f) : 0f;

        // Apply pulse on top of current intensity when light is active
        if (crystalLight.intensity > 0.05f)
        {
            float targetMul = Mathf.Clamp(1f + pulse + jitter, 0.8f, 1.25f);
            crystalLight.pointLightOuterRadius = Mathf.Lerp(crystalLight.pointLightOuterRadius, 3.5f + pulse * 1.5f, Time.deltaTime * 5f);
        }

        // Periodic electric spark crackle
        if (Time.time >= nextSparkTime)
        {
            nextSparkTime = Time.time + Random.Range(1.8f, 3.5f);
            if (crystalLight.intensity > 0.1f && burst != null)
            {
                // Emit 2-3 tiny electric cyan sparks around crystal
                Vector3 sparkPos = transform.position + new Vector3(Random.Range(-0.35f, 0.35f), Random.Range(-0.1f, 0.45f), 0f);
                Color32 sparkColor = Random.value < 0.5f ? new Color32(80, 235, 255, 255) : new Color32(230, 253, 255, 255);
                burst.Play(sparkPos, sparkColor, Random.Range(2, 4), 18f, 0.28f);
            }
        }
    }
}
