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
    public UnityEvent OnStep;

    public bool isMoving { get; private set; } = false;
    private bool _canMove = true;


    void Start()
    {
        InitPlayer();
    }

    public void InitPlayer()
    {
        currentTile = _boardManager.startingTile;
        if (!_boardManager.IsTutorial)
            transform.position = _boardManager.startingTile.transform.position;
        else
            _canMove = false;
        _jumpParticlesDuration = jumpParticles.main.duration;
    }

    public void TryMoveTo(BoardTile target, System.Action onArrive = null)
    {
        if (!_canMove) return;
        if (currentTile == null || currentTile.GetGridPosition() != GetPlayerGridPosition()) ResetCurrentTile();

        var path = Pathfinding.FindPath(currentTile, target);
        if (path == null)
        {
            Debug.Log("algo bloquea el camino");
            return;
        }

        BoardTile interactTarget = null;
        if (path.Count > 0 && !path[path.Count - 1].IsWalkable())
        {
            interactTarget = path[path.Count - 1];
            path.RemoveAt(path.Count - 1);
        }

        BoardTile captured = interactTarget;
        StartCoroutine(MoveAlongPath(path, () => onArrive?.Invoke(), captured));
    }

    public void MoveForward(System.Action/*<BoardTile>*/ onStop = null)
    {
        if (isMoving || !_canMove) return;
        if (currentTile == null || currentTile.GetGridPosition() != GetPlayerGridPosition()) ResetCurrentTile();

        StartCoroutine(MoveForwardCoroutine(onStop));
    }

    private IEnumerator MoveForwardCoroutine(System.Action/*<BoardTile>*/ onStop)
    {
        isMoving = true;
        bool firstStep = true;

        BoardTile nonWalkableAhead = null;
        while (true)
        {
            BoardTile nextWalkable = GetNextForwardTile(walkableOnly: true);

            if (nextWalkable == null)
            {
                BoardTile nonWalkable = GetNextForwardTile(walkableOnly: false);
                nonWalkable?.Interact();
                break;
            }

            if (!firstStep)
                yield return new WaitForSeconds(waitTimeBetweenTilesInSeconds);

            firstStep = false;

            yield return StartCoroutine(JumpToTile(nextWalkable.transform.position));
            yield return new WaitUntil(() => transform.position == nextWalkable.transform.position);
            currentTile = nextWalkable;
            OnStep.Invoke();
            SpawnStepParticles(currentTile);

            bool interactionDone = false;
            currentTile.Interact(() => interactionDone = true);
            yield return new WaitUntil(() => interactionDone);
            //yield return new WaitForSeconds(0.1f); // Small delay to ensure interactions are fully processed

            //ResetCurrentTile();
            if (currentTile == null || currentTile.GetGridPosition() != GetPlayerGridPosition()) ResetCurrentTile();

            Debug.LogWarning($"[BRANCH] MoveForward step on {currentTile.GetGridPosition()} isBranchOrigin={currentTile.IsBranchOrigin()}");
            if (currentTile.IsBranchOrigin())
            {
                yield return new WaitForSeconds(0.2f);
                currentTile.Interact();
                break;
            }

            nonWalkableAhead = GetNextForwardTileFrom(currentTile, walkableOnly: false);
            if (nonWalkableAhead != null)
            {
                yield return new WaitForSeconds(0.2f);
                nonWalkableAhead.Interact();
                break;
            }
            else
                nonWalkableAhead = null;
        }

        isMoving = false;
        onStop?.Invoke(/*nonWalkableAhead*/);
    }

    private void SpawnStepParticles(BoardTile tile)
    {
        GameObject newParticles = Instantiate(
            jumpParticles.gameObject,
            tile.transform.position,
            tile.transform.rotation,
            tile.transform);
        Destroy(newParticles, _jumpParticlesDuration);
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

    IEnumerator MoveAlongPath(List<BoardTile> path, System.Action onComplete, BoardTile interactTarget = null)
    {
        isMoving = true;

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == path[0] && path[0] == currentTile) continue;
            BoardTile tile = path[i];

            if (!tile.IsWalkable())
                break;

            yield return StartCoroutine(JumpToTile(tile.transform.position));

            currentTile = tile;
            OnStep.Invoke();

            // Stop at branch origins so the path choice panel can take over
            if (currentTile.IsBranchOrigin())
            {
                SpawnStepParticles(tile);
                yield return new WaitForSeconds(0.2f);
                currentTile.Interact();
                isMoving = false;
                onComplete?.Invoke();
                yield break;
            }

            bool isLastTile = i == path.Count - 1;
            if (!isLastTile || interactTarget == null)
                currentTile.Interact();

            SpawnStepParticles(tile);

            if (i < path.Count - 1)
                yield return new WaitForSeconds(waitTimeBetweenTilesInSeconds);
        }

        interactTarget?.Interact();

        isMoving = false;
        onComplete?.Invoke();
    }

    IEnumerator JumpToTile(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, targetPos);
        float duration = moveSpeed > 0f ? distance / moveSpeed : jumpDuration;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

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
        currentTile = _boardManager.generatedTiles
            .Where(tile => tile.GetGridPosition() == GetPlayerGridPosition())
            .FirstOrDefault();
        //currentTile = _boardManager.generatedTiles[_boardManager.generatedTiles.Count - 1];
    }

    public BoardTile GetCurrentTile() => currentTile;

    private Vector2Int GetPlayerGridPosition()
    {
        return new Vector2Int(
            Mathf.RoundToInt(transform.position.x),
            Mathf.RoundToInt(transform.position.z));
    }

    // FORCED!!!
    private void OnCollisionEnter2D(Collision2D collision)
    {
        transform.position = _boardManager.startingTile.transform.position;
        GameObject newParticles = Instantiate(jumpParticles.gameObject, _boardManager.startingTile.transform.position, currentTile.transform.rotation, currentTile.transform);
        Destroy(newParticles, _jumpParticlesDuration);
        this.gameObject.GetComponent<Rigidbody2D>().simulated = false;
        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        collision.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        _canMove = true;
        OnStartingTileTouchedWhileFalling?.Invoke();
        //
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(true);
    }
}
