using UnityEngine;
using DG.Tweening;

public class OnBoardingFingerAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _moveDistance = 80f;
    [SerializeField] private float _moveDuration = 0.6f;
    [SerializeField] private Ease _easeType = Ease.InOutSine;

    private Vector3 _initialLocalPosition;
    private Tween _currentTween;

    private void Start()
    {
        _initialLocalPosition = transform.localPosition;
        Time.timeScale = 1f;
        PlayAnimation();
    }

    private void OnDisable()
    {
        _currentTween?.Kill();
    }

    public void PlayAnimation()
    {
        _currentTween?.Kill();
        transform.localPosition = _initialLocalPosition;

        Vector3 targetPosition = _initialLocalPosition + Vector3.up * _moveDistance;

        _currentTween = transform
            .DOLocalMove(targetPosition, _moveDuration)
            .SetEase(_easeType)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopAnimation()
    {
        _currentTween?.Kill();
        transform.localPosition = _initialLocalPosition;
    }
}
