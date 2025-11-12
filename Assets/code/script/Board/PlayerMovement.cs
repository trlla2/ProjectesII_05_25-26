using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;

    private BoardTile currentTile;
    public bool isMoving { get; private set; } = false;

    void Start()
    {
        currentTile = BoardManager.instance.startingTile;
        transform.position = BoardManager.instance.startingTile.transform.position;
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
            Vector3 targetPos = tile.transform.position;
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
                yield return null;
            }
            currentTile = tile;
        }

        isMoving = false;
        onComplete?.Invoke();
    }

    private void ResetCurrentTile()
    {
        //foreach (var tile in BoardManager.instance.generatedTiles)
        //    if (Vector3.Distance(transform.position, tile.transform.position) < 0.5)
        //        currentTile = tile;
        currentTile = BoardManager.instance.generatedTiles[BoardManager.instance.generatedTiles.Count - 1];
    }

}
