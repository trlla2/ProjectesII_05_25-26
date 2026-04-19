using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFoggedTilesShower : MonoBehaviour
{
    [SerializeField] private BoardManager _boardManager;
    [SerializeField] private List<FogTile> _foggedTiles;

    public void ShowFoggedTiles()
    {
        GameManager.Instance.OnEnterBoard -= ShowFoggedTiles;

        foreach (FogTile foggedTile in _foggedTiles)
        {
            if (foggedTile.FoggedTileData != null)
            {
                _boardManager.ReplaceTileType(foggedTile, foggedTile.FoggedTileData);
            }
        }
    }
}
