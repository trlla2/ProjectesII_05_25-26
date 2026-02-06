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
    private List<EnemyType> currentEnemyList;

    private EnemyBehaviour enemyInCombat;

    private EnemyType forcedEnemyType;


    [SerializeField]private TutorialManager tutorialManager;
    

    private int expMultiplyer = 4;
    private int moneyMultiplyer = 4;

    private SpellHoverHandler spellHoverHandler;

    public delegate void GetOnEndBattle(bool isWin, int exp, int money);
    public event GetOnEndBattle OnEndBattle;

    public event System.Action OnEnterBattle;
    public event System.Action OnEnterBoard;

    public event System.Action OnPlayerMove;
    public event System.Action OnPlayerEXPChange;
    public event System.Action OnPlayerLevelUp;
    public event System.Action OnPlayerMoneyChange;
    public event System.Action OnPlayerHitRecived;
    public event System.Action OnPlayerHealed;
    public event System.Action OnPlayerHitEvaded;
    public event System.Action OnPlayerInventoryChange;

    public event System.Action<EnemyBehaviour> OnSetEnemyReference;


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        currentEnemyList = new List<EnemyType>(enemyTypes);
    }

    public void EnterBattle() { OnEnterBattle?.Invoke(); }
    public void EnterBoard() { OnEnterBoard?.Invoke(); }
    public void SetForcedEnemy(EnemyType type)
    {
        forcedEnemyType = type;
    }

    public void SetEnemyRef(EnemyBehaviour enemyRef)
    {
        enemyInCombat = enemyRef;
        enemyInCombat.SetEnemyType(null);


        EnemyType typeToUse;
        Debug.Log("aaaaaaaaaaaaaaaaaaaa");

        if (forcedEnemyType != null)
        {
            typeToUse = forcedEnemyType;
            forcedEnemyType = null;
        }
        else
        {
            typeToUse = PickRandomEenemy();
        }

        //no override on tutorial enemies
        if (tutorialManager == null || !tutorialManager.IsTutorialActive())
        {
            enemyInCombat.SetEnemyType(typeToUse);
        }

        //no auto-start battle during tutorial
        if (tutorialManager == null || !tutorialManager.IsTutorialActive())
        {
            enemyInCombat?.StartBattle();
        }

       OnSetEnemyReference?.Invoke(enemyRef);
    }
    
    private EnemyType PickRandomEenemy()
    {
        int index = Random.Range(0, currentEnemyList.Count);
        EnemyType type = currentEnemyList[index];
        currentEnemyList.RemoveAt(index);

        if (currentEnemyList.Count <= 0)
        {
            currentEnemyList.Clear();
            currentEnemyList = new List<EnemyType>(enemyTypes);
        }

        return type;
    }
    
    public void DeleteEnemyRef() { enemyInCombat = null; }

    public void AttackEnemy(ElementType element)
    {
        enemyInCombat?.DealDamage(playerStats.AttackWithElement(element) );
    }

    public void NotifyPlayerMove()
    {
        OnPlayerMove?.Invoke();
    }

    public void AttackPlayer(float damage)
    {
        if (playerStats.EvadeAttack())
        {
            OnPlayerHitEvaded?.Invoke();
            return;
        }

        playerStats.Damage(damage);
        Camera.main.GetComponent<CameraShake>().Shake(0.2f);

        OnPlayerHitRecived.Invoke();

        if(!playerStats.IsAlive())
            OnPlayerDead();
    }

    public void HealPlayer(int amount)
    {
        playerStats.Heal(amount);
        OnPlayerHealed?.Invoke();
    }

    public bool UsePlayerMoney(int amount)
    {
        return playerStats.UseMoney(amount);
    }

    public bool AddItemToPlayerInventory(Item item)
    {
        bool isItemAdded = playerStats.AddItemToInventory(item);
        OnPlayerInventoryChange?.Invoke();
        
        return isItemAdded;
    }

    public void RemoveItemFromPlayerInventory(Item item) 
    { 
        playerStats.RemoveItemFromInventory(item);
        OnPlayerInventoryChange?.Invoke();
    }

    public void UpgradePlayerElementLevel(ElementType type)
    {
        playerStats.UpgradeElementalLevel(type);
        playerStats.LevelUp();
    }

    public void AddSpellToPlayerSpellBook(Spell spell, int slot) { playerStats.AddSpellToSpellBook(spell, slot); }
    public bool SpellExistOnPlayerSpellBook(Spell spell)
    {
        foreach(Spell s in playerStats.SpellBook)
        {
            Debug.Log($"{spell.name} is {s}");
            if(s == spell)
                return true;
        }
        return false;
    }
    public void ClearPlayerSpellBook() { playerStats.ClearSpellBook(); }
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

    public void SetHoverHandlerRef(SpellHoverHandler spellHover) { spellHoverHandler = spellHover; }
    public void DeleteHoverHandlerRef() { spellHoverHandler = null; }
    public void HoverSpell(Spell spell)
    {
        spellHoverHandler.SetupSpellUI(spell);
        spellHoverHandler.Show();
    }
    public void UnHoverSpell() { spellHoverHandler.UnShow(); } 

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    public float GetPlayerHP() => playerStats.Hp;
    public float GetPlayerHPPercentage() => playerStats.Hp / (playerStats.MaxHp + playerStats.ExtraHealth);
    public int GetPlayerLevel() => playerStats.Level;
    public int GetPlayerEXP() => playerStats.ExperiencePoints;
    public int GetPlayerEXPForNextLevel() => playerStats.GetEXPForNextLevel();
    public int GetPlayerEXPForLastLevel() => playerStats.GetEXPForLastLevel();
    public int GetPlayerMoney() => playerStats.Money;
    public List<Spell> GetPlayerSpellBook() => new List<Spell>(playerStats.SpellBook);
    public Spell GetSpellFromPlayerSpellBook(int slotIndex) => playerStats.SpellBook[slotIndex];
    public int PlayerSpellBookSize => playerStats.SpellBookSize;
    public List<Item> GetPlayerInventory() => playerStats.Inventory;
    public EnemyBehaviour GetEnemyRef() => enemyInCombat;
}