using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public struct TutorialStep
{
    public TutorialStepType type;

    [Header("Popup / Toast")]
    public string text;
    public float autoDismissTime; // 0 = manual dismiss

    [Header("Highlight")]
    public List<Vector2Int> highlightPositions;
    public List<Vector2Int> allowedPath; // empty = full input

    [Header("Hand Animation")]
    public bool showHand;
    public List<Vector2Int> handPath;
    public float handDuration;

    [Header("Arrow")]
    public bool showArrow;
    public TutorialArrowTarget arrowFrom;
    public TutorialArrowTarget arrowTo;
    public string arrowLabel;

    [Header("Flow Control")]
    public TutorialAdvanceCondition advanceCondition;
    public bool pauseEnemyTimers;
    public bool lockInput;
}

[CreateAssetMenu(menuName = "Game/Tutorial/Tutorial Sequence")]
public class TutorialSequence : ScriptableObject
{
    public List<TutorialStep> steps;
}

public enum TutorialStepType
{
    Popup,
    Toast,
    HandDemo,
    FreePlay,
    WaitForSpellCast,
    WaitForEnemyDeath
}

public enum TutorialAdvanceCondition
{
    Tap,
    Timer,
    SpellCast,
    EnemyDeath,
    Auto
}

public enum TutorialArrowTarget
{
    None,
    GridPosition,
    PlayerHPBar,
    EnemyHPBar,
    SpellHUD
}
