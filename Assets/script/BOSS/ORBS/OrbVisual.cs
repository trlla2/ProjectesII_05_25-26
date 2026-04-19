using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class OrbVisual : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Orb orb;              
    [SerializeField] private Slider chargeBar;
    public Transform ChargeElementTargetPos;
    [SerializeField] private Image orbImage;
    [SerializeField] private Image fillImage;
    [SerializeField] private ParticleSystem chargeParticles;
    [SerializeField] public ParticleSystem fullChargeParticles;

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

    private void Awake()
    {
        if (orb == null)
        {
            orb = GetComponentInParent<Orb>();
        }
    }

    private void OnEnable()
    {
        if (orb != null)
        {
            orb.OnChargeChanged += HandleChargeChanged;
            orb.OnStateChanged += SetState;
        }
    }

    private void OnDisable()
    {
        if (orb != null)
        {
            orb.OnChargeChanged -= HandleChargeChanged;
            orb.OnStateChanged -= SetState;
        }
    }

    private void Start()
    {
        animationEffects = GetComponent<OrbAnimationEffects>();
    }

    private void HandleChargeChanged(int previous, int current)
    {
        UpdateCharge(current, Orb.MaxCharge);

        int delta = Mathf.Max(0, current - previous);
        if (delta > 0)
        {
            PlayChargeEffect(delta);
        }
    }

    public void UpdateCharge(int current, int max)
    {
        float targetFill = (float)current / max;

        chargeBar.DOKill();
        chargeBar.DOValue(targetFill, fillAnimationDuration).SetEase(fillCurve);

        if (fillImage != null)
        {
            fillImage.DOKill();
            fillImage.DOFillAmount(targetFill, fillAnimationDuration).SetEase(fillCurve);
        }

        if (animationEffects != null && current > 0)
        {
            float shakeAmount = maxShakeAmount * targetFill;
            float shakeDuration = maxShakeDuration * targetFill;
            animationEffects.Shake(shakeAmount, shakeDuration);
        }
    }

    public void SetState(OrbState state)
    {
        switch (state)
        {
            case OrbState.Charged:
                PlayFullChargeEffect();
                break;
            case OrbState.Activated:
                PlayDepletedEffect();
                break;
        }
    }

    public void PlayChargeEffect(int chargeAmount)
    {
        if (chargeParticles != null)
        {
            var emmission = chargeParticles.emission;
            var rate = emmission.rateOverTime;

            rate.constant = chargeAmount * 5;

            chargeParticles.Play();
        }
    }

    public void PlayFullChargeEffect()
    {
        if (fullChargeParticles != null) { fullChargeParticles.Play(); }

        if (animationEffects != null)
        {
            animationEffects.SetScale(chargedScale, scaleDuration);
            animationEffects.ColorBlink();
            animationEffects.AnimateHand();
        }
    }

    public void PlayDepletedEffect()
    {
        if (animationEffects != null)
        {
            animationEffects.DisableBlink();
            animationEffects.SetScale(depletedScale, scaleDuration);
            animationEffects.Shake(maxShakeAmount * 1.5f, maxShakeDuration);
        }
    }

    public Vector3 GetPosition() { return transform.position; }
}