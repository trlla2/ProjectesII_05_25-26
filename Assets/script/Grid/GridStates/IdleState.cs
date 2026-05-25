using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class IdleState : IGridState
{
    private Grid grid;
    private SpellValidator spellValidator;

    private float idleTimer;
    private float hintDelay = 3f;
    private bool hintPlaying;
    private List<Vector2Int> currentHintPath;
    private List<ElementVisual> hintedVisuals;
    private Sequence hintSequence;

    private float bopScaleStrength = 0.2f;
    private float bopMoveUpStrength = 0.15f;
    private float bopDuration = 0.25f;

    public IdleState(Grid gridController)
    {
        grid = gridController;
        spellValidator = new SpellValidator();
    }

    public void Enter()
    {
        grid.VisualGrid.ShowInactiveGrid(false);
        idleTimer = 0f;
        hintPlaying = false;
        currentHintPath = null;
        hintedVisuals = null;
        hintSequence = null;
    }

    public void Update()
    {
        if (hintPlaying)
            return;

        idleTimer += Time.deltaTime;

        if (idleTimer >= hintDelay)
        {
            TryShowHint();
        }
    }

    public void Exit()
    {
        grid.VisualGrid.ShowInactiveGrid(true);
        StopHint();
    }

    public GridStateType GetStateType()
    {
        return GridStateType.Idle;
    }

    private void TryShowHint()
    {
        List<Spell> spells = null;
        if (GameManager.Instance != null && GameManager.Instance.PlayerStats != null)
            spells = GameManager.Instance.PlayerStats.GetSpellBook();

        HashSet<Vector2Int> blockedPositions = GetBlockedPositions();
        spellValidator.SetBlockedPositions(blockedPositions);

        List<Vector2Int> hintPath = spellValidator.FindFirstValidSpellPath(grid.GridData, spells);
        if (hintPath != null && hintPath.Count > 0)
        {
            currentHintPath = hintPath;
            PlayHintAnimation(hintPath);
        }
        else
        {
            grid.ChangeState(new ValidateSpellState(grid, spells));
        }
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

    private void PlayHintAnimation(List<Vector2Int> path)
    {
        // Always stop any running hint before starting a new one
        StopHint();

        hintedVisuals = new List<ElementVisual>();
        Sequence bop = DOTween.Sequence();

        for (int i = 0; i < path.Count; i++)
        {
            ElementVisual visual = grid.GridData.GetVisualAt(path[i]);
            if (visual == null)
                continue;

            hintedVisuals.Add(visual);
            bop.Append(visual.transform.DOPunchScale(Vector3.one * bopScaleStrength, bopDuration, 1, 0.5f));
            bop.Join(visual.transform.DOPunchPosition(new Vector3(0f, bopMoveUpStrength, 0f), bopDuration, 1, 0.5f));
        }

        hintSequence = bop.SetLoops(-1).SetEase(Ease.InOutSine).SetDelay(0.25f);
        hintPlaying = true;
        idleTimer = 0f;
    }

    private void StopHint()
    {
        hintSequence?.Kill();
        hintSequence = null;

        if (hintedVisuals != null)
        {
            foreach (ElementVisual visual in hintedVisuals)
            {
                if (visual == null)
                    continue;

                visual.transform.DOKill();
                visual.transform.localScale = Vector3.one;
                visual.transform.position = grid.VisualGrid.GridToWorldPosition(visual.GridPosition);
            }
            hintedVisuals = null;
        }

        currentHintPath = null;
        hintPlaying = false;
        idleTimer = 0f;
    }
}
