using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyUIHandler : MonoBehaviour
{
    [Header("HealthBar")]
    [SerializeField]
    private Slider healthBar;

    [Header("Attack TimeCircle")]
    [SerializeField]
    private Image attackTimeCircle;
    
    [Header("Damage Text")]
    [SerializeField]
    private TMP_Text damageText;
    [SerializeField]
    private TranslatorUI damageTextTranslator;

    [SerializeField]
    private float timeToChangeDmg = 0.3f;
    private float timerChangeDmg = 0;

    [Header("Weakness Text")]
    [SerializeField]
    private TMP_Text weaknessText;
    [SerializeField]
    private TranslatorUI weaknessTextTranslator;

    [SerializeField]
    private float timeToChangeWeak = 0.3f;
    private float timerChangeWeak = 0;

    private EnemyBehaviour enemyBehaviour;

    private void Start()
    {
        enemyBehaviour = GameManager.Instance.GetEnemyRef();

        enemyBehaviour.OnHitRecived += ChangeHealthBar;
        enemyBehaviour.OnGetDamage += StartChangeDamageText;
    }

    private void ChangeHealthBar(float health)
    {
        healthBar.value = health / enemyBehaviour.GetMaxHP();
    }

    private void StartChangeDamageText(DamageType damageType)
    {
        StartCoroutine(ChangeDamageText(damageType));
    }

    private IEnumerator ChangeDamageText(DamageType damageType)
    {
        
        damageTextTranslator.ResetPosition();

        damageText.text = damageType.damage.ToString();
        damageTextTranslator.ToTarget();


        switch (damageType.element)
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
        }
        if(enemyBehaviour.GetEnemyType().IsWeakTo(damageType.element))
            StartCoroutine(TriggerWeaknessText(damageType.element));

        while (timerChangeDmg < timeToChangeDmg)
        {
            timerChangeDmg += Time.deltaTime;
            yield return null;
        }

        damageTextTranslator.ResetPosition();
        timerChangeDmg = 0;
        damageText.text = "";
        
    }

    private IEnumerator TriggerWeaknessText(ElementType elementType)
    {
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
        while (timerChangeWeak < timeToChangeWeak)
        {
            timerChangeWeak += Time.deltaTime;
            yield return null;
        }

        //weaknessTextTranslator.ResetPosition();
        timerChangeWeak = 0;
    }

    private void OnDestroy()
    {
        enemyBehaviour.OnHitRecived -= ChangeHealthBar;
        enemyBehaviour.OnGetDamage -= StartChangeDamageText;
    }
}
    
