using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OrbVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image chargeBar;
    [SerializeField] private Image orbImage;
    [SerializeField] private ParticleSystem chargeParticles;
    [SerializeField] private ParticleSystem fullChargeParticles;

    [Header("Sprites")]
    [SerializeField] private Sprite inactive;
    [SerializeField] private Sprite charging;
    [SerializeField] private Sprite charged;
    [SerializeField] private Sprite depleted;

    [Header("Animation")]
    [SerializeField] private float fillAnimationDuration = 0.3f;
    [SerializeField] private AnimationCurve fillCurve;

    private Coroutine fillCoroutine;

    public void UpdateCharge(int current, int max)
    {
        float targetFill = (float)current / max;

        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(AnimateFill(targetFill));
    }

    public void SetState(OrbState state)
    {
        Color targetColor = Color.white;

        switch (state)
        {
            case OrbState.Inactive:
                orbImage.sprite = inactive;
                PlayDepletedEffect();
                break;
            case OrbState.Charging:
                orbImage.sprite = charging;
                break;
            case OrbState.Charged:
                orbImage.sprite = charged;
                PlayFullChargeEffect();
                break;
            case OrbState.Activated:
                orbImage.sprite = depleted;
                PlayDepletedEffect();
                break;
        }

        orbImage.color = targetColor;

        if (state == OrbState.Charged && fullChargeParticles != null)
            fullChargeParticles.Play();
    }

    public void PlayChargeEffect(int chargeAmount)
    {
        if (chargeParticles != null)
        {
            var burst = chargeParticles.emission.GetBurst(0);
            burst.count = chargeAmount;
            chargeParticles.emission.SetBurst(0, burst);
            chargeParticles.Play();
        }
    }

    public void PlayFullChargeEffect()
    {
        if (fullChargeParticles != null)
            fullChargeParticles.Play();
    }

    public void PlayDepletedEffect()
    {
    }

    public Vector3 GetPosition() { return transform.position; }

    private IEnumerator AnimateFill(float target)
    {
        float start = chargeBar.fillAmount;
        float elapsed = 0f;

        while (elapsed < fillAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = fillCurve.Evaluate(elapsed / fillAnimationDuration);
            chargeBar.fillAmount = Mathf.Lerp(start, target, t);
            yield return null;
        }

        chargeBar.fillAmount = target;
    }
}