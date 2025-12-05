using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ElementVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer elementRenderer;
    [SerializeField] private GameObject deletionEffectPrefab; 
    [SerializeField] private Image targetFillImage;

    private Vector2Int gridPosition;
    private bool canSelect = true;
    private bool isTargetedByEnemy;
    private Coroutine fillCoroutine;

    public Vector2Int GridPosition => gridPosition;
    public bool CanSelect => canSelect;
    public bool IsTargetedByEnemy => isTargetedByEnemy;

    public void Initialize(Vector2Int position, Element element)
    {
        gridPosition = position;
        UpdateVisual(element);
        if (targetFillImage != null)
        {
            if (!isTargetedByEnemy)
            {
                targetFillImage.enabled = false;
                targetFillImage.fillAmount = 0f;
            }
            else
            {
                targetFillImage.enabled = true;
            }
        }
    }

    public void UpdateVisual(Element element)
    {
        if (element == null || elementRenderer == null)
            return;

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
    }

    public void ResetSpriteTransform()
    {
        SetLocalSpritePosition(Vector3.zero);
        SetSpriteScale(Vector3.one);
    }

    public void SetSelectable(bool selectable)
    {
        canSelect = selectable;
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
            StopCountdown();
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
}