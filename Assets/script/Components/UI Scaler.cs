using UnityEngine;
using DG.Tweening;

public class UIScaler : MonoBehaviour
{
    public bool AnimateAllways = true;
    public float ScaleUpFactor = 1.2f;
    public float Duration = 0.5f;

    private Vector3 _baseScale;
    private Tween _scaleTween;

    private void OnEnable()
    {
        _baseScale = transform.localScale;

        if (AnimateAllways)
        {
            _scaleTween?.Kill();
            _scaleTween = transform
                .DOScale(_baseScale * ScaleUpFactor, Duration)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void OnDisable()
    {
        _scaleTween?.Kill();
        transform.localScale = _baseScale;
    }
}
