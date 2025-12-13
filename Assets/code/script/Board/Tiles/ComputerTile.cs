using UnityEngine;
using UnityEngine.UI;

public class ComputerTile : BoardTile
{
    [SerializeField] private GameObject _openComputerScreen;   
    public override void Interact()
    {
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        _openComputerScreen.SetActive(true);
    }

}
