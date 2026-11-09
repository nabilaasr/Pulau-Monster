using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Collider2D))]
public class LightZone : MonoBehaviour
{
    [SerializeField] private Light2D globalLight;
    [SerializeField] private Light2D playerLight;
    [SerializeField] private Light2D[] extraLights;
    [SerializeField] private float insideIntensity = 0.08f;
    [SerializeField] private float fadeSpeed = 2f;

    private float target = 1f;

    public void Setup(Light2D global, Light2D player, Light2D[] crystals)
    {
        globalLight = global;
        playerLight = player;
        extraLights = crystals;
    }

    private void OnTriggerEnter2D(Collider2D o)
    {
        if (o.CompareTag("Player")) target = insideIntensity;
    }

    private void OnTriggerExit2D(Collider2D o)
    {
        if (o.CompareTag("Player")) target = 1f;
    }

    private void Update()
    {
        if (globalLight == null) return;

        globalLight.intensity = Mathf.MoveTowards(globalLight.intensity, target, fadeSpeed * Time.deltaTime);
        float dark = 1f - globalLight.intensity;

        if (playerLight != null) playerLight.intensity = dark * 0.75f;
        if (extraLights != null)
        {
            for (int i = 0; i < extraLights.Length; i++)
            {
                if (extraLights[i] != null) extraLights[i].intensity = dark * 1.3f;
            }
        }
    }
}
