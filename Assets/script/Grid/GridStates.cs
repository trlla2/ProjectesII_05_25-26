using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using DG.Tweening;

public enum GridStateType
{
    Initialize,
    Idle,
    Refill,
    Reroll,
    Processing,
    ValidateSpell,
    Swap,
}

public interface IGridState
{
    void Enter();
    void Update();
    void Exit();
    GridStateType GetStateType();
}

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
            idleTimer = 0f;
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

public class RefillState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private bool isRefilling;
    private float refillDuration = 0.15f;

    private System.Action onRefillComplete;

    public RefillState(Grid gridController, VisualGrid visual, System.Action onRefillComplete = null)
    {
        grid = gridController;
        visualGrid = visual;
        this.onRefillComplete = onRefillComplete;
    }

    public void Enter()
    {
        visualGrid.RemoveAllHighlights();
        isRefilling = true;
        PerformRefill();
    }

    public void Update()
    {
        if (!isRefilling)
        {
            if (onRefillComplete != null)
            {
                onRefillComplete.Invoke();
            }
            else
            {
                grid.ChangeState(new ValidateSpellState(grid, GameManager.Instance.PlayerStats.GetSpellBook()));
            }
        }
    }

    public void Exit()
    {
        isRefilling = false;
    }

    public GridStateType GetStateType() => GridStateType.Refill;

    private void PerformRefill()
    {
        //pass through the slots that were emptied by gravity
        List<Vector2Int> movedPositions = grid.GravityHandler.ApplyGravity();
        grid.StartCoroutine(RefillSequence(movedPositions, refillDuration));
    }

    private IEnumerator RefillSequence(List<Vector2Int> movedPositions, float gravityDuration)
    {
        if (visualGrid != null && movedPositions != null && movedPositions.Count > 0)
        {
            yield return grid.StartCoroutine(visualGrid.AnimateGravity(movedPositions, gravityDuration));
        }

        grid.RetargetActiveSpell();

        List<Vector2Int> emptyPositions = GetEmptyPositions();

        if (emptyPositions.Count > 0)
        {
            grid.FillEmptySlots(grid.SpawnableElements, emptyPositions);

            foreach (var pos in emptyPositions)
            {
                Element element = grid.GridData.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    visualGrid.EnsureVisualAt(pos, element);
                }
            }

            foreach (var pos in emptyPositions)
            {
                ElementVisual visual = grid.GridData.GetVisualAt(pos);
                if (visual != null)
                    visual.SetSelectable(false);
            }

            visualGrid.FillEmpty(emptyPositions, refillDuration);

            yield return new WaitForSeconds(refillDuration);

            //finalize visuals 
            for (int x = 0; x < grid.GridData.GridSize; x++)
            {
                for (int y = 0; y < grid.GridData.GridSize; y++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    ElementVisual visual = grid.GridData.GetVisualAt(pos);

                    if (visual != null)
                    {
                        visual.transform.position = visualGrid.GridToWorldPosition(pos);
                        visual.ResetSpriteTransform();
                        visual.SetWorldPosition(visualGrid.GridToWorldPosition(pos));

                        Element element = grid.GridData.GetElementAt(pos);
                        visual.Initialize(pos, element);
                        visual.SetSelectable(true);
                    }
                }
            }

            isRefilling = false;
        }
        else
        {
            isRefilling = false;
        }
    }

    private List<Vector2Int> GetEmptyPositions()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GridData.GetElementAt(pos);

                if (element == grid.EmptyElement || element == null)
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        return emptyPositions;
    }
}

public class RerollState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private List<Element> availableElements;
    private List<Spell> availableSpells;
    private bool isAnimating;

    private float scrollDuration = 1.5f;
    private float columnDelay = 0.25f;
    private int scrollTimes = 5;

    public RerollState(Grid gridController, VisualGrid visual,
                       List<Element> elements, List<Spell> spells, int times = 3)
    {
        grid = gridController;
        visualGrid = visual;
        availableElements = elements;
        availableSpells = spells;
        scrollTimes = Mathf.Max(1, times);
    }

    public void Enter()
    {
        visualGrid.RemoveAllHighlights();
        isAnimating = true;
        grid.StartCoroutine(PerformReroll());
    }

    public void Update()
    {
        if (!isAnimating)
        {
            grid.ChangeState(new ValidateSpellState(grid, GameManager.Instance.PlayerStats.GetSpellBook()));
        }
    }

    public void Exit()
    {
        isAnimating = false;
    }

    public GridStateType GetStateType()
    {
        return GridStateType.Reroll;
    }

    private IEnumerator PerformReroll()
    {
        grid.NotifyRerollStarted();

        int gridSize = grid.GridData.GridSize;

        Element[,] finalGrid = GenerateFinalGrid();

        if (availableSpells != null && availableSpells.Count > 0)
        {
            Spell randomSpell = availableSpells[Random.Range(0, availableSpells.Count)];
            TryInsertSpellIntoGrid(finalGrid, randomSpell.Recipe);
        }

        yield return grid.StartCoroutine(
            visualGrid.PlayRerollAnimation(scrollTimes, scrollDuration, columnDelay, availableElements, finalGrid)
        );

        ApplyFinalGrid(finalGrid);
        FinalizeVisuals();

        grid.NotifyRerollCompleted();

        isAnimating = false;
    }

    private Element[,] GenerateFinalGrid()
    {
        int size = grid.GridData.GridSize;
        Element[,] result = new Element[size, size];
        Element bombEl = grid.BombElement;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element current = grid.GridData.GetElementAt(pos);

                // Preserve the bomb element so the active BombSpell stays valid
                if (bombEl != null && current == bombEl)
                    result[x, y] = bombEl;
                else
                    result[x, y] = grid.PickWeightedRandom(availableElements);
            }
        }

        return result;
    }

    //Cant use the spellinsert interface since we need to reroll the whole grid and working with a temp grid
    //rather than the actual griddata, so we need to reimplement it here
    private void TryInsertSpellIntoGrid(Element[,] targetGrid, List<Element> recipe)
    {
        if (recipe == null || recipe.Count == 0)
            return;

        int size = targetGrid.GetLength(0);

        //Collect bomb positions so spell paths never overwrite them, mb overrkillusing a hashset but we can use 2 bombs in the future mb?
        HashSet<Vector2Int> bombPositions = new HashSet<Vector2Int>();
        Element bombEl = grid.BombElement;
        if (bombEl != null)
        {
            for (int x = 0; x < size; x++)
                for (int y = 0; y < size; y++)
                    if (targetGrid[x, y] == bombEl)
                        bombPositions.Add(new Vector2Int(x, y));
        }

        int maxAttempts = 50;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2Int startPos = new Vector2Int(
                Random.Range(0, size),
                Random.Range(0, size)
            );

            if (bombPositions.Contains(startPos))
                continue;

            List<Vector2Int> path = GenerateRandomPathFromStart(size, recipe.Count, startPos, bombPositions);

            if (path != null && path.Count == recipe.Count)
            {
                for (int i = 0; i < recipe.Count; i++)
                {
                    Vector2Int pos = path[i];
                    targetGrid[pos.x, pos.y] = recipe[i];
                }
                return;
            }
        }
    }

    private List<Vector2Int> GenerateRandomPathFromStart(int gridSize, int length, Vector2Int start, HashSet<Vector2Int> excluded = null)
    {
        if (length > gridSize * gridSize)
            return null;

        List<Vector2Int> path = new List<Vector2Int> { start };
        HashSet<Vector2Int> visited = new HashSet<Vector2Int> { start };

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (path.Count < length)
        {
            Vector2Int current = path[path.Count - 1];
            List<Vector2Int> neighbors = new List<Vector2Int>();

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (next.x >= 0 && next.x < gridSize &&
                    next.y >= 0 && next.y < gridSize &&
                    !visited.Contains(next) &&
                    (excluded == null || !excluded.Contains(next)))
                {
                    neighbors.Add(next);
                }
            }

            if (neighbors.Count == 0)
                return null;

            Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
            path.Add(chosen);
            visited.Add(chosen);
        }

        return path;
    }

    private void ApplyFinalGrid(Element[,] finalGrid)
    {
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                grid.GridData.SetElementAt(new Vector2Int(x, y), finalGrid[x, y]);
            }
        }
    }

    private void FinalizeVisuals()
    {
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GridData.GetVisualAt(pos);
                Element element = grid.GridData.GetElementAt(pos);

                if (visual != null)
                {
                    visual.transform.position = visualGrid.GridToWorldPosition(pos);
                    visual.Initialize(pos, element);
                    visual.ResetSpriteTransform();
                    visual.SetWorldPosition(visualGrid.GridToWorldPosition(pos));
                    visual.SetSelectable(true);
                }
                else if (element != null)
                {
                    visualGrid.EnsureVisualAt(pos, element);
                }
            }
        }
    }
}

public class EnemySpellState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private List<Vector2Int> positions;
    private System.Action onComplete;
    private bool isProcessing;
    private float refillDuration = 0.15f;

    private bool rerollAfterProcess = false;

    public EnemySpellState(Grid gridController, VisualGrid visual, List<Vector2Int> positionsToProcess, 
                           System.Action onCompleteCallback, bool rerollAfter = false)
    {
        grid = gridController;
        visualGrid = visual;
        positions = positionsToProcess ?? new List<Vector2Int>();
        onComplete = onCompleteCallback;
        this.rerollAfterProcess = rerollAfter;
    }

    public void Enter()
    {
        isProcessing = true;
        grid.StartCoroutine(ProcessSequence());
    }

    public void Update()
    {
        if (!isProcessing)
        {

            if (rerollAfterProcess)
            {
                grid.ChangeState(new RerollState(grid, visualGrid, grid.SpawnableElements,
                    GameManager.Instance.PlayerStats.GetSpellBook(), 3));
            }
            else
            {
                grid.ChangeState(new IdleState(grid));
            }
        }
    }

    public void Exit()
    {
        isProcessing = false;
    }

    public GridStateType GetStateType()
    {
        return GridStateType.Processing;
    }

    private IEnumerator ProcessSequence()
    {
        if (positions == null || positions.Count == 0)
        {
            isProcessing = false;
            onComplete?.Invoke();
            yield break;
        }

        grid.InstantVisualEmpty(positions);

        List<Vector2Int> movedPositions = grid.GravityHandler.ApplyGravity();

        if (visualGrid != null && movedPositions != null && movedPositions.Count > 0)
        {
            yield return grid.StartCoroutine(visualGrid.AnimateGravity(movedPositions, refillDuration));
        }

        List<Vector2Int> emptyPositions = GetEmptyPositions();

        if (emptyPositions.Count > 0)
        {
            grid.FillEmptySlots(grid.SpawnableElements, emptyPositions);

            foreach (var pos in emptyPositions)
            {
                Element element = grid.GridData.GetElementAt(pos);
                if (element != null && element != grid.EmptyElement)
                {
                    visualGrid.EnsureVisualAt(pos, element);
                }
            }

            foreach (var pos in emptyPositions)
            {
                ElementVisual visual = grid.GridData.GetVisualAt(pos);
                if (visual != null)
                    visual.SetSelectable(false);
            }

            visualGrid.FillEmpty(emptyPositions, refillDuration);

            // Wait for fill animation to complete then finalize
            yield return new WaitForSeconds(refillDuration);
            yield return grid.StartCoroutine(CompleteRefillAfterDelay(0f));
        }
        else
        {
            // no refill needed, finish processing now
            isProcessing = false;
            onComplete?.Invoke();
        }
    }

    private List<Vector2Int> GetEmptyPositions()
    {
        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        int size = grid.GridData.GridSize;

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                Element element = grid.GridData.GetElementAt(pos);

                if (element == grid.EmptyElement || element == null)
                {
                    emptyPositions.Add(pos);
                }
            }
        }

        return emptyPositions;
    }

    private IEnumerator CompleteRefillAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        for (int x = 0; x < grid.GridData.GridSize; x++)
        {
            for (int y = 0; y < grid.GridData.GridSize; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                ElementVisual visual = grid.GridData.GetVisualAt(pos);

                if (visual != null)
                {
                    visual.transform.position = visualGrid.GridToWorldPosition(pos);
                    visual.ResetSpriteTransform();
                    visual.SetWorldPosition(visualGrid.GridToWorldPosition(pos));

                    Element element = grid.GridData.GetElementAt(pos);
                    visual.Initialize(pos, element);
                    visual.SetSelectable(true);
                }
            }
        }

        isProcessing = false;
        onComplete?.Invoke();
    }
}

//check if there are available spells to the player
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

public class ItemProcessingState : IGridState
{
    private Grid grid;

    public ItemProcessingState(Grid gridController)
    {
        grid = gridController;
    }

    public void Enter() { }
    public void Update() { }
    public void Exit() { }
    public GridStateType GetStateType() => GridStateType.Processing;
}

public class SwapState : IGridState
{
    private Grid grid;
    private VisualGrid visualGrid;
    private List<(Vector2Int, Vector2Int)> swapPairs;
    private float previewDuration;
    private float swapDuration;
    private bool isSwapping;

    public SwapState(Grid gridController, VisualGrid visual, List<(Vector2Int, Vector2Int)> pairs, float previewDuration, float swapDuration)
    {
        grid = gridController;
        visualGrid = visual;
        swapPairs = pairs;
        this.previewDuration = previewDuration;
        this.swapDuration = swapDuration;
    }

    public void Enter()
    {
        isSwapping = true;
        grid.StartCoroutine(PerformSwap());
    }

    public void Update()
    {
        if (!isSwapping)
            grid.ChangeState(new IdleState(grid));
    }

    public void Exit()
    {
        isSwapping = false;
    }

    public GridStateType GetStateType() => GridStateType.Swap;

    private IEnumerator PerformSwap()
    {
        // Phase 1: warn with ScaleAnimator blink on all elements being swapped
        yield return grid.StartCoroutine(visualGrid.PreviewSwapTargets(swapPairs, previewDuration));

        // Phase 2: swap data – visuals are now in correct data slots but old screen positions
        foreach (var pair in swapPairs)
            grid.GridData.SwapElements(pair.Item1, pair.Item2);

        // Phase 3: animate visuals from their old positions to the new grid positions
        yield return grid.StartCoroutine(visualGrid.AnimateEnemySwap(swapPairs, swapDuration));

        isSwapping = false;
    }
}