using System.Collections.Generic; 
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUIHandler : MonoBehaviour
{
    [Header("Description")]
    [SerializeField]
    private TMP_Text nameText;

    [Header("HealthBar")]
    [SerializeField]
    private Slider healthBar;

    [Header("Damage Text")]
    [SerializeField]
    private TMP_Text damageText;
    [SerializeField]
    private TranslatorUI damageTextTranslator;
    [SerializeField]
    private GameObject critImage; 

    [SerializeField]
    private float timeToChangeDmg = 0.3f; 

    private float currentDamageTimer = 0;
    private bool isShowingDamage = false;

    private Queue<DamageType> damageQueue = new Queue<DamageType>();

    [Header("Weakness Text")]
    [SerializeField]
    private TMP_Text weaknessText;
    [SerializeField]
    private TranslatorUI weaknessTextTranslator;

    [SerializeField]
    private float timeToChangeWeak = 0.3f;
    private float currentWeaknessTimer = 0;
    private bool isShowingWeakness = false;

    private EnemyBehaviour enemyBehaviour;

    private void Awake()
    {
        GameManager.Instance.CombatHandler.OnSetEnemyReference += SetEnemyRef;

        critImage.SetActive(false);
    }

    private void SetEnemyRef(EnemyBehaviour reference)
    {
        enemyBehaviour = reference;
        enemyBehaviour.OnHitRecived += ChangeHealthBar;
        enemyBehaviour.OnGetDamage += AddDamageToQueue;
        nameText.text = enemyBehaviour.GetEnemyType().Name + ", Level " + enemyBehaviour.GetEnemyType().Level;

    }

    private void Update()
    {
        HandleDamageQueue();
        HandleWeaknessTimer();
    }

    private void ChangeHealthBar(float health)
    {
        healthBar.value = health / enemyBehaviour.GetMaxHP();
    }

    private void AddDamageToQueue(DamageType damageType) { damageQueue.Enqueue(damageType); }

    private void HandleDamageQueue()
    {
        if (isShowingDamage)
        {
            currentDamageTimer += Time.deltaTime;

            if (currentDamageTimer >= timeToChangeDmg)
            {
                damageText.text = ""; 
                damageTextTranslator.ResetPosition(); 
                isShowingDamage = false;
                critImage.SetActive(false);
                currentDamageTimer = 0;  
            }
        }
        else if (damageQueue.Count > 0)
        {
            ShowNextDamage(damageQueue.Dequeue());
        }
    }

    private void ShowNextDamage(DamageType damageInfo)
    {
        isShowingDamage = true;
        currentDamageTimer = 0;

        damageTextTranslator.ResetPosition();
        damageText.text = damageInfo.damage.ToString();
        if (damageInfo.IsCrit)
            critImage.SetActive(true);

        SetDamageColor(damageInfo.element);

        damageTextTranslator.ToTarget();

        if (enemyBehaviour.GetEnemyType().IsWeakTo(damageInfo.element))
        {
            TriggerWeakness(damageInfo.element);
        }
    }

    private void SetDamageColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Red:
                damageText.color = Color.red;
                break;
            case ElementType.Blue:
                damageText.color = Color.blue;
                break;
            case ElementType.Green:
                damageText.color = Color.green;
                break;
            case ElementType.Yellow:
                damageText.color = Color.yellow;
                break;
            default:
                damageText.color = Color.white;
                break;
        }
    }

    private void TriggerWeakness(ElementType elementType)
    {
        isShowingWeakness = true;
        currentWeaknessTimer = 0;
        weaknessTextTranslator.ResetPosition();

        switch (elementType)
        {
            case ElementType.Red:
                weaknessText.color = Color.red;
                break;
            case ElementType.Blue:
                weaknessText.color = Color.blue;
                break;
            case ElementType.Green:
                weaknessText.color = Color.green;
                break;
            case ElementType.Yellow:
                weaknessText.color = Color.yellow;
                break;
        }
        weaknessTextTranslator.ToTarget();
    }

    private void HandleWeaknessTimer()
    {
        if (isShowingWeakness)
        {
            currentWeaknessTimer += Time.deltaTime;
            if (currentWeaknessTimer >= timeToChangeWeak)
            {
                isShowingWeakness = false;
                currentWeaknessTimer = 0;
            }
        }
    }

    private void OnDestroy()
    {
        if (enemyBehaviour != null)
        {
            enemyBehaviour.OnHitRecived -= ChangeHealthBar;
            enemyBehaviour.OnGetDamage -= AddDamageToQueue;
        }
    }
}