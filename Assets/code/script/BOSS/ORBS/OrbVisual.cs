using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class OrbVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider chargeBar;
    public Transform ChargeElementTargetPos;
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

    [Header("Shake Settings")]
    [SerializeField] private float maxShakeAmount = 0.3f;
    [SerializeField] private float maxShakeDuration = 0.6f;

    [Header("Scale Settings")]
    [SerializeField] private float chargedScale = 1.2f;
    [SerializeField] private float depletedScale = 0.8f;
    [SerializeField] private float scaleDuration = 0.3f;

    private OrbAnimationEffects animationEffects;

    private void Start()
    {
        animationEffects = GetComponent<OrbAnimationEffects>();
    }

    public void UpdateCharge(int current, int max)
    {
        float targetFill = (float)current / max;

        chargeBar.DOKill();
        chargeBar.DOValue(targetFill, fillAnimationDuration).SetEase(fillCurve);

        if (animationEffects != null && current > 0)
        {
            float chargePercent = targetFill;
            float shakeAmount = maxShakeAmount * chargePercent;
            float shakeDuration = maxShakeDuration * chargePercent;
            animationEffects.Shake(shakeAmount, shakeDuration);
        }
    }

    public void SetState(OrbState state)
    {
        Color targetColor = Color.white;

        switch (state)
        {
            case OrbState.Inactive:
                orbImage.sprite = inactive;
                PlayDepletedEffect();
                if (animationEffects != null)
                    animationEffects.SetScale(depletedScale, scaleDuration);
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

    //future setup required
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

        if (animationEffects != null)
            animationEffects.SetScale(chargedScale, scaleDuration);
    }

    public void PlayDepletedEffect()
    {
        if (animationEffects != null)
            animationEffects.SetScale(depletedScale, scaleDuration);
    }

    public Vector3 GetPosition() { return transform.position; }
}