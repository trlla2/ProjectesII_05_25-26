using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyType", menuName = "Game/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [Header("Data")]
    [SerializeField] private string name;

    [Header("Basic Stats")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private int elementCount = 3;
    [SerializeField] private int level;
    [SerializeField] private EnemyDifficulty difficulty;

    [Header("Elemental Properties")]
    [SerializeField] private List<ElementType> weaknesses = new List<ElementType>();
    [SerializeField] private float weaknessMultiplier = 2f;
    [SerializeField] private List<ElementType> resistances = new List<ElementType>();
    [SerializeField] private float resistanceMultiplier = 0.5f;

    [Header("Art")]
    [SerializeField] private Sprite enemySprite;
    [SerializeField] private RuntimeAnimatorController animatorController;

    [Header("Spell Behavior")]
    [SerializeField] private EnemySpellType spellType = EnemySpellType.ColumnDestroy;
    [SerializeField] private float damagePerElement = 4f;
    [SerializeField] private int moveCount = 2; // number of player moves until spell triggers

    [Header("TimerEnemy")]
    [SerializeField] private float AtackTimer = 4f;

    [Header("Lvl2 Timed Settings")]
    [SerializeField] private float lvl2Delay = 5f;
    [SerializeField] private float lvl2Countdown = 15f;

    [Header("Hidden Enemy Settings")]
    [SerializeField] private float visibilityDuration = 2.5f;
    [SerializeField] private Sprite hiddenSprite;

    public bool excludeBelowTargets = false;

    public float Health => health;
    public float Damage => damage;
    public Sprite EnemySprite => enemySprite;
    public List<ElementType> Weaknesses => new List<ElementType>(weaknesses);
    public EnemySpellType SpellType => spellType;
    public float DamagePerElement => damagePerElement;
    public int MoveCount => moveCount;
    public string Name => name;
    public int Level => level;
    public EnemyDifficulty Difficulty => difficulty;
    public RuntimeAnimatorController AnimatorController => animatorController;
    public float VisibilityDuration => visibilityDuration;
    public Sprite HiddenSprite => hiddenSprite;

    public float CalculateDamageTaken(ElementType elementType, float baseDamage)
    {
        if (weaknesses.Contains(elementType))
        {
            float finalDamage = baseDamage * weaknessMultiplier;
            Debug.Log($"Weakness hit: {elementType} | Multiplier: {weaknessMultiplier} | Total DMG: {finalDamage}");
            return finalDamage;
        }
        if (resistances.Contains(elementType))
        {
            return baseDamage * resistanceMultiplier;
        }

        return baseDamage;
    }

    public bool IsWeakTo(ElementType elementType)
    {
        return weaknesses.Contains(elementType);
    }

    public IEnemySpell CreateSpell()
    {
        switch (spellType)
        {
            case EnemySpellType.ColumnDestroy:
                return new ColumnDestroySpell(damagePerElement, moveCount, excludeBelowTargets);

            case EnemySpellType.RandomDestroy:
                return new RandomDestroySpell(elementCount, damagePerElement, moveCount, excludeBelowTargets);

            case EnemySpellType.SingleTimedTarget:
                return new SingleElementTimedSpell(AtackTimer, damagePerElement, excludeBelowTargets);

            case EnemySpellType.DiagonalDestroy:
                return new DiagonalDestroySpell(damagePerElement, moveCount, excludeBelowTargets);

            case EnemySpellType.BossStage1Attack:
                return new BossStage1Spell(damagePerElement, moveCount, excludeBelowTargets);

            case EnemySpellType.ColumnDestroyLvl2:
                return new ColumnDestroyLvl2Spell(damagePerElement, lvl2Delay, lvl2Countdown, excludeBelowTargets);

            case EnemySpellType.DiagonalDestroyLvl2:
                return new DiagonalDestroyLvl2Spell(damagePerElement, lvl2Delay, lvl2Countdown, excludeBelowTargets);

            case EnemySpellType.RandomDestroyLvl2:
                return new RandomDestroyLvl2Spell(elementCount, damagePerElement, lvl2Delay, lvl2Countdown, excludeBelowTargets);

            case EnemySpellType.BombAttack:
                return new BombSpell(damagePerElement, moveCount, excludeBelowTargets);

            case EnemySpellType.HiddenDestroy:
                return new HiddenDestroySpell(elementCount, damagePerElement, AtackTimer, excludeBelowTargets, visibilityDuration, hiddenSprite);

            case EnemySpellType.TutorialStage2FixedTargets:
                return new TutorialStage2FixedTargetsSpell(damagePerElement, moveCount, excludeBelowTargets);

            case EnemySpellType.TutorialStage2TimedSpell:
                return new TutorialStage2TimedSpell(AtackTimer, damagePerElement, excludeBelowTargets);

            default:
                return new ColumnDestroySpell(damagePerElement, moveCount, excludeBelowTargets);
        }
    }
}

public enum EnemySpellType
{
    ColumnDestroy,
    DiagonalDestroy,
    RandomDestroy,
    SingleTimedTarget,
    BossStage1Attack,
    ColumnDestroyLvl2,
    DiagonalDestroyLvl2,
    RandomDestroyLvl2,
    BombAttack,
    HiddenDestroy,
    TutorialStage2FixedTargets,
    TutorialStage2TimedSpell
}