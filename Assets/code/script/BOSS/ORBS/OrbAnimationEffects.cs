using UnityEngine;
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

    private Vector3 _startPosition;

    WaitForSeconds startdelay = new WaitForSeconds(0.25f);


    void Start()
    {
        if(!DelayHover)
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
        transform.DOLocalMoveY(_startPosition.y + hoverDistance, hoverDuration).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
    }

    public void Shake(float amount)
    {
        Shake(amount, defaultShakeDuration);
    }

    public void Shake(float amount, float duration)
    {
        transform.DOShakePosition(duration, amount, 10, 90, false, true).SetRelative(false);
    }

    public void SetScale(float amount, float duration)
    {
        transform.DOScale(amount, duration).SetEase(Ease.OutQuad);
    }
}
