using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

//Same as SpellPathAnimator but for the orb charge animation
public class OrbChargeAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float elementScale = 0.8f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float elementSpacing = 0.05f;

    [Header("References")]
    [SerializeField] private GameObject clonePrefab;

    private List<GameObject> activeClones = new List<GameObject>();

    private void OnDestroy()
    {
        CleanupAllAnimations();
    }

    public void AnimateChargeToOrb(List<Vector3> startPositions, List<Element> elements, OrbVisual targetOrb, System.Action<int> onEachElementArrival = null, System.Action onComplete = null)
    {
        if (startPositions == null || startPositions.Count == 0 || targetOrb == null)
        {
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimateChargeRoutine(startPositions, elements, targetOrb, onEachElementArrival, onComplete));
    }

    private IEnumerator AnimateChargeRoutine(List<Vector3> startPositions, List<Element> elements, OrbVisual targetOrb, System.Action<int> onEachElementArrival, System.Action onComplete)
    {
        List<GameObject> clones = new List<GameObject>();
        Vector3 targetPosition = targetOrb.ChargeElementTargetPos.position;

        for (int i = 0; i < startPositions.Count; i++)
        {
            Element element = (elements != null && i < elements.Count) ? elements[i] : null;
            GameObject clone = CreateClone(element, startPositions[i]);
            
            if (clone != null)
            {
                clones.Add(clone);
                activeClones.Add(clone);

                float delay = i * elementSpacing;
                int elementIndex = i;
                StartCoroutine(AnimateSingleClone(clone, startPositions[i], targetPosition, delay, () =>
                {
                    onEachElementArrival?.Invoke(elementIndex);
                }));
            }
        }

        if (clones.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        float totalDuration = animationDuration + (clones.Count * elementSpacing);
        yield return new WaitForSeconds(totalDuration + 0.1f);

        foreach (GameObject clone in clones)
        {
            if (clone != null)
            {
                Destroy(clone);
            }
        }

        activeClones.RemoveAll(obj => obj == null);
        onComplete?.Invoke();
    }

    private IEnumerator AnimateSingleClone(GameObject clone, Vector3 startPos, Vector3 targetPos, float delay, System.Action onArrival)
    {
        yield return new WaitForSeconds(delay);

        if (clone == null)
            yield break;

        Sequence animSequence = DOTween.Sequence();

        animSequence.Append(clone.transform.DOMove(targetPos, animationDuration).SetEase(movementCurve));

        animSequence.Join(clone.transform.DOScale(Vector3.zero, animationDuration * 0.5f).SetDelay(animationDuration * 0.5f).SetEase(Ease.InBack));

        animSequence.OnComplete(() =>
        {
            onArrival?.Invoke();
            
            if (clone != null)
            {
                SpriteRenderer sr = clone.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.DOFade(0f, 0.1f);
                }
            }
        });

        animSequence.Play();
    }

    private GameObject CreateClone(Element element, Vector3 position)
    {
        if (clonePrefab == null)
            return null;

        GameObject clone = Instantiate(clonePrefab, position, Quaternion.identity);

        SpriteRenderer renderer = clone.GetComponent<SpriteRenderer>();
        if (renderer != null && element != null)
        {
            renderer.sprite = element.Sprite;
            renderer.color = element.Color;
        }

        clone.transform.localScale = Vector3.one * elementScale;
        return clone;
    }

    public void CleanupAllAnimations()
    {
        StopAllCoroutines();

        foreach (GameObject clone in activeClones)
        {
            if (clone != null)
            {
                Destroy(clone);
            }
        }
        activeClones.Clear();
    }
}