using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private PlayerStats playerStats = new PlayerStats();

    [Header("Stage")]
    [SerializeField]
    private int stageLevel = 1;

    [Header("List Of Enemies")]
    [SerializeField]
    private List<EnemyType> enemyTypes = new List<EnemyType>();

    private EnemyBehaviour enemyInCombat;
    
    private int expMultiplyer = 4;
    private int moneyMultiplyer = 4;

    // EVENTS
    public delegate void GetOnEndBattle(bool isWin, int exp, int money);
    public event GetOnEndBattle OnEndBattle;

    public event System.Action OnPlayerMove;

    public event System.Action OnPlayerEXPChange;
    public event System.Action OnPlayerLevelUp;
    public event System.Action OnPlayerMoneyChange;
    public event System.Action<float> OnPlayerHitRecived;
    public event System.Action OnPlayerHitEvaded;
    public event System.Action OnPlayerInventoryChange;
    

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

    public void SetEnemyRef(EnemyBehaviour enemyRef)
    {
        enemyInCombat = enemyRef;
        enemyInCombat.SetEnemyType(enemyTypes[Random.Range(0, enemyTypes.Count)]);
        enemyInCombat?.StartBattle();
        Debug.Log("enemyInCombat assinged");
    }
    public void DeleteEnemyRef() { enemyInCombat = null; }

    public void AttackEnemy(ElementType element)
    {
        enemyInCombat?.DealDamage(playerStats.AttackWithElement(element));

        OnPlayerMove?.Invoke();
    }

    //future use? Mb if the player misses a spell
    public void NotifyPlayerMove()
    {
        OnPlayerMove?.Invoke();
    }

    public void AttackPlayer(float damage)
    {
        if (playerStats.EvadeAttack())
        {
            OnPlayerHitEvaded?.Invoke();
            Debug.Log("EVADEDDDDDDDDDD");
            return;
        }

        playerStats.Damage(damage);
        Camera.main.GetComponent<CameraShake>().Shake(0.2f);

        OnPlayerHitRecived.Invoke(playerStats.Hp);

        if(!playerStats.IsAlive())
            OnPlayerDead();
    }

    public void HealPlayer(int amount)
    {
        playerStats.Heal(amount);
    }

    public bool UsePlayerMoney(int amount)
    {
        return playerStats.UseMoney(amount);
    }

    public void AddItemToPlayerInventory(Item item)
    {
        playerStats.AddItemToInventory(item);
        OnPlayerInventoryChange?.Invoke();
    }

    public void UpgradePlayerElementLevel(ElementType type)
    {
        playerStats.UpgradeElementalLevel(type);
        playerStats.LevelUp();
    }

    public void AddSpellToPlayerSpellBook(Spell spell) { playerStats.AddSpellToSpellBook(spell); }

    public void OnPlayerDead()
    {
        enemyInCombat?.StopBattle();
        OnEndBattle?.Invoke(false, 0, 0); // 0 cause player lose
    }

    public void OnEnemyDead()
    {
        int exp = stageLevel * expMultiplyer;
        int money = stageLevel * moneyMultiplyer;

        playerStats.AddExp(exp);
        OnPlayerEXPChange?.Invoke();

        playerStats.AddMoney(money);
        OnPlayerMoneyChange?.Invoke();

        DeleteEnemyRef();

        if(playerStats.CanLevelUp())
            OnPlayerLevelUp.Invoke();

        OnEndBattle?.Invoke(true, exp, money);
    }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    public float GetPlayerHP() => playerStats.Hp;
    public float GetPlayerMaxHP() => playerStats.MaxHp;
    public int GetPlayerLevel() => playerStats.Level;
    public int GetPlayerEXP() => playerStats.ExperiencePoints;
    public int GetPlayerEXPForNextLevel() => playerStats.GetEXPForNextLevel();
    public int GetPlayerMoney() => playerStats.Money;
    public List<Spell> GetPlayerSpellBook() => playerStats.SpellBook;
    public List<Item> GetPlayerInventory() => playerStats.Inventory;
    public EnemyBehaviour GetEnemyRef() => enemyInCombat;
}