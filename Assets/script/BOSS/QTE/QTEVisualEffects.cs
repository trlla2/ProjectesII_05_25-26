using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class QTEVisualEffects : MonoBehaviour
{
    [Header("Screen Effects")]
    [SerializeField] private CanvasGroup darkOverlay;
    [SerializeField] private float overlayFadeDuration = 0.3f;
    [SerializeField] private float overlayTargetAlpha = 0.7f;
    
    [Header("Screen Flash")]
    [SerializeField] private Image flashImage;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.2f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem[] successParticles;
    [SerializeField] private ParticleSystem chargeAtatckParticles;

    [SerializeField] private float particleSequenceInterval = 0.15f;

    CameraShake cameraShake;

    private bool isActive = false;

    void Start()
    {
        if (darkOverlay != null)
            darkOverlay.gameObject.SetActive(false);
        if (flashImage != null)
            flashImage.gameObject.SetActive(false);
    }   
    public void ShowOverlay(bool instant = false)
    {
        if (darkOverlay == null) return;

        isActive = true;
        darkOverlay.gameObject.SetActive(true);

        if (instant)
        {
            darkOverlay.alpha = overlayTargetAlpha;
        }
        else
        {
            darkOverlay.DOFade(overlayTargetAlpha, overlayFadeDuration);
        }
    }

    public void HideOverlay(bool instant = false)
    {
        if (darkOverlay == null) return;

        isActive = false;

        if (instant)
        {
            darkOverlay.alpha = 0f;
            darkOverlay.gameObject.SetActive(false);
        }
        else
        {
            darkOverlay.DOFade(0f, overlayFadeDuration).OnComplete(() => 
            {
                darkOverlay.gameObject.SetActive(false);
            });
        }
    }

    public void PlayScreenFlash(Color? color = null, float? duration = null)
    {
        if (flashImage == null) return;

        Color useColor = color ?? flashColor;
        float useDuration = duration ?? flashDuration;

        StartCoroutine(ScreenFlashRoutine(useColor, useDuration));
    }

    private IEnumerator ScreenFlashRoutine(Color color, float duration)
    {
        flashImage.gameObject.SetActive(true);
        flashImage.color = new Color(color.r, color.g, color.b, 1f);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = 1f - (elapsed / duration);
            flashImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        flashImage.color = new Color(color.r, color.g, color.b, 0f);
        flashImage.gameObject.SetActive(false);
    }

    public void ShakeCamera(float intensity, float duration)
    {
        cameraShake = Camera.main.GetComponent<CameraShake>();
        if (cameraShake != null)
        {
            cameraShake.Shake(intensity, duration);
        }
    }

    public void PlaySuccessEffect()
    {
        PlayScreenFlash(new Color(1f, 0.8f, 0.2f, 1f), 0.5f);

        ShakeCamera(0.5f, 0.3f);

        if (successParticles != null && successParticles.Length > 0)
        {
            float totalDuration = successParticles.Length * particleSequenceInterval;
            StartCoroutine(SuccessParticleSequence());

            if (HapticsManager.Instance != null)
                StartCoroutine(SustainedHapticRoutine(HapticsManager.HapticTypes.HeavyImpact, totalDuration, particleSequenceInterval));
        }
        else if (HapticsManager.Instance != null)
        {
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.HeavyImpact);
        }
    }

    private IEnumerator SuccessParticleSequence()
    {
        foreach (ParticleSystem ps in successParticles)
        {
            if (ps != null)
                ps.Play();

            yield return new WaitForSeconds(particleSequenceInterval);
        }
    }

    private IEnumerator SustainedHapticRoutine(HapticsManager.HapticTypes hapticType, float totalDuration, float interval)
    {
        float elapsed = 0f;
        while (elapsed < totalDuration)
        {
            HapticsManager.Instance.GenerateHapticFeedback(hapticType);
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }
    }

    public void PlayChargeAttackEffect()
    {
        if (chargeAtatckParticles != null)
        {
            chargeAtatckParticles.Play();
        }
        if (HapticsManager.Instance != null)
        {
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.MediumImpact);
        }
    }

    public void PlayFailureEffect()
    {
        PlayScreenFlash(Color.red, 0.2f);

        ShakeCamera(0.2f, 0.15f);

        if (HapticsManager.Instance != null)
        {
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.Failure);
        }
    }

    public bool IsActive()
    {
        return isActive;
    }

    public void Clear()
    {
        HideOverlay(instant: true);
        StopAllCoroutines();
    }
}