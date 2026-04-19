using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UIExplosionToTarget : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject prefab;
    public int amount = 10;

    [Header("Transforms")]
    public RectTransform startPoint;
    public RectTransform uiParent;

    [Header("Explosion")]
    public float explosionRadius = 150f;
    public float explosionDuration = 0.5f;

    [Header("target")]
    public RectTransform targetWaypoint;
    public float moveToTargetDuration = 1f;

    [Header("Events")]
    public UnityEvent onReachedTarget;

    public bool alreadyHit = false;

    public void TriggerEffect()
    {
        Vector3[] corners = new Vector3[4];
        startPoint.GetWorldCorners(corners);

        Vector3 exactStartPos = (corners[0] + corners[2]) / 2f;

        for (int i = 0; i < amount; i++)
        {
            GameObject obj = Instantiate(prefab, uiParent);

            obj.transform.position = exactStartPos;

            Vector2 randomCircle = Random.insideUnitCircle * explosionRadius;
            Vector3 explosionEndPos = exactStartPos + new Vector3(randomCircle.x, randomCircle.y, 0f);

            StartCoroutine(AnimateItem(obj.GetComponent<RectTransform>(), explosionEndPos));
        }
    }

    private IEnumerator AnimateItem(RectTransform item, Vector3 explosionEndPos)
    {
        float elapsedTime = 0f;
        Vector3 startPos = item.position;

        while (elapsedTime < explosionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / explosionDuration;

            item.position = Vector3.Lerp(startPos, explosionEndPos, t);
            yield return null;
        }

        item.position = explosionEndPos;

        elapsedTime = 0f;
        Vector3 currentPos = item.position;

        while (elapsedTime < moveToTargetDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / moveToTargetDuration;

            item.position = Vector3.Lerp(currentPos, targetWaypoint.position, t);
            yield return null;
        }

        item.position = targetWaypoint.position;
        if(!alreadyHit)
        {
            onReachedTarget?.Invoke();
            alreadyHit = true;
        }

        Destroy(item.gameObject);
    }
}