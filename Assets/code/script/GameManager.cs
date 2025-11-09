using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Stage")]
    [SerializeField]
    private int stageLevel = 1;

    private EnemyBehaviour enemyInCombat;
    
    private PlayerStats playerStats;

    public delegate void GetOnEndBattle(bool isWin, int exp, int money);
    public event GetOnEndBattle OnEndBattle;

    private CameraShake cameraShake;
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

    private void Start()
    {
        cameraShake = Camera.main.GetComponent<CameraShake>();
    }

    public void AttackEnemy(List<DamageType> damageList)
    {
        enemyInCombat?.DealDamage(damageList);
    }

    public void AttackPlayer(float damage)
    {
        playerStats?.DealDamage(damage);
        cameraShake.Shake(0.2f);
    }

    public void SetEnemyRef(EnemyBehaviour enemyRef) { enemyInCombat = enemyRef; Debug.Log("enemyInCombat assinged"); }
    public void DeleteEnemyRef() {  enemyInCombat = null; }

    public void OnPlayerDead()
    {
        enemyInCombat?.StopEnemy();
        OnEndBattle?.Invoke(false, 0, 0);
    }

    public void OnEnemyDead()
    {
        int exp = stageLevel * 4;
        int money = stageLevel * 4;

        playerStats.OnWinBattle(exp, money);
        DeleteEnemyRef();
        OnEndBattle?.Invoke(true, exp, money);
    }

    public void SetPlayerRef(PlayerStats playerRef) { playerStats = playerRef; Debug.Log("PlayerStats assinged"); }

    
}
