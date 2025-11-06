using System.Collections;
using System.Collections.Generic;
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

    private EnemyBehaviour enemyBehaviour;

    private void Start()
    {
        enemyBehaviour = this.gameObject.GetComponentInParent<EnemyBehaviour>();

        enemyBehaviour.OnHitRecived += ChangeHealthBar;
        enemyBehaviour.OnAttackTimeChange += ChangeAttackTimeCircle;
        enemyBehaviour.OnDamageList += StartChangeDamageText;
    }

    private void ChangeHealthBar(float health)
    {
        healthBar.value = health / enemyBehaviour.GetMaxHP();
    }

    private void ChangeAttackTimeCircle(float attackTime)
    {
        attackTimeCircle.fillAmount = 1-(attackTime/enemyBehaviour.GetFireRate());
    }

    private void StartChangeDamageText(List<DamageType> damageList)
    {
        StartCoroutine(ChangeDamageText(damageList));
    }

    private IEnumerator ChangeDamageText(List<DamageType> damageList)
    {
        foreach (DamageType damage in damageList)
        {
            damageText.text = damage.damage.ToString();
            damageTextTranslator.ToTarget();


            switch (damage.element)
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

            while (timerChangeDmg < timeToChangeDmg)
            {
                timerChangeDmg += Time.deltaTime;
                yield return null;
            }

            damageTextTranslator.ResetPosition();
            timerChangeDmg = 0;
            damageText.text = "";
        }
    }

    private void OnDestroy()
    {
        enemyBehaviour.OnHitRecived -= ChangeHealthBar;
        enemyBehaviour.OnAttackTimeChange -= ChangeAttackTimeCircle;
        enemyBehaviour.OnDamageList -= StartChangeDamageText;
    }
}
    
