using System.Collections.Generic;
using UnityEngine;

public class ColumnDestroySpell : EnemySpellBase
{
    private int columnIndex;

    public ColumnDestroySpell(float damage = 4f, int moves = 2, bool excludeBelowTargets = true, int column = -1)
    {
        damagePerElement = damage;
        remainingMoves = moves;
        initialMoves = moves;
        columnIndex = column;
        spellName = "Column Destroy";
        this.excludeBelowTargets = excludeBelowTargets;
    }

    public override void Execute(Grid grid)
    {
        targetedPositions.Clear();

        if (grid == null)
            return;

        int targetColumn;
        targetColumn = columnIndex >= 0 ? columnIndex : Random.Range(0, Grid.GridSize);

        for (int row = 0; row < Grid.GridSize; row++)
        {
            Vector2Int pos = new Vector2Int(targetColumn, row);
            if (grid.IsPositionValid(pos))
            {
                Element element = grid.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    targetedPositions.Add(pos);
                    ElementVisual visual = grid.GetElementVisualAt(pos);
                    if (visual != null)
                    {
                        visual.SetTargetedByEnemy(true);
                        visual.UpdateTargetVisual(remainingMoves, initialMoves, grid.EnemyTargetAnimDuration, grid.EnemyTargetFillCurve);
                    }
                }
            }
        }
    }

    //private void InsertFirstPlayerSpellBottomLeft(Grid grid)
    //{
    //    if (grid == null || GameManager.Instance == null)
    //        return;

    //    List<Spell> spellBook = GameManager.Instance.PlayerStats.GetSpellBook();
    //    if (spellBook == null || spellBook.Count == 0)
    //        return;

    //    List<Element> recipe = spellBook[0].Recipe;
    //    if (recipe == null || recipe.Count == 0)
    //        return;

    //    int y = 0;
    //    int count = Mathf.Min(3, recipe.Count);

    //    for (int x = 0; x < count; x++)
    //    {
    //        Vector2Int pos = new Vector2Int(x, y);

    //        if (!grid.IsPositionValid(pos))
    //            continue;

    //        grid.SetElementAt(pos, recipe[x]);

    //        ElementVisual visual = grid.GetElementVisualAt(pos);
    //        if (visual != null)
    //        {
    //            visual.UpdateVisual(recipe[x]);
    //        }
    //    }
    //}

}