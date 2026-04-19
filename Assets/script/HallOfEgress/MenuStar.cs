using UnityEngine;
using DG.Tweening;

public class MenuStar : MonoBehaviour
{
    [SerializeField] private Transform[] LeftWaypoints;
    [SerializeField] private Transform[] RightWaypoints;

    [SerializeField] private float MoveDurationMin = 0.5f, MoveDurationMax = 3.0f;
    [SerializeField] private float DelayBetweenMoves = 7.0f;
    [SerializeField] private float DelayOffsetSec = 5.0f;

    [SerializeField] private Ease[] Eases;

    private bool _shootingFromLeft = true;
    private Vector3 _nextStartPosition;

    void Start()
    {
        _nextStartPosition = LeftWaypoints[Random.Range(0, LeftWaypoints.Length)].position;
        ScheduleNextShoot();
    }

    private void ScheduleNextShoot()
    {
        float delay = DelayBetweenMoves + Random.Range(-DelayOffsetSec, DelayOffsetSec);
        DOVirtual.DelayedCall(delay, Shoot);
    }

    private void Shoot()
    {
        Transform[] endPool = _shootingFromLeft ? RightWaypoints : LeftWaypoints;

        Transform end      = endPool[Random.Range(0, endPool.Length)];
        Ease ease          = Eases[Random.Range(0, Eases.Length)];
        float duration     = Random.Range(MoveDurationMin, MoveDurationMax);

        _shootingFromLeft = !_shootingFromLeft;

        transform.position = _nextStartPosition;
        _nextStartPosition = end.position;

        transform.DOMove(end.position, duration)
            .SetEase(ease)
            .OnComplete(ScheduleNextShoot);
    }
}
