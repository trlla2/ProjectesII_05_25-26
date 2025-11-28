using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyType", menuName = "Game/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [Header("Basic Stats")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float fireRate = 3f;
    [SerializeField] private int ElementCount = 3;

    [Header("Elemental Properties")]
    [SerializeField] private List<ElementType> weaknesses = new List<ElementType>();
    [SerializeField] private float weaknessMultiplier = 2f;
    [SerializeField] private List<ElementType> resistances = new List<ElementType>();
    [SerializeField] private float resistanceMultiplier = 0.5f;

    [Header("Art")]
    [SerializeField] private Sprite enemySprite;

    [Header("Spell Behavior")]
    [SerializeField] private EnemySpellType spellType = EnemySpellType.ColumnDestroy;
    [SerializeField] private float damagePerElement = 4f;
    [SerializeField] private float spellCastDuration = 1.5f;

    public float Health => health;
    public float Damage => damage;
    public float FireRate => fireRate;
    public Sprite EnemySprite => enemySprite;
    public List<ElementType> Weaknesses => new List<ElementType>(weaknesses);
    public EnemySpellType SpellType => spellType;
    public float DamagePerElement => damagePerElement;
    public float SpellCastDuration => spellCastDuration;
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
                return new ColumnDestroySpell(damagePerElement, spellCastDuration);

            case EnemySpellType.RandomDestroy:
                return new RandomDestroySpell(ElementCount, damagePerElement, spellCastDuration);

            default:
                return new ColumnDestroySpell(damagePerElement, spellCastDuration);
        }
    }
}

public enum EnemySpellType
{
    ColumnDestroy,
    RandomDestroy
}