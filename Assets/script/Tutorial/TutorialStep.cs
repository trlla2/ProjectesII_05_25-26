using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;


[Serializable]
public struct TutorialStep
{
    public TutorialStepType type;

    [Header("Popup / Toast")]
    public string text /*=> LocalizationSettings.StringDatabase.GetLocalizedString("TutorialFixedTexts", )*/;
    public float autoDismissTime; // 0 = manual dismiss

    [Header("Highlight")]
    public List<Vector2Int> highlightPositions;
    public List<Vector2Int> allowedPath; // empty = full input

    [Header("Hand Animation")]
    public bool showHand;
    public List<Vector2Int> handPath;
    public float handDuration;

    [Header("Flow Control")]
    public TutorialAdvanceCondition advanceCondition;
    public bool pauseEnemyTimers;
    public bool lockInput;
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
