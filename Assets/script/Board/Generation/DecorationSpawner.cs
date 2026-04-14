using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecorationSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private BoardGenerator _boardGenerator;

    [Header("Decoration Prefabs")]
    [SerializeField] private List<GameObject> _decorationPrefabs;

    [Header("Spawn Settings")]
    [SerializeField] private int _decorationCount = 12;
    [SerializeField] private float _spawnMargin = 1.5f;
    [SerializeField] private float _spawnPadding = 2.5f;

    [Header("Height Randomization")]
    [SerializeField] private float _minHeight = 0.5f;
    [SerializeField] private float _maxHeight = 3.0f;

    private void Start()
    {
        StartCoroutine(SpawnAfterBoard());
    }

    private IEnumerator SpawnAfterBoard()
    {
        yield return null;
        SpawnDecorations();
    }

    private void SpawnDecorations()
    {
        if (_decorationPrefabs == null || _decorationPrefabs.Count == 0)
        {
            Debug.LogWarning("DecorationSpawner: no hay prefabs de decoración");
            return;
        }

        if (_boardManager == null || _boardManager.generatedTiles == null
            || _boardManager.generatedTiles.Count == 0)
        {
            Debug.LogWarning("DecorationSpawner: BoardManager no tiene tiles generadas.");
            return;
        }

        Bounds boardBounds = CalculateBoardBounds();

        for (int i = 0; i < _decorationCount; i++)
        {
            Vector3 spawnPos = GetRandomPositionAroundBoard(boardBounds);
            GameObject prefab = _decorationPrefabs[Random.Range(0, _decorationPrefabs.Count)];
            Instantiate(prefab, spawnPos, Quaternion.identity, transform);
        }
    }

    private Bounds CalculateBoardBounds()
    {
        Bounds bounds = new Bounds(_boardManager.generatedTiles[0].transform.position, Vector3.zero);
        foreach (BoardTile tile in _boardManager.generatedTiles)
            bounds.Encapsulate(tile.transform.position);

        return bounds;
    }

    private Vector3 GetRandomPositionAroundBoard(Bounds boardBounds)
    {
        float y = Random.Range(_minHeight, _maxHeight);

        // área exterior donde spawnean
        float outerMinX = boardBounds.min.x - _spawnPadding;
        float outerMaxX = boardBounds.max.x + _spawnPadding;
        float outerMinZ = boardBounds.min.z - _spawnPadding;
        float outerMaxZ = boardBounds.max.z + _spawnPadding;

        // Área interior (board + margen) // evitar spawn here
        float innerMinX = boardBounds.min.x - _spawnMargin;
        float innerMaxX = boardBounds.max.x + _spawnMargin;
        float innerMinZ = boardBounds.min.z - _spawnMargin;
        float innerMaxZ = boardBounds.max.z + _spawnMargin;

        Vector3 candidate;
        int maxAttempts = 30;

        do
        {
            float x = Random.Range(outerMinX, outerMaxX);
            float z = Random.Range(outerMinZ, outerMaxZ);
            candidate = new Vector3(x, y, z);
            maxAttempts--;
        }
        while (IsInsideInnerArea(candidate, innerMinX, innerMaxX, innerMinZ, innerMaxZ) && maxAttempts > 0);

        return candidate;
    }

    private bool IsInsideInnerArea(Vector3 pos, float minX, float maxX, float minZ, float maxZ)
    {
        return pos.x > minX && pos.x < maxX && pos.z > minZ && pos.z < maxZ;
    }
}
