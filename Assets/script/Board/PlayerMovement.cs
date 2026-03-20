using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class PlayerMovement : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpDuration;
    [SerializeField] private float waitTimeBetweenTilesInSeconds;

    [Header("Particles")]
    [SerializeField] private ParticleSystem jumpParticles;
    private float _jumpParticlesDuration;

    [Header("Board")]
    [SerializeField] private BoardManager _boardManager;
    private BoardTile currentTile;

    public UnityEvent OnStartingTileTouchedWhileFalling;

    public bool isMoving { get; private set; } = false;

    void Start()
    {
        InitPlayer();
    }

    public void InitPlayer()
    {
        currentTile = _boardManager.startingTile;
        if (!_boardManager.IsTutorial)
            transform.position = _boardManager.startingTile.transform.position;
        _jumpParticlesDuration = jumpParticles.main.duration;
    }

    public void TryMoveTo(BoardTile target, System.Action onArrive = null)
    {
        if (currentTile == null) ResetCurrentTile();

        var path = Pathfinding.FindPath(currentTile, target);
        if (path == null)
        {
            Debug.Log("algo bloquea el camino");
            return;
        }

        StartCoroutine(MoveAlongPath(path, () =>
        {
            onArrive?.Invoke();
        }));
    }

    public void MoveOneStepForward(System.Action onArrive = null)
    {
        if (isMoving) return;
        if (currentTile == null) ResetCurrentTile();

        BoardTile walkableNext = GetNextForwardTile(walkableOnly: true);
        if (walkableNext != null)
        {
            StartCoroutine(MoveAlongPath(new List<BoardTile> { walkableNext }, () =>
            {
                onArrive?.Invoke();
            }));
            return;
        }

        BoardTile nonWalkableNext = GetNextForwardTile(walkableOnly: false);
        if (nonWalkableNext != null)
        {
            nonWalkableNext.Interact();
            return;
        }

        Debug.Log("No hay forward tile disponible");
    }

    public void MoveOneStepTo(BoardTile target, System.Action onArrive = null)
    {
        if (isMoving) return;
        if (currentTile == null) ResetCurrentTile();

        StartCoroutine(MoveOneStepCoroutine(target, onArrive));
    }

    private IEnumerator MoveOneStepCoroutine(BoardTile target, System.Action onArrive)
    {
        isMoving = true;

        yield return StartCoroutine(JumpToTile(target.transform.position));
        currentTile = target;

        GameObject newParticles = Instantiate(jumpParticles.gameObject, target.transform.position, target.transform.rotation, target.transform);
        Destroy(newParticles, _jumpParticlesDuration);

        isMoving = false;
        onArrive?.Invoke();
    }

    private BoardTile GetNextForwardTile(bool walkableOnly)
    {
        if (currentTile == null) return null;

        Vector2Int currentPos = currentTile.GetGridPosition();

        IEnumerable<BoardTile> candidates = currentTile.neighbors
            .Where(n =>
                n != null &&
                IsForwardPosition(currentPos, n.GetGridPosition()));

        if (walkableOnly)
            candidates = candidates.Where(n => n.IsWalkable());
        else
            candidates = candidates.Where(n => !n.IsWalkable());

        return candidates
            .OrderBy(n => Vector2Int.Distance(currentPos, n.GetGridPosition()))
            .FirstOrDefault();
    }

    private bool IsForwardPosition(Vector2Int from, Vector2Int to)
    {
        int dx = to.x - from.x;
        int dy = to.y - from.y;

        return (dx > 0 && dy == 0) || (dx == 0 && dy > 0) || (dx > 0 && dy > 0);
    }

    IEnumerator MoveAlongPath(List<BoardTile> path, System.Action onComplete)
    {
        isMoving = true;

        foreach (var tile in path)
        {
            if (!tile.IsWalkable())
                break;

            Vector3 targetPos = tile.transform.position;
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                yield return StartCoroutine(JumpToTile(tile.transform.position));
            }
            currentTile = tile;

            GameObject newParticles = Instantiate(jumpParticles.gameObject, tile.transform.position, currentTile.transform.rotation, currentTile.transform);
            Destroy(newParticles, _jumpParticlesDuration);

            if (currentTile != path[0])
            {
                Debug.Log("waiting..");
                yield return new WaitForSecondsRealtime(waitTimeBetweenTilesInSeconds);
            }
        }

        isMoving = false;
        onComplete?.Invoke();
    }

    IEnumerator JumpToTile(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            Vector3 horizontalPos = Vector3.Lerp(startPos, targetPos, t);
            float height = Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = new Vector3(
                horizontalPos.x,
                horizontalPos.y + height,
                horizontalPos.z
            );

            yield return null;
        }

        transform.position = targetPos;
    }

    private void ResetCurrentTile()
    {
        currentTile = _boardManager.generatedTiles[_boardManager.generatedTiles.Count - 1];
    }

    public BoardTile GetCurrentTile() => currentTile;

    // FORCED!!!
    private void OnCollisionEnter2D(Collision2D collision)
    {
        transform.position = _boardManager.startingTile.transform.position;
        GameObject newParticles = Instantiate(jumpParticles.gameObject, _boardManager.startingTile.transform.position, currentTile.transform.rotation, currentTile.transform);
        Destroy(newParticles, _jumpParticlesDuration);
        this.gameObject.GetComponent<Rigidbody2D>().simulated = false;
        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        collision.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        OnStartingTileTouchedWhileFalling?.Invoke();
    }
}
