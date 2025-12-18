using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpHeight;
    [SerializeField] private float jumpDuration;
    [SerializeField] private ParticleSystem jumpParticles;
    private float _jumpParticlesDuration;
    [SerializeField] private float waitTimeBetweenTilesInSeconds;

    private BoardTile currentTile;
    public bool isMoving { get; private set; } = false;

    void Start()
    {
        currentTile = BoardManager.instance.startingTile;
        transform.position = BoardManager.instance.startingTile.transform.position;
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
        //foreach (var tile in BoardManager.instance.generatedTiles)
        //    if (Vector3.Distance(transform.position, tile.transform.position) < 0.5)
        //        currentTile = tile;
        currentTile = BoardManager.instance.generatedTiles[BoardManager.instance.generatedTiles.Count - 1];
    }

}
