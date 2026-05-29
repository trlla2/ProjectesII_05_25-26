using DG.Tweening;
using UnityEngine;

public class DecorationTile : MonoBehaviour
{
    [Header("Float Animation")]
    [SerializeField] private float _floatAmplitude = 0.2f;
    [SerializeField] private float _floatDuration = 1.8f;

    private Tween _floatTween;

    private void Start()
    {
        PlayFloatAnimation();

        int randomRotation = Random.Range(0, 4) * 90; // 0, 90, 180, or 270 degrees
        transform.localRotation = Quaternion.Euler(0f, randomRotation, 0f);
    }

    private void PlayFloatAnimation()
    {
        // que floten desincronizadas
        float phaseOffset = Random.Range(0f, _floatDuration);

        _floatTween = transform
            .DOLocalMoveY(transform.localPosition.y + _floatAmplitude, _floatDuration)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetDelay(phaseOffset);
    }

    private void OnDestroy()
    {
        _floatTween?.Kill();
    }
}
