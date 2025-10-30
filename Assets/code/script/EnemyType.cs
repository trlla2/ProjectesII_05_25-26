using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NewEnemyType", menuName = "Game/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [Header("Basic Stats")]
    [Tooltip("Base health points")]
    [SerializeField]
    private float health = 100f;

    [Tooltip("Base damage dealt per attack")]
    [SerializeField] 
    private float damage = 10f;

    [Tooltip("Time between attacks in seconds")]
    [SerializeField]
    private float fireRate = 1f;

    

    [Tooltip("Elements enenmy is weak to")]
    [SerializeField]
    private List<ElementType> weaknesses = new List<ElementType>();

    [Tooltip("Multiplayer added to the total damage of the elements damage enemy resist")]
    [SerializeField]
    private float weaknessMultiplayer = 2f;

    [Tooltip("Damage with this elements will do X0.5 total damage")]
    [SerializeField]
    private List<ElementType> resistances = new List<ElementType>();

    [Tooltip("Multiplayer added to the total damage of the elements damage enemy resist")]
    [SerializeField]
    private float resistanceMultiplayer = 0.5f;

    [Header("Art Stuff")]
    [SerializeField]
    private Sprite enemySprite;

    [Tooltip("Based on damage by elemnts")]
    public float CalculateDamageTaken( ElementType elementType, float baseDamage)
    {
        float finalDamage = baseDamage;

        foreach (ElementType weakness in weaknesses)
        {
            if (weakness == elementType)
            {
                finalDamage *= weaknessMultiplayer;
                break;
            }
        }

        foreach (ElementType resistance in resistances)
        {
            if (resistance == elementType)
            {
                finalDamage *= resistanceMultiplayer;
                break;
            }
        }

        return finalDamage;
    }


    public float GetHealth() { return health; }
    public float GetDamage() { return damage; }
    public float GetFireRate() { return fireRate; }
    public Sprite GetSprite() { return enemySprite; }
}



