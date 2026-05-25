using System;
using UnityEngine;
using DG.Tweening;

// Pure presentation layer for item effects.
// No gameplay state changes happen here, only visual/audio/haptic feedback.
public static class ItemEffectFeedback
{
    private const float XMarkerScaleDuration = 0.1f;
    private const int ElementSortingOrder = 100;
    private const int MarkerSortingOrder = 101;
    private const float CameraShakeIntensity = 0.1f;
    private const float CameraShakeDuration = 0.08f;
    private const string ElementPlacedSfx = "ReplaceElementGrid";

    public static GameObject SpawnXMarker(Vector3 worldPos, Sprite sprite)
    {
        GameObject xObj = new GameObject("ItemEffect_XMarker");
        SpriteRenderer sr = xObj.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = Color.white;
        sr.sortingOrder = MarkerSortingOrder;
        xObj.transform.position = worldPos;
        xObj.transform.localScale = Vector3.zero;
        xObj.transform.DOScale(Vector3.one, XMarkerScaleDuration).SetEase(Ease.OutBack);
        return xObj;
    }

    public static void AnimateClonedElement(Element element, Vector3 startPos, Vector3 targetPos,
        int curveDirection, float duration, float curveStrength, Action onArrived)
    {
        GameObject tempVisual = new GameObject("ItemEffect_Element");
        SpriteRenderer sr = tempVisual.AddComponent<SpriteRenderer>();
        sr.sprite = element.Sprite;
        sr.color = element.Color;
        sr.sortingOrder = ElementSortingOrder;
        tempVisual.transform.position = startPos;
        tempVisual.transform.localScale = Vector3.one * 0.2f;

        Tween moveTween;
        if (curveDirection == 0)
        {
            moveTween = tempVisual.transform.DOMove(targetPos, duration).SetEase(Ease.OutQuad);
        }
        else
        {
            Vector3 midPoint = (startPos + targetPos) / 2f;
            midPoint.x += curveDirection * curveStrength;

            Vector3[] path = { startPos, midPoint, targetPos };
            moveTween = tempVisual.transform.DOPath(path, duration, PathType.CatmullRom).SetEase(Ease.OutQuad);
        }

        tempVisual.transform.DOScale(Vector3.one, duration).SetEase(Ease.OutBack);

        moveTween.OnComplete(() =>
        {
            onArrived?.Invoke();
            GameObject.Destroy(tempVisual);
        });
    }

    public static void PlayElementPlacedFeedback()
    {
        if (Camera.main != null)
        {
            CameraShake cameraShake = Camera.main.GetComponent<CameraShake>();
            cameraShake?.Shake(CameraShakeIntensity, CameraShakeDuration);
        }

        if (HapticsManager.Instance != null)
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.LightImpact);

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySFXByName(ElementPlacedSfx);
    }
}
