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

    //public void MoveOneStepForward(System.Action onArrive = null)
    //{
    //    if (isMoving) return;
    //    if (currentTile == null) ResetCurrentTile();

    //    BoardTile walkableNext = GetNextForwardTile(walkableOnly: true);
    //    if (walkableNext != null)
    //    {
    //        StartCoroutine(MoveAlongPath(new List<BoardTile> { walkableNext }, () =>
    //        {
    //            onArrive?.Invoke();
    //        }));
    //        return;
    //    }

    //    BoardTile nonWalkableNext = GetNextForwardTile(walkableOnly: false);
    //    if (nonWalkableNext != null)
    //    {
    //        nonWalkableNext.Interact();
    //        return;
    //    }

    //    Debug.Log("No hay forward tile disponible");
    //}

    public void MoveUntilStop(System.Action<BoardTile> onStop = null)
    {
        if (isMoving) return;
        if (currentTile == null) ResetCurrentTile();

        List<BoardTile> path = BuildAutoAdvancePath();

        if (path.Count == 0)
        {
            onStop?.Invoke(currentTile);
            return;
        }

        StartCoroutine(MoveAlongPath(path, () =>
        {
            onStop?.Invoke(currentTile);
        }));
    }

    public void MoveUntilStopFrom(BoardTile firstTile, System.Action<BoardTile> onStop = null)
    {
        if (isMoving) return;
        if (currentTile == null) ResetCurrentTile();

        List<BoardTile> path = BuildAutoAdvancePathFrom(firstTile);

        if (path.Count == 0)
        {
            // moverse a firstTile igualmente y detenerse allí
            if (firstTile != null && firstTile.IsWalkable())
            {
                StartCoroutine(MoveAlongPath(new List<BoardTile> { firstTile }, () =>
                {
                    onStop?.Invoke(currentTile);
                }));
            }
            else
            {
                onStop?.Invoke(currentTile);
            }
            return;
        }

        StartCoroutine(MoveAlongPath(path, () =>
        {
            onStop?.Invoke(currentTile);
        }));
    }

    private List<BoardTile> BuildAutoAdvancePath()
    {
        return BuildAutoAdvancePathFrom(currentTile);
    }

    private List<BoardTile> BuildAutoAdvancePathFrom(BoardTile startFrom)
    {
        List<BoardTile> path = new List<BoardTile>();
        BoardTile current = currentTile;

        if (startFrom != null && startFrom != currentTile)
        {
            path.Add(startFrom);
            current = startFrom;
        }

        while (true)
        {
            BoardTile next = GetNextForwardTileFrom(current, walkableOnly: true);

            if (next == null)
                break;

            if (next.IsBranchOrigin())
            {
                path.Add(next);
                break;
            }

            if (!next.IsWalkable())
                break;

            path.Add(next);
            current = next;
        }

        return path;
    }

    private BoardTile GetNextForwardTile(bool walkableOnly)
    {
        return GetNextForwardTileFrom(currentTile, walkableOnly);
    }

    public BoardTile GetNextForwardTileFrom(BoardTile from, bool walkableOnly)
    {
        if (from == null) return null;

        Vector2Int currentPos = from.GetGridPosition();

        IEnumerable<BoardTile> candidates = from.neighbors
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

        for (int i = 0; i < path.Count; i++)
        {
            BoardTile tile = path[i];

            if (!tile.IsWalkable())
                break;

            yield return StartCoroutine(JumpToTile(tile.transform.position));

            currentTile = tile;
            currentTile.Interact();

            GameObject newParticles = Instantiate(
                jumpParticles.gameObject,
                tile.transform.position,
                tile.transform.rotation,
                tile.transform);
            Destroy(newParticles, _jumpParticlesDuration);

            if (i < path.Count - 1)
                yield return new WaitForSeconds(waitTimeBetweenTilesInSeconds);
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
