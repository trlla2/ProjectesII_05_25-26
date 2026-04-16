using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LigthningPanel : MonoBehaviour
{
    Image image;

    [SerializeField] float animationDuration = 0.8f;

    void Start()
    {
        image = GetComponent<Image>();
    }

    public void PlayAnimation()
    {
        float flashDuration = animationDuration / 4f;

        image.color = Color.white;
        image.DOFade(1f, 0f);
        Sequence sequence = DOTween.Sequence();

        sequence.Append(image.DOFade(0f, flashDuration));
        sequence.AppendCallback(() => image.color = new Color(1f, 1f, 0f, 0f));
        sequence.Append(image.DOFade(0.6f, flashDuration));
        sequence.Append(image.DOFade(0f, flashDuration));
        sequence.AppendCallback(() => image.color = new Color(1f, 1f, 1f, 0f));
        sequence.Append(image.DOFade(0.6f, flashDuration));
        sequence.Append(image.DOFade(0f, 0f));
    }
}
