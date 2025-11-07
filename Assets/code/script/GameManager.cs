using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private EnemyBehaviour enemyInCombat;
    
    private PlayerStats playerStats;

    public delegate void GetOnEndGame(bool isWin);
    public event GetOnEndGame OnEndGame;

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
        OnEndGame?.Invoke(false);
    }

    public void OnEnemyDead()
    {
        DeleteEnemyRef();
        OnEndGame?.Invoke(true);
    }

    public void SetPlayerRef(PlayerStats playerRef) { playerStats = playerRef; Debug.Log("PlayerStats assinged"); }
}
