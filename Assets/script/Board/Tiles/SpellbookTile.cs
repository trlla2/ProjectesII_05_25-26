using UnityEngine;

public class SpellbookTile : BoardTile
{
    [SerializeField] private SpellbookScreen _openSpellbookScreen;
    [SerializeField] private GameObject warning;
    public override void Interact()
    {
        _openSpellbookScreen.CreateSpellsButtons();
        Camera.main.GetComponent<TileClickHandler>().HandleBoardRaycast(false);

        _openSpellbookScreen.OpenSpellBook();

        warning.SetActive(false);
    }

    private void Start()
    {
        bool hasUsedSpellbook = false;

        foreach (Spell spell in GameManager.Instance.PlayerStats.KnownSpells)
        {
            if (SaveManager.Instance.HasUsedSpell(spell))
            {
                hasUsedSpellbook = true;
                break;
            }
        }
        warning.SetActive(!hasUsedSpellbook);
    }
}
