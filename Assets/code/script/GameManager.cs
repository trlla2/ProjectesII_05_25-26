using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Stage")]
    [SerializeField]
    private int stageLevel = 1;

    [Header("List Of Enemies")]
    [SerializeField]
    private List<EnemyType> enemyTypes = new List<EnemyType>();

    private EnemyBehaviour enemyInCombat;

    public delegate void GetOnEndBattle(bool isWin, int exp, int money);
    public event GetOnEndBattle OnEndBattle;

    public event System.Action OnPlayerMove;

    private int expMultiplyer = 4;
    private int moneyMultiplyer = 4;



    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void AttackEnemy(List<DamageType> damageList)
    {
        enemyInCombat?.DealDamage(damageList);

        OnPlayerMove?.Invoke();
    }

    //future use? Mb if the player misses a spell
    public void NotifyPlayerMove()
    {
        OnPlayerMove?.Invoke();
    }

    public void AttackPlayer(float damage)
    {
        PlayerStats.Instance.DealDamage(damage);
        Camera.main.GetComponent<CameraShake>().Shake(0.2f);
    }

    public void SetEnemyRef(EnemyBehaviour enemyRef) 
    { 
        enemyInCombat = enemyRef; 
        enemyInCombat.SetEnemyType(enemyTypes[Random.Range(0, enemyTypes.Count)]);
        enemyInCombat?.InitBatle();
        Debug.Log("enemyInCombat assinged"); 
    }
    public void DeleteEnemyRef() {  enemyInCombat = null; }

    public void OnPlayerDead()
    {
        enemyInCombat?.StopEnemy();
        OnEndBattle?.Invoke(false, 0, 0); // 0 cause player lose
    }

    public void OnEnemyDead()
    {
        int exp = stageLevel * expMultiplyer;
        int money = stageLevel * moneyMultiplyer;

        PlayerStats.Instance.OnWinBattle(exp, money);
        DeleteEnemyRef();
        OnEndBattle?.Invoke(true, exp, money);
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }
}
