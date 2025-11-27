using System.Collections;
using UnityEngine;

public class ElementVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer elementRenderer;
    [SerializeField] private SpriteRenderer blinkSprite;
    [SerializeField] private GameObject deletionEffectPrefab;

    public SpriteRenderer deletionSpriteRenderer;

    private Vector2Int gridPosition;
    private bool canSelect = true;
    private bool isTargetedByEnemy;
    private Coroutine blinkCoroutine;

    public Vector2Int GridPosition => gridPosition;
    public bool CanSelect => canSelect;
    public bool IsTargetedByEnemy => isTargetedByEnemy;

    public void Initialize(Vector2Int position, Element element)
    {
        if (deletionEffectPrefab != null)
            deletionSpriteRenderer = deletionEffectPrefab.GetComponent<SpriteRenderer>();

        gridPosition = position;
        UpdateVisual(element);

        if (blinkSprite != null)
            blinkSprite.gameObject.SetActive(false);
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

        if (blinkSprite != null)
            blinkSprite.transform.position = position;

        if (deletionSpriteRenderer != null)
            deletionSpriteRenderer.transform.position = position;
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

        if (blinkSprite != null)
            blinkSprite.transform.localPosition = localPosition;

        if (deletionSpriteRenderer != null)
            deletionSpriteRenderer.transform.localPosition = localPosition;
    }

    public void SetSpriteWorldPosition(Vector3 worldPosition)
    {
        SetWorldPosition(worldPosition);
    }

    public void SetSpriteScale(Vector3 scale)
    {
        if (elementRenderer != null)
            elementRenderer.transform.localScale = scale;

        if (blinkSprite != null)
            blinkSprite.transform.localScale = scale;

        if (deletionSpriteRenderer != null)
            deletionSpriteRenderer.transform.localScale = scale;
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
    }

    public void StartBlinkingAnimation(float duration, AnimationCurve blinkCurve)
    {
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        //use deletionSpriteRenderer as source sprite if available
        blinkCoroutine = StartCoroutine(BlinkRoutine(duration, blinkCurve, deletionSpriteRenderer));
    }

    public void StopBlinkingAnimation()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        if (blinkSprite != null)
            blinkSprite.gameObject.SetActive(false);
    }

    public void PlayDeletionEffect()
    {
        if (deletionEffectPrefab != null)
        {
            Instantiate(deletionEffectPrefab, GetWorldPosition(), Quaternion.identity);
        }
    }

    private IEnumerator BlinkRoutine(float duration, AnimationCurve curve, SpriteRenderer sprite)
    {
        if (blinkSprite == null || sprite == null)
            yield break;

        // copy sprite so the blink sprite matches the deletion / source sprite
        blinkSprite.sprite = sprite.sprite;
        blinkSprite.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / duration;
            float alpha = curve.Evaluate(normalizedTime);

            Color color = blinkSprite.color;
            color.a = alpha;
            blinkSprite.color = color;

            yield return null;
        }

        blinkSprite.gameObject.SetActive(false);
    }
}