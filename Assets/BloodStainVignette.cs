using DG.Tweening;
using UnityEngine;

public class BloodStainVignette : MonoBehaviour
{
    [SerializeField] SpriteRenderer[] bloodStainSpriteRenderer;
    [SerializeField] float fadeInDuration = 0.5f;
    [SerializeField] float HalfFadeOutDuration = 0.5f;
    [SerializeField] float FinalFadeOutDuration = 1.5f;

    private Sequence currentSequence;

    private void Start()
    {
        GameManager.Instance.CombatHandler.OnPlayerHitRecived += PerformAnimationOnRandomImage;
    }


    void PerformAnimationOnRandomImage()
    {
        if (bloodStainSpriteRenderer == null || bloodStainSpriteRenderer.Length == 0)
            return;

        for (int i = 0; i < bloodStainSpriteRenderer.Length; i++)
        {
            if (bloodStainSpriteRenderer[i] == null) continue;

            bloodStainSpriteRenderer[i].DOKill();
            Color c = bloodStainSpriteRenderer[i].color;
            c.a = 0f;
            bloodStainSpriteRenderer[i].color = c;
        }

        SpriteRenderer randRenderer = bloodStainSpriteRenderer[Random.Range(0, bloodStainSpriteRenderer.Length)];

        currentSequence?.Kill();
        currentSequence = DOTween.Sequence();
        currentSequence.Append(randRenderer.DOFade(0.8f, fadeInDuration));
        currentSequence.Append(randRenderer.DOFade(0.25f, HalfFadeOutDuration));
        currentSequence.Append(randRenderer.DOFade(0f, FinalFadeOutDuration));
    }
}
