using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ElementVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer elementRenderer;
    [SerializeField] private GameObject deletionEffectPrefab; 
    [SerializeField] private Image targetFillImage;
    [SerializeField] private Image targetFillTimedImage;
    [SerializeField] private Image targetFillBombImage;
    [SerializeField] private GameObject breakEnemyHoverParticles;

    private Vector2Int gridPosition;
    private bool canSelect = true;
    private bool isTargetedByEnemy;
    private Coroutine fillCoroutine;
    private Coroutine timedFillCoroutine;

    private bool isTimedCountdownPaused;
    private float timedCountdownRemaining;
    private float timedCountdownTotal;


    private bool isBombTarget;
    private Color bombOriginalColor = Color.white;
    private bool bombAnimationStarted;

    private Element currentElement;

    private Sprite overrideSprite;
    private bool hasOverrideSprite;

    public Vector2Int GridPosition => gridPosition;
    public bool CanSelect => canSelect;
    public bool IsTargetedByEnemy => isTargetedByEnemy;

    public void Initialize(Vector2Int position, Element element)
    {
        gridPosition = position;
        UpdateVisual(element);
        
        if (!isTargetedByEnemy)
        {
            if (targetFillImage != null)
            {
                targetFillImage.enabled = false;
                targetFillImage.fillAmount = 0f;
            }
            if (targetFillTimedImage != null)
            {
                targetFillTimedImage.enabled = false;
                targetFillTimedImage.fillAmount = 0f;
            }
            if (targetFillBombImage != null)
            {
                targetFillBombImage.enabled = false;
                targetFillBombImage.fillAmount = 0f;
            }
        }
    }

    public void UpdateVisual(Element element)
    {
        if (element == null || elementRenderer == null)
            return;

        currentElement = element;

        if (isBombTarget)
            bombOriginalColor = element.Color;

        // Preserve active sprite override (e.g. hidden enemy "?" image)
        if (hasOverrideSprite && overrideSprite != null)
        {
            elementRenderer.sprite = overrideSprite;
            elementRenderer.color = Color.white;
            return;
        }

        elementRenderer.sprite = element.Sprite;
        elementRenderer.color = element.Color;
    }

    public void SetWorldPosition(Vector3 position)
    {
        if (elementRenderer != null)
            elementRenderer.transform.position = position;

        if (deletionEffectPrefab != null)
            deletionEffectPrefab.transform.position = position;

        if (targetFillImage != null)
        {
            targetFillImage.rectTransform.position = position;
        }

        if (targetFillTimedImage != null)
        {
            targetFillTimedImage.rectTransform.position = position;
        }

        if (targetFillBombImage != null)
        {
            targetFillBombImage.rectTransform.position = position;
        }
    }

    public Vector3 GetWorldPosition()
    {
        if (elementRenderer != null)
            return elementRenderer.transform.position;

        return transform.position;
    }

    public void SetLocalSpritePosition(Vector3 localPosition)
    {
        if (elementRenderer != null)
            elementRenderer.transform.localPosition = localPosition;

        if (deletionEffectPrefab != null)
            deletionEffectPrefab.transform.localPosition = localPosition;

        if (targetFillImage != null)
            targetFillImage.rectTransform.localPosition = localPosition;

        if (targetFillTimedImage != null)
            targetFillTimedImage.rectTransform.localPosition = localPosition;

        if (targetFillBombImage != null)
            targetFillBombImage.rectTransform.localPosition = localPosition;
    }

    public void SetSpriteWorldPosition(Vector3 worldPosition)
    {
        SetWorldPosition(worldPosition);
    }

    public void SetSpriteScale(Vector3 scale)
    {
        if (elementRenderer != null)
            elementRenderer.transform.localScale = scale;

        if (deletionEffectPrefab != null)
            deletionEffectPrefab.transform.localScale = scale;

        if (targetFillImage != null)
            targetFillImage.rectTransform.localScale = scale;

        if (targetFillTimedImage != null)
            targetFillTimedImage.rectTransform.localScale = scale;

        if (targetFillBombImage != null)
            targetFillBombImage.rectTransform.localScale = scale;
    }

    public void ResetSpriteTransform()
    {
        SetLocalSpritePosition(Vector3.zero);
        SetSpriteScale(Vector3.one);
    }

    public void SetSelectable(bool selectable)
    {
        if (selectable && currentElement != null && currentElement.Type == ElementType.Bomb)
            return;

        canSelect = selectable;
    }
    public void SetSpriteVisible(bool visible)
    {
        if (elementRenderer != null)
            elementRenderer.enabled = visible;
    }

    public void SetTargetedByEnemy(bool targeted)
    {
        isTargetedByEnemy = targeted;

        if (targetFillImage != null)
        {
            targetFillImage.enabled = targeted;
            if (!targeted)
                targetFillImage.fillAmount = 0f;
        }

        if (!targeted)
        {
            GameObject temp = Instantiate(breakEnemyHoverParticles, transform);
            temp.transform.SetParent(null);
            StopCountdown();
            StopTimedCountdown();

            if (isBombTarget)
                SetBombVisual(false, 0f);
        }
    }

    public void UpdateTargetVisual(int remainingMoves, int totalMoves, float animDuration, AnimationCurve fillCurve)
    {
        if (targetFillImage == null)
            return;

        float targetFill = 0f;
        if (totalMoves > 0)
            targetFill = Mathf.Clamp((float)remainingMoves / (float)totalMoves, 0, 1);

        if (!targetFillImage.enabled)
            targetFillImage.enabled = true;

        if (fillCoroutine != null)
            StopCoroutine(fillCoroutine);

        fillCoroutine = StartCoroutine(FillRoutine(targetFill, animDuration, fillCurve));
    }

    public void StartCountdown(float totalSeconds)
    {
        if (targetFillImage == null)
            return;

        totalSeconds = Mathf.Max(0.0001f, totalSeconds);

        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }

        targetFillImage.enabled = true;
        fillCoroutine = StartCoroutine(CountdownRoutine(totalSeconds));
    }

    public void StopCountdown()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }
    }

    public void StartTimedCountdown(float totalSeconds)
    {
        if (targetFillTimedImage == null)
            return;

        totalSeconds = Mathf.Max(0.0001f, totalSeconds);

        if (timedFillCoroutine != null)
        {
            StopCoroutine(timedFillCoroutine);
            timedFillCoroutine = null;
        }

        isTimedCountdownPaused = false;
        timedCountdownTotal = totalSeconds;
        timedCountdownRemaining = totalSeconds;

        targetFillTimedImage.enabled = true;
        timedFillCoroutine = StartCoroutine(TimedCountdownRoutine(totalSeconds));
    }

    public void PauseTimedCountdown()
    {
        if (timedFillCoroutine == null) return;
        isTimedCountdownPaused = true;
        StopCoroutine(timedFillCoroutine);
        timedFillCoroutine = null;
    }

    public void ResumeTimedCountdown()
    {
        if (!isTimedCountdownPaused) return;
        isTimedCountdownPaused = false;

        if (targetFillTimedImage == null || timedCountdownRemaining <= 0f) return;
        timedFillCoroutine = StartCoroutine(TimedCountdownRoutine(timedCountdownRemaining));
    }

    public void StopTimedCountdown()
    {
        if (timedFillCoroutine != null)
        {
            StopCoroutine(timedFillCoroutine);
            timedFillCoroutine = null;
        }

        if (targetFillTimedImage != null)
        {
            targetFillTimedImage.fillAmount = 0f;
            targetFillTimedImage.enabled = false;
        }
    }

    public void ClearTargetVisual()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
        }

        if (targetFillImage != null)
        {
            targetFillImage.fillAmount = 0f;
            targetFillImage.enabled = false;
        }
    }

    public void ClearTimedTargetVisual()
    {
        StopTimedCountdown();
    }

    public void SetBombVisual(bool active, float progress)
    {
        if (targetFillBombImage != null)
        {
            targetFillBombImage.enabled = active;
            targetFillBombImage.fillAmount = active ? Mathf.Clamp01(progress) : 0f;
        }

        if (elementRenderer != null)
        {
            if (active)
            {
                if (!isBombTarget)
                {
                    bombOriginalColor = elementRenderer.color;
                    isBombTarget = true;
                }
                elementRenderer.color = Color.Lerp(bombOriginalColor, new Color(1f, 0.3f, 0.3f), progress * 0.5f);
            }
            else if (isBombTarget)
            {
                elementRenderer.color = bombOriginalColor;
                isBombTarget = false;
            }
        }

        ScaleAnimator scaleAnimator = GetComponent<ScaleAnimator>();
        if (scaleAnimator != null)
        {
            if (active)
            {
                scaleAnimator.loopStrength = 0.05f + progress * 0.15f;
                if (!bombAnimationStarted)
                {
                    scaleAnimator.PlayAlways();
                    bombAnimationStarted = true;
                }
            }
            else if (bombAnimationStarted)
            {
                scaleAnimator.alwaysPlay = false;
                scaleAnimator.enabled = false;
                scaleAnimator.enabled = true;
                bombAnimationStarted = false;
            }
        }
    }

    public void SetOverrideSprite(Sprite sprite)
    {
        overrideSprite = sprite;
        hasOverrideSprite = sprite != null;

        if (elementRenderer != null && sprite != null)
        {
            elementRenderer.sprite = sprite;
            elementRenderer.color = Color.white;
        }
    }

    public void ClearOverrideSprite()
    {
        overrideSprite = null;
        hasOverrideSprite = false;

        if (currentElement != null)
            UpdateVisual(currentElement);
    }

    private IEnumerator FillRoutine(float targetFill, float duration, AnimationCurve curve)
    {
        if (targetFillImage == null)
            yield break;

        float startFill = targetFillImage.fillAmount;
        float elapsed = 0f;

        if (duration <= 0f)
        {
            targetFillImage.fillAmount = targetFill;

            if (targetFill <= 0f && !isTargetedByEnemy)
                targetFillImage.enabled = false;

            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float c = curve != null ? curve.Evaluate(t) : t;
            targetFillImage.fillAmount = Mathf.Lerp(startFill, targetFill, c);
            yield return null;
        }

        targetFillImage.fillAmount = targetFill;

        if (targetFill <= 0f && !isTargetedByEnemy)
            targetFillImage.enabled = false;

        fillCoroutine = null;
    }

    private IEnumerator CountdownRoutine(float totalSeconds)
    {
        if (targetFillImage == null)
            yield break;

        float elapsed = 0f;

        targetFillImage.fillAmount = 1f;

        while (elapsed < totalSeconds)
        {
            elapsed += Time.deltaTime;
            float remainingFraction = Mathf.Clamp01(Mathf.Max(0f, 1f - (elapsed / totalSeconds)));
            targetFillImage.fillAmount = remainingFraction;
            yield return null;
        }

        targetFillImage.fillAmount = 0f;
        fillCoroutine = null;
    }

    private IEnumerator TimedCountdownRoutine(float totalSeconds)
    {
        if (targetFillTimedImage == null)
            yield break;

        if (!isTimedCountdownPaused && timedCountdownRemaining <= 0f)
        {
            timedCountdownTotal = totalSeconds;
            timedCountdownRemaining = totalSeconds;
        }

        targetFillTimedImage.fillAmount = Mathf.Clamp01(timedCountdownRemaining / timedCountdownTotal);

        while (timedCountdownRemaining > 0f)
        {
            timedCountdownRemaining -= Time.deltaTime;
            timedCountdownRemaining = Mathf.Max(0f, timedCountdownRemaining);
            targetFillTimedImage.fillAmount = Mathf.Clamp01(timedCountdownRemaining / timedCountdownTotal);
            yield return null;
        }

        targetFillTimedImage.fillAmount = 0f;
        timedFillCoroutine = null;
    }
}