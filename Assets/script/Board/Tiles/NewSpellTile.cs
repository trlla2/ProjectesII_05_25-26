using UnityEngine;

public class NewSpellTile : BoardTile
{
    [Header("Spell")]
    [SerializeField] private Spell spell;
    [SerializeField] private BoardTileData postInteractionTile;
    [SerializeField] private AudioSource newSpellSFX;

    public Spell Spell => spell;
    public override void Interact()
    {
        newSpellSFX.Play();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);
        _boardManager.GetNewSpell(this);

    }

    public void Replace()
    {
        _boardManager.ReplaceTileType(this, postInteractionTile);
    }
}
