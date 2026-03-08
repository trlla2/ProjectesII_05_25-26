using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;

public class OrbAnimationEffects : MonoBehaviour
{
    [Header("Hover Settings")]
    [SerializeField] private float hoverDistance = 0.3f;
    [SerializeField] private float hoverDuration = 1.5f;
    [SerializeField] private bool DelayHover = false;

    [Header("Shake Settings")]
    [SerializeField] private float defaultShakeDuration = 0.5f;

    [Header("Blink Settings")]
    [SerializeField] private Color blinkColor = Color.white;
    [SerializeField] private float blinkHalfPeriod = 0.15f;

    private Vector3 _startPosition;

    private Image image;
    private Color _baseColor;

    private Tween _blinkTween;
    private Tween _postBlinkColorTween;
    private Tween _postBlinkScaleTween;

    private readonly WaitForSeconds startdelay = new WaitForSeconds(0.25f);

    void Start()
    {
        image = GetComponent<Image>();
        if (image != null) _baseColor = image.color;

        if (!DelayHover)
        {
            _startPosition = transform.localPosition;
            HoverAnim();
        }
        else
        {
            _startPosition = transform.localPosition;
            StartCoroutine(DelayedHover());
        }
    }

    private IEnumerator DelayedHover()
    {
        yield return startdelay;
        HoverAnim();
    }

    private void HoverAnim()
    {
        transform.DOLocalMoveY(_startPosition.y + hoverDistance, hoverDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void Shake(float amount)
    {
        Shake(amount, defaultShakeDuration);
    }

    public void ColorBlink()
    {
        if (image == null) return;

        DisableBlink();

        _blinkTween = image.DOColor(blinkColor, blinkHalfPeriod).SetEase(Ease.InOutSine).SetLoops(5, LoopType.Yoyo)
            .OnComplete(() =>
            {
                _postBlinkColorTween = image.DOColor(blinkColor * Color.red, blinkHalfPeriod / 2f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);

                _postBlinkScaleTween = transform.DOScale(Vector3.one * 1.5f, blinkHalfPeriod / 2f)
                    .SetEase(Ease.InOutSine)
                    .SetLoops(-1, LoopType.Yoyo);
            });
    }

    public void DisableBlink()
    {
        _blinkTween?.Kill();
        _blinkTween = null;

        _postBlinkColorTween?.Kill();
        _postBlinkColorTween = null;

        _postBlinkScaleTween?.Kill();
        _postBlinkScaleTween = null;
    }


    public void ResetVisual()
    {
        if (image == null) return;

        _blinkTween?.Kill();
        _blinkTween = null;

        image.color = _baseColor;
    }


    public void Shake(float amount, float duration)
    {
        transform.DOShakePosition(duration, amount, 10, 90, false, true).SetRelative(false);
    }

    public void SetScale(float amount, float duration)
    {
        transform.DOScale(amount, duration).SetEase(Ease.OutQuad);
    }

    public void KillAllTweens()
    {
        transform.DOKill();
        if (image != null) image.DOKill();
    }
}