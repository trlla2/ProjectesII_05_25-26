using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "NewEnemyType", menuName = "Game/Enemy Type")]
public class EnemyType : ScriptableObject
{
    [Header("Basic Stats")]
    [SerializeField]
    private float health = 100f;

    [SerializeField] 
    private float damage = 10f;

    [SerializeField]
    private float fireRate = 1f;

    

    [SerializeField]
    private List<ElementType> weaknesses = new List<ElementType>();

    [SerializeField]
    private float weaknessMultiplayer = 2f;

    [SerializeField]
    private List<ElementType> resistances = new List<ElementType>();

    [SerializeField]
    private float resistanceMultiplayer = 0.5f;

    [Header("Art Stuff")]
    [SerializeField]
    private Sprite enemySprite;

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



