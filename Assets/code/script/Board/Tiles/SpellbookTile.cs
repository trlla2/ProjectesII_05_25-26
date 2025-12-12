using UnityEngine;

public class SpellbookTile : BoardTile
{
    [SerializeField] private SpellbookScreen _openSpellbookScreen;

    public override void Interact()
    {
        _openSpellbookScreen.CreateSpellsButtons();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);

        _openSpellbookScreen.gameObject.SetActive(true);
    }
}
