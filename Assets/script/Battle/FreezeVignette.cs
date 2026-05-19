using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FreezeVignette : MonoBehaviour
{
    Image sr;

    void Start()
    {
        sr = GetComponent<Image>();

        if (sr == null) return;
        sr.DOFade(0.0f, 0);
    }
    public void PerformFreezePaneleffect(float duration)
    {
        if (sr == null) return;

        sr.DOKill();
        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, 0f);

        Sequence seq = DOTween.Sequence();
        seq.Append(sr.DOFade(0.6f, duration / 2.0f).SetEase(Ease.InSine));
        seq.Append(sr.DOFade(0.0f, duration / 2.0f).SetEase(Ease.OutSine));
    }
}
