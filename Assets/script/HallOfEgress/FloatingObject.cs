using UnityEngine;
using DG.Tweening;

public class FloatingObject : MonoBehaviour
{
    [Header("Models")]
    [SerializeField] private GameObject[] _models;

    [Header("Float Settings")]
    [SerializeField] private float minFloatHeight = 0.2f;
    [SerializeField] private float maxFloatHeight = 0.5f;
    [SerializeField] private float minFloatDuration = 1.2f;
    [SerializeField] private float maxFloatDuration = 2.5f;

    [SerializeField] private bool startDelay = false;

    [Header("Rotation Settings")]
    [SerializeField] private float minRotationAmount = 10f;
    [SerializeField] private float maxRotationAmount = 25f;
    [SerializeField] private float minRotationDuration = 2f;
    [SerializeField] private float maxRotationDuration = 4f;

    private Tween floatTween;
    private Tween rotateTween;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnRandomModel();

        if (startDelay)
        {
            float delay = Random.Range(0f, 1f);
            DOVirtual.DelayedCall(delay, StartFloating);
        }
        else
        {
            StartFloating();
        }
    }

    void StartFloating()
    {
        float floatHeight    = Random.Range(minFloatHeight, maxFloatHeight);
        float floatDuration  = Random.Range(minFloatDuration, maxFloatDuration);
        float rotationAmount = Random.Range(minRotationAmount, maxRotationAmount);
        float rotateDuration = Random.Range(minRotationDuration, maxRotationDuration);

        float fullFloatCycle  = floatDuration * 2f;
        float fullRotateCycle = rotateDuration * 2f;

        Vector3 startPos = transform.position;
        transform.position = new Vector3(startPos.x, startPos.y - floatHeight, startPos.z);
        transform.rotation = Quaternion.Euler(rotationAmount, 0f, -rotationAmount);

        floatTween = transform.DOMoveY(startPos.y + floatHeight, fullFloatCycle).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        rotateTween = transform.DORotate(new Vector3(0f, 0f, rotationAmount), fullRotateCycle).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);

        //Jump each tween to a random point in its cycle to randomize
        floatTween.Goto(Random.Range(0f, fullFloatCycle), andPlay: true);
        rotateTween.Goto(Random.Range(0f, fullRotateCycle), andPlay: true);
    }

    private void SpawnRandomModel()
    {
        if (_models == null || _models.Length == 0) return;

        GameObject prefab = _models[Random.Range(0, _models.Length)];
        prefab.transform.localScale = Vector3.one * 0.8f;
        prefab.layer = gameObject.layer; //set layer to macth to apply outline
        Instantiate(prefab, transform.position, transform.rotation, transform);
    }

    private void OnDestroy()
    {
        floatTween?.Kill();
        rotateTween?.Kill();
    }
}
