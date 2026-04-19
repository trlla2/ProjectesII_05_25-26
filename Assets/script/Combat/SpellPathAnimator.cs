using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SpellPathAnimator : MonoBehaviour
{
    [Header("Spell Animation Settings")]
    [SerializeField] private float elementScale = 0.8f;
    [SerializeField] private float elementSpacing = 0.1f;

    [Header("References")]
    [SerializeField] private GameObject clonePrefab;
    [SerializeField] private SpellPathDate spellPathData;
    [SerializeField] private Grid grid;

    [Header("Animator Type")]
    [SerializeField] private bool isPlayerAnimator = true;

    [Header("Hit Particle Systems")]
    [SerializeField] private ParticleSystem redHitParticle;
    [SerializeField] private ParticleSystem blueHitParticle;
    [SerializeField] private ParticleSystem greenHitParticle;
    [SerializeField] private ParticleSystem yellowHitParticle;

    [Header("Element Trails")]
    [SerializeField] private GameObject redTrail;
    [SerializeField] private GameObject blueTrail;
    [SerializeField] private GameObject greenTrail;
    [SerializeField] private GameObject yellowTrail;

    private float damagePerElement = 4f;

    private CameraShake cameraShake;
    private List<GameObject> activeClones = new List<GameObject>();
    private Dictionary<GameObject, Element> cloneElementMap = new Dictionary<GameObject, Element>();
    private Dictionary<GameObject, GameObject> cloneTrailMap = new Dictionary<GameObject, GameObject>();
    public System.Action OnAttack;

    private void Start()
    {
        cameraShake = Camera.main != null ? Camera.main.GetComponent<CameraShake>() : null;

        if (grid == null)
        {
            grid = FindFirstObjectByType<Grid>();
        }
    }

    private void OnDestroy()
    {
        CleanupAllAnimations();
    }

    public void Initialize(Grid gridController)
    {
        grid = gridController;
    }

    public void SetIsPlayerAnimator(bool isPlayer)
    {
        isPlayerAnimator = isPlayer;
    }

    public void SetDamagePerElement(float damage)
    {
        damagePerElement = damage;
    }

    public void OnSpellCast(Vector2Int[] pathPositions, List<Element> elements, System.Action onComplete = null)
    {
        if (!isPlayerAnimator)
        {
            onComplete += (() =>
            {
                GameManager.Instance?.CombatHandler.GetEnemyRef().animator.SetTrigger("Attack");
            });
        }
        if (pathPositions == null || pathPositions.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (spellPathData == null || spellPathData.pathSections.Length == 0)
        {
            onComplete?.Invoke();
            return;
        }

        if (grid == null)
        {
            Debug.LogError("[SpellPathAnimator] Cannot cast spell: Grid reference is null");
            onComplete?.Invoke();
            return;
        }

        StartCoroutine(AnimateSpellPath(pathPositions, elements, onComplete));
    }

    public void CleanupAllAnimations()
    {
        StopAllCoroutines();

        foreach (GameObject clone in activeClones)
        {
            if (clone != null)
            {
                DetachTrailFromClone(clone);
                Destroy(clone);
            }
        }
        activeClones.Clear();
        cloneElementMap.Clear();
        cloneTrailMap.Clear();
    }

    private IEnumerator AnimateSpellPath(Vector2Int[] pathPositions, List<Element> elements, System.Action onComplete)
    {
        List<GameObject> clones = CreateClones(pathPositions, elements);

        if (clones.Count == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        for (int i = 0; i < clones.Count; i++)
        {
            StartCoroutine(AnimateSingleClone(clones[i], i * elementSpacing));
        }

        float totalDuration = CalculateTotalAnimationDuration();
        yield return new WaitForSeconds(totalDuration + (clones.Count * elementSpacing));

        foreach (GameObject clone in clones)
        {
            if (clone != null)
            {
                Destroy(clone);
            }
        }

        activeClones.RemoveAll(obj => obj == null);
        cloneElementMap.Clear();
        cloneTrailMap.Clear();
        OnAttack?.Invoke();
        onComplete?.Invoke();
    }

    private IEnumerator AnimateSingleClone(GameObject clone, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        AttachTrailToClone(clone);

        Sequence animationSequence = DOTween.Sequence();

        foreach (PathSection section in spellPathData.pathSections)
        {
            // Resolve random loop params once per clone per cast
            if (section.isRandPos && section.mode == SpellPathMode.Loop)
                section.ResolveRandomLoop();

            List<Transform> waypoints = section.GetProcessedWaypoints();

            if (waypoints.Count == 0)
                continue;

            switch (section.mode)
            {
                case SpellPathMode.CurvedLinear:
                    AnimateCurvedPath(animationSequence, clone, waypoints, section);
                    break;

                case SpellPathMode.Loop:
                    AnimateLoopPath(animationSequence, clone, waypoints, section);
                    break;
            }
        }

        animationSequence.OnComplete(() => OnCloneAnimationComplete(clone, startDelay));
        animationSequence.Play();
    }

    private void AttachTrailToClone(GameObject clone)
    {
        if (!cloneElementMap.TryGetValue(clone, out Element element)) return;

        GameObject trailPrefab = GetTrailForElement(element.Type);
        if (trailPrefab == null) return;

        GameObject trailInstance = Instantiate(trailPrefab, clone.transform);
        trailInstance.transform.localPosition = Vector3.zero;
        trailInstance.SetActive(true);

        cloneTrailMap[clone] = trailInstance;
    }

    private void DetachTrailFromClone(GameObject clone)
    {
        if (!cloneTrailMap.TryGetValue(clone, out GameObject trailInstance)) return;
        if (trailInstance == null)
        {
            cloneTrailMap.Remove(clone);
            return;
        }

        // Unparent so the trail lingers briefly before being destroyed,
        trailInstance.transform.SetParent(null, true);

        TrailRenderer tr = trailInstance.GetComponentInChildren<TrailRenderer>();
        if (tr != null)
        {
            tr.emitting = false;
            Destroy(trailInstance, tr.time);
        }
        else
        {
            Destroy(trailInstance);
        }

        cloneTrailMap.Remove(clone);
    }

    private GameObject GetTrailForElement(ElementType elementType)
    {
        return elementType switch
        {
            ElementType.Red    => redTrail,
            ElementType.Blue   => blueTrail,
            ElementType.Green  => greenTrail,
            ElementType.Yellow => yellowTrail,
            _                  => null
        };
    }

    private void AnimateCurvedPath(Sequence sequence, GameObject clone, List<Transform> waypoints, PathSection section)
    {
        Vector3[] pathPositions = new Vector3[waypoints.Count];
        for (int i = 0; i < waypoints.Count; i++)
        {
            pathPositions[i] = waypoints[i].position;
        }

        sequence.Append(
            clone.transform.DOPath(
                pathPositions,
                section.duration,
                PathType.CatmullRom
            )
            .SetEase(section.animCurve)
        );
    }

    private void AnimateLoopPath(Sequence sequence, GameObject clone, List<Transform> waypoints, PathSection section)
    {
        if (waypoints.Count >= 2)
        {
            Vector3 startPoint = waypoints[0].position;
            Vector3 endPoint   = waypoints[1].position;
            Vector3 center     = (startPoint + endPoint) / 2f;

            float radius = section.isRandPos
                ? section.finalRadius
                : Vector3.Distance(startPoint, endPoint) / 2f;

            Vector3 toEnd      = endPoint - center;
            float startAngle   = Mathf.Atan2(toEnd.y, toEnd.x);
            bool clockwise     = section.isRandPos && section.finalClockwise;

            Vector3[] circlePoints = CreateCircularPathBetweenPoints(center, radius, startAngle, 8, clockwise);

            sequence.Append(
                clone.transform.DOPath(circlePoints, section.duration, PathType.CatmullRom)
                    .SetEase(section.animCurve)
            );
        }
        else if (waypoints.Count == 1)
        {
            Vector3 center = waypoints[0].position;

            float radius   = section.isRandPos ? section.finalRadius : 1.2f;
            bool clockwise = section.isRandPos && section.finalClockwise;

            Vector3[] circlePoints = CreateCircularPath(center, radius, 8, clockwise);

            sequence.Append(
                clone.transform.DOPath(circlePoints, section.duration, PathType.CatmullRom)
                    .SetEase(section.animCurve)
            );
        }
    }

    private void OnCloneAnimationComplete(GameObject clone, float startDelay)
    {
        DetachTrailFromClone(clone);

        if (GameManager.Instance != null && cloneElementMap.ContainsKey(clone))
        {
            ElementType element = cloneElementMap[clone].Type;

            element = element switch
            {
                ElementType.Orange => ElementType.Red,
                ElementType.Melon  => ElementType.Green,
                _                  => element
            };

            if (isPlayerAnimator)
            {
                PlayHitParticle(element, clone.transform.position);
                GameManager.Instance.CombatHandler.AttackEnemy(element);
            }
            else
            {
                GameManager.Instance.CombatHandler.AttackPlayer(damagePerElement);
            }
        }

        if (cameraShake != null)
        {
            cameraShake.Shake(0.25f, startDelay - 0.05f);
        }

        SpriteRenderer sr = clone.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.DOFade(0f, 0.3f).OnComplete(() =>
            {
                if (clone != null)
                {
                    cloneElementMap.Remove(clone);
                    Destroy(clone);
                }
            });
        }
        else
        {
            if (clone != null)
            {
                cloneElementMap.Remove(clone);
                Destroy(clone);
            }
        }
    }

    private void PlayHitParticle(ElementType elementType, Vector3 position)
    {
        ParticleSystem ps = elementType switch
        {
            ElementType.Red    => redHitParticle,
            ElementType.Blue   => blueHitParticle,
            ElementType.Green  => greenHitParticle,
            ElementType.Yellow => yellowHitParticle,
            ElementType.Orange => redHitParticle,
            ElementType.Melon  => greenHitParticle,
            _                  => null
        };

        if (ps == null) return;

        ps.transform.position = position;
        ps.Play();
    }

    private List<GameObject> CreateClones(Vector2Int[] pathPositions, List<Element> providedElements = null)
    {
        List<GameObject> clones = new List<GameObject>();

        if (pathPositions == null || pathPositions.Length == 0)
            return clones;

        if (grid == null)
        {
            Debug.LogError("CreateClones called but Grid reference is null. No clones will be created.");
            return clones;
        }

        for (int i = 0; i < pathPositions.Length; i++)
        {
            Vector2Int pos = pathPositions[i];

            Element element = null;
            if (providedElements != null && i < providedElements.Count)
            {
                element = providedElements[i];
            }

            if (element == null)
            {
                element = grid.GetElementAt(pos);
            }

            if (element != null)
            {
                GameObject clone = CreateClone(element);
                if (clone != null)
                {
                    Vector3 worldPos = GetWorldPosition(pos);
                    clone.transform.position = worldPos;
                    clones.Add(clone);
                    activeClones.Add(clone);
                    cloneElementMap[clone] = element;
                }
            }
        }

        return clones;
    }

    private GameObject CreateClone(Element element)
    {
        if (clonePrefab == null)
            return null;

        GameObject clone = Instantiate(clonePrefab);

        SpriteRenderer renderer = clone.GetComponent<SpriteRenderer>();
        if (renderer != null && element.Sprite != null)
        {
            renderer.sprite = element.Sprite;
            renderer.color = element.Color;
        }

        clone.transform.localScale = Vector3.one * elementScale;
        return clone;
    }

    private Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        if (grid == null)
            return Vector3.zero;

        ElementVisual visual = grid.GetElementVisualAt(gridPosition);
        if (visual != null)
        {
            return visual.GetWorldPosition();
        }
        return Vector3.zero;
    }

    private Vector3[] CreateCircularPath(Vector3 center, float radius, int segments, bool clockwise = false)
    {
        Vector3[] points  = new Vector3[segments + 1];
        float direction   = clockwise ? -1f : 1f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = direction * i * (360f / segments) * Mathf.Deg2Rad;
            float x     = center.x + Mathf.Cos(angle) * radius;
            float y     = center.y + Mathf.Sin(angle) * radius;
            points[i]   = new Vector3(x, y, center.z);
        }

        return points;
    }

    private Vector3[] CreateCircularPathBetweenPoints(Vector3 center, float radius, float startAngle, int segments, bool clockwise = false)
    {
        Vector3[] points  = new Vector3[segments + 1];
        float direction   = clockwise ? -1f : 1f;

        for (int i = 0; i <= segments; i++)
        {
            float angle = startAngle + direction * (i * (360f / segments) * Mathf.Deg2Rad);
            float x     = center.x + Mathf.Cos(angle) * radius;
            float y     = center.y + Mathf.Sin(angle) * radius;
            points[i]   = new Vector3(x, y, center.z);
        }

        return points;
    }

    private float CalculateTotalAnimationDuration()
    {
        float total = 0f;
        foreach (PathSection section in spellPathData.pathSections)
        {
            total += section.duration;
        }
        return total;
    }

}