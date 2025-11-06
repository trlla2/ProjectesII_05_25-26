using UnityEngine;
using UnityEngine.UI;

public class EnemyUIHandler : MonoBehaviour
{
    [Header("SETUP")]
    [SerializeField]
    private Slider healthBar;
    
    [SerializeField]
    private Image attackTimeCircle;

    private EnemyBehaviour enemyBehaviour;

    private void Start()
    {
        enemyBehaviour = this.gameObject.GetComponentInParent<EnemyBehaviour>();

        enemyBehaviour.OnHitRecived += ChangeHealthBar;
        enemyBehaviour.OnAttackTimeChange += ChangeAttackTimeCircle;
    }

    private void ChangeHealthBar(float health)
    {
        healthBar.value = health / enemyBehaviour.GetMaxHP();
    }

    private void ChangeAttackTimeCircle(float attackTime)
    {
        attackTimeCircle.fillAmount = 1-(attackTime/enemyBehaviour.GetFireRate());
    }

    private void OnDestroy()
    {
        enemyBehaviour.OnHitRecived -= ChangeHealthBar;
        enemyBehaviour.OnAttackTimeChange -= ChangeAttackTimeCircle;

    }
}
    
