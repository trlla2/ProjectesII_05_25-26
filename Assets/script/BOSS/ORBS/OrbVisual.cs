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
    [SerializeField] private ParticleSystem chargeParticles;
    [SerializeField] public ParticleSystem fullChargeParticles;

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

        //number of particles = amount gained this event
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
        if (fullChargeParticles != null) { fullChargeParticles.Play();}

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