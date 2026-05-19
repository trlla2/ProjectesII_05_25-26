using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyType", menuName = "Game/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [Header("Data")]
    //new bc we override the default name from obj
    [SerializeField] new private string name;

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

    [Header("Rewards")]
    [SerializeField] private int goldReward = 100;
    [SerializeField] private int expReward = 4;
    public int GoldReward => goldReward;
    public int ExpReward => expReward;

    [Header("Timed Enemy Settings")]
    [SerializeField] private float attackTimer = 6f;

    [Header("Lvl2 Timed Settings")]
    [SerializeField] private float lvl2Countdown = 8f; 
    [SerializeField] private float lvl2BonusTimeOnClear = 2f; 

    [Header("Continuously Timed Settings")]
    [SerializeField] private float continuousSelectionInterval = 2.5f;

    [Header("Hidden Enemy Settings")]
    [SerializeField] private float visibilityDuration = 2.5f;
    [SerializeField] private Sprite hiddenSprite;

    [Header("Blocker Enemy Settings")]
    [SerializeField] private int blockerCount = 2;
    [SerializeField] private float blockerDamagePercent = 0.25f; // 25% of enemy health per block break

    [Header("Drops")]
    [SerializeField] private bool haveDrops = false;
    [SerializeField] private List<Spell> spellDrops = new List<Spell>();

    [Header("Swapper Enemy Settings")]
    [SerializeField] private float swapperSwapInterval = 2.5f;


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
                return new ColumnDestroySpell(damagePerElement, moveCount);

            case EnemySpellType.RandomDestroy:
                return new RandomDestroySpell(elementCount, damagePerElement, moveCount);

            case EnemySpellType.SingleTimedTarget:
                return new SingleElementTimedSpell(attackTimer, damagePerElement);

            case EnemySpellType.DiagonalDestroy:
                return new DiagonalDestroySpell(damagePerElement, moveCount);

            case EnemySpellType.BossStage1Attack:
                return new BossStage1Spell(damagePerElement, moveCount);

            case EnemySpellType.ColumnDestroyLvl2:
                return new ColumnDestroyLvl2Spell(lvl2Countdown, damagePerElement, lvl2BonusTimeOnClear);

            case EnemySpellType.DiagonalDestroyLvl2:
                return new DiagonalDestroyLvl2Spell(lvl2Countdown, damagePerElement, lvl2BonusTimeOnClear);

            case EnemySpellType.RandomDestroyLvl2:
                return new RandomDestroyLvl2Spell(elementCount, lvl2Countdown, damagePerElement, lvl2BonusTimeOnClear);

            case EnemySpellType.BombAttack:
                return new BombSpell(damagePerElement, moveCount);

            case EnemySpellType.HiddenDestroy:
                return new HiddenDestroySpell(elementCount, damagePerElement, attackTimer, visibilityDuration, hiddenSprite, lvl2BonusTimeOnClear);

            case EnemySpellType.TutorialStage2FixedTargets:
                return new TutorialStage2FixedTargetsSpell(damagePerElement, moveCount);

            case EnemySpellType.TutorialStage2TimedSpell:
                return new TutorialStage2TimedSpell(attackTimer, damagePerElement);

            case EnemySpellType.ContinuouslyTimedTarget:
                return new ContinuouslyTimedSpell(continuousSelectionInterval, attackTimer, damagePerElement, lvl2BonusTimeOnClear);

            case EnemySpellType.BlockerAttack:
                return new BlockerSpell(blockerCount, blockerDamagePercent, attackTimer, damagePerElement);

            case EnemySpellType.SwapperAttack:
                return new SwapperSpell( swapperSwapInterval, attackTimer, damagePerElement, lvl2BonusTimeOnClear);

            case EnemySpellType.HalfScreenHiddenDestroy:
                return new HalfScreenHiddenDestroySpell(damagePerElement, attackTimer, visibilityDuration, hiddenSprite, lvl2BonusTimeOnClear);

            case EnemySpellType.SnakeLineTimedTarget:
                return new ContinousLineTimedEnemy(continuousSelectionInterval, attackTimer, damagePerElement, lvl2BonusTimeOnClear);

            case EnemySpellType.DoubleBombAttack:
                return new DoubleBomberSpell(damagePerElement, moveCount);

            default:
                return new ColumnDestroySpell(damagePerElement, moveCount);
        }
    }

    public Spell GetSpellDrop()
    {
        if (!haveDrops || spellDrops == null || spellDrops.Count == 0)
            return null;

        List<Spell> availableDrops = new List<Spell>();
        foreach (Spell s in spellDrops)
        {
            if (s != null && !GameManager.Instance.PlayerStats.KnownSpells.Contains(s))
            {
                availableDrops.Add(s);
            }
        }

        if (availableDrops.Count == 0)
            return null;

        int randomIndex = Random.Range(0, availableDrops.Count);
        Spell droppedSpell = availableDrops[randomIndex];

        return droppedSpell;
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
    TutorialStage2TimedSpell,
    ContinuouslyTimedTarget,
    BlockerAttack,
    SwapperAttack,
    HalfScreenHiddenDestroy,
    SnakeLineTimedTarget,
    DoubleBombAttack
}