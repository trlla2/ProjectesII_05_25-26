using System.Collections.Generic;
using UnityEngine;

// check if there are available spells to the player
public class ValidateSpellState : IGridState
{
    private Grid grid;
    private SpellValidator spellValidator;
    private List<Spell> availableSpells;
    private bool isValidating;

    public ValidateSpellState(Grid gridController, List<Spell> spells)
    {
        grid = gridController;
        spellValidator = new SpellValidator();
        availableSpells = spells;
    }

    public void Enter()
    {
        isValidating = true;
        ValidateGrid();
    }

    public void Update()
    {
        if (!isValidating)
        {
            grid.ChangeState(new IdleState(grid));
        }
    }

    public void Exit()
    {
        isValidating = false;
    }

    public GridStateType GetStateType()
    {
        return GridStateType.ValidateSpell;
    }

    private void ValidateGrid()
    {
        HashSet<Vector2Int> blockedPositions = GetBlockedPositions();
        spellValidator.SetBlockedPositions(blockedPositions);

        bool hasValidSpell = spellValidator.HasValidSpell(grid.GridData, availableSpells);

        if (!hasValidSpell)
        {
            if (grid.IsTutorialActive())
            {
                bool inserted = grid.TryInsertRandomSpell(availableSpells);
                if (!inserted)
                    grid.Reroll();
            }
            else
            {
                grid.Reroll();
            }
        }

        isValidating = false;
    }

    private HashSet<Vector2Int> GetBlockedPositions()
    {
        if (GameManager.Instance == null || GameManager.Instance.CombatHandler == null)
            return new HashSet<Vector2Int>();

        EnemyBehaviour enemy = GameManager.Instance.CombatHandler.GetEnemyRef();
        if (enemy == null)
            return new HashSet<Vector2Int>();

        return enemy.GetBlockedPositions();
    }
}
