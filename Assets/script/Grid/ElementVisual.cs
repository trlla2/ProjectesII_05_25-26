using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ElementVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer elementRenderer;
    [SerializeField] private Image targetFillImage;
    [SerializeField] private Image targetFillTimedImage;
    [SerializeField] private Image targetFillBombImage;
    [SerializeField] private GameObject breakEnemyHoverParticles;
    [SerializeField] private BlockadeVisual blockadeVisual;

    [HideInInspector]
    public Highlighter highlighter;

    private Vector2Int gridPosition;
    private bool canSelect = true;
    private bool isTargetedByEnemy;
    private Coroutine fillCoroutine;

    private bool isTimedCountdownPaused;
    private float timedCountdownRemaining;
    private float timedCountdownTotal;
    private Tween timedFillTween;

    private bool isBombTarget;
    private Color bombOriginalColor = Color.white;
    private bool bombAnimationStarted;

    private Element currentElement;

    private Sprite overrideSprite;
    private bool hasOverrideSprite;

    private Tween hoverMoveTween;
    private Tween hoverRotateTween;
    private Tween hoverScaleTween;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector3 initialScale;

    private bool isHoverAnimating;

    private bool isBlocked;
    private float blockBreakProgress;
    private bool isSwapTargeted;

    public Vector2Int GridPosition => gridPosition;
    public bool CanSelect => canSelect;
    public bool IsTargetedByEnemy => isTargetedByEnemy;
    public bool IsBlocked => isBlocked;
    public float BlockBreakProgress => blockBreakProgress;
    public bool IsSwapTargeted => isSwapTargeted;

    public void Initialize(Vector2Int position, Element element)
    {
        StopHoverAnimation();

        highlighter = GetComponent<Highlighter>();
        highlighter?.SetHighlight(false);

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

        //Preserve active sprite override
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

        if(blockadeVisual != null)
            blockadeVisual.transform.position = position;

        if (targetFillImage != null)
            targetFillImage.rectTransform.position = position;

        if (targetFillTimedImage != null)
            targetFillTimedImage.rectTransform.position = position;

        if (targetFillBombImage != null)
            targetFillBombImage.rectTransform.position = position;
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

        if (targetFillImage != null)
            targetFillImage.rectTransform.localPosition = localPosition;

        if(blockadeVisual != null)
            blockadeVisual.transform.localPosition = localPosition;

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

        if(blockadeVisual != null)
            blockadeVisual.transform.localScale = scale;

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

        if (selectable && isBlocked)
            return;

        canSelect = selectable;
    }

    public void SetSpriteVisible(bool visible)
    {
        if (elementRenderer != null)
            elementRenderer.enabled = visible;
    }

    public void ShowInactive(bool inactive)
    {
        if (elementRenderer == null)
            return;

        Color currentColor = elementRenderer.color;
        currentColor.a = inactive ? 0.5f : 1f;
        elementRenderer.color = currentColor;
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

    public void StopCountdown()
    {
        if (fillCoroutine != null)
        {
            StopCoroutine(fillCoroutine);
            fillCoroutine = null;
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

    public void StartHoverAnimation(bool highlight)
    {
        StopHoverAnimation();

        if (elementRenderer == null) return;

        initialPosition = elementRenderer.transform.localPosition;
        initialRotation = elementRenderer.transform.localRotation;
        initialScale = elementRenderer.transform.localScale;

        if (!highlight) return;

        isHoverAnimating = true;

        TweenCallback onKillReset = () =>
        {
            if (elementRenderer == null) return;
            elementRenderer.transform.localPosition = initialPosition;
            elementRenderer.transform.localRotation = initialRotation;
            elementRenderer.transform.localScale = initialScale;
        };

        hoverMoveTween = elementRenderer.transform.DOLocalMoveY(initialPosition.y + 0.1f, 0.9f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).OnKill(onKillReset);
        hoverRotateTween = elementRenderer.transform.DOLocalRotate(new Vector3(0f, 0f, 3f), 1.0f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).OnKill(onKillReset);
        hoverScaleTween = elementRenderer.transform.DOScale(1.05f, 0.9f)
            .SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo).OnKill(onKillReset);
    }

    public void StopHoverAnimation()
    {
        hoverMoveTween?.Kill();
        hoverRotateTween?.Kill();
        hoverScaleTween?.Kill();

        hoverMoveTween = null;
        hoverRotateTween = null;
        hoverScaleTween = null;

        if (!isHoverAnimating || elementRenderer == null)
        {
            isHoverAnimating = false;
            return;
        }

        isHoverAnimating = false;
        elementRenderer.transform.localPosition = initialPosition;
        elementRenderer.transform.localRotation = initialRotation;
        elementRenderer.transform.localScale = initialScale;
    }

    public void StartTimedCountdown(float totalSeconds)
    {
        if (targetFillTimedImage == null) return;

        totalSeconds = Mathf.Max(0.0001f, totalSeconds);

        timedFillTween?.Kill();
        timedCountdownTotal = totalSeconds;
        timedCountdownRemaining = totalSeconds;
        isTimedCountdownPaused = false;

        targetFillTimedImage.enabled = true;
        targetFillTimedImage.fillAmount = 1f;

        timedFillTween = DOTween.To(() => timedCountdownRemaining,
            x =>
            {
                timedCountdownRemaining = x;
                targetFillTimedImage.fillAmount = Mathf.Clamp(x / timedCountdownTotal, 0f, 1f);
            }
            , 0f, totalSeconds).SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            targetFillTimedImage.fillAmount = 0f;
            timedFillTween = null;
        });
    }

    public void PauseTimedCountdownDOTween()
    {
        if (timedFillTween == null) return;
        timedFillTween.Pause();
        isTimedCountdownPaused = true;
    }

    public void ResumeTimedCountdownDOTween()
    {
        if (timedFillTween == null || !isTimedCountdownPaused) return;
        timedFillTween.Play();
        isTimedCountdownPaused = false;
    }

    public void StopTimedCountdown()
    {
        timedFillTween?.Kill();
        timedFillTween = null;
        isTimedCountdownPaused = false;

        if (targetFillTimedImage != null)
        {
            targetFillTimedImage.fillAmount = 0f;
            targetFillTimedImage.enabled = false;
        }
    }

    public void ExtendTimedCountdown(float newRemainingTime, float newTotalTime)
    {
        if (targetFillTimedImage == null) return;

        timedFillTween?.Kill();

        timedCountdownRemaining = newRemainingTime;
        timedCountdownTotal = newTotalTime;

        float targetFill = Mathf.Clamp01(newRemainingTime / newTotalTime);

        Sequence seq = DOTween.Sequence();

        seq.Append(DOTween.To(
            () => targetFillTimedImage.fillAmount,
            x => targetFillTimedImage.fillAmount = x,
            targetFill, 0.25f)
        .SetEase(Ease.OutQuad));

        seq.AppendCallback(() =>
        {
            timedFillTween = DOTween.To(
                () => timedCountdownRemaining,
                x =>
                {
                    timedCountdownRemaining = x;
                    targetFillTimedImage.fillAmount = Mathf.Clamp01(x / timedCountdownTotal);
                },
                0f,
                newRemainingTime)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                targetFillTimedImage.fillAmount = 0f;
                timedFillTween = null;
            });

            if (isTimedCountdownPaused)
                timedFillTween.Pause();
        });

        timedFillTween = seq;
    }


    public void SetBlocked(bool blocked)
    {
        isBlocked = blocked;
        canSelect = !blocked;

        if (blockadeVisual != null)
        {
            blockadeVisual.SetActive(blocked);
            if (blocked)
            {
                blockBreakProgress = 0f;
                blockadeVisual.UpdateProgress(0f);
            }
        }
    }

    public void UpdateBlockProgress(float normalizedProgress)
    {
        if (!isBlocked) return;

        blockBreakProgress = normalizedProgress;

        if (blockadeVisual != null)
            blockadeVisual.UpdateProgress(normalizedProgress);
    }

    public void ResetBlockProgress()
    {
        blockBreakProgress = 0f;
        if (blockadeVisual != null)
            blockadeVisual.UpdateProgress(0f);
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

    public void PlaySwapWarning(float duration, System.Action onComplete = null)
    {
        isSwapTargeted = true;

        System.Action setFlag = () =>
        {
            isSwapTargeted = false;
            onComplete?.Invoke();
        };

        ScaleAnimator scaleAnimator = GetComponent<ScaleAnimator>();
        if (scaleAnimator != null)
            scaleAnimator.PlayDelayBlink(0.1f, 1f, duration, setFlag);
        else
            setFlag.Invoke();
    }
}