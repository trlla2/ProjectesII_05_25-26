using UnityEngine;
using UnityEngine.UI;

public class ComputerTile : BoardTile
{
    [SerializeField] private GameObject _openComputerScreen;   
    public override void Interact()
    {
        _openComputerScreen.SetActive(true);
    }

}
