using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private PlayerStats playerStats = new PlayerStats();
    public PlayerStats PlayerStats => playerStats;

    
    private int stageLevel = 1;
    public int StageLevel => stageLevel;

    private EnemyType forcedEnemyType;


    [SerializeField]private TutorialManager tutorialManager;
    

    private CombatHandler cH;
    public CombatHandler CombatHandler => cH;

    //Events
    public event System.Action OnEnterBoard;
    public event System.Action OnEnterBattle;
    
    public event System.Action OnPlayerMove;


    


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

    public void SetCombatHandler(CombatHandler cHRef) {  cH = cHRef; }
    public void EnterBoard() { OnEnterBoard?.Invoke(); }
    public void EnterBattle() { OnEnterBattle?.Invoke(); }


    public void SetForcedEnemy(EnemyType type) { forcedEnemyType = type; }

    public void NotifyPlayerMove() { OnPlayerMove?.Invoke(); }



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
        for(int i = 0; i < PlayerSpellBookSize; i++)
        {
            if(playerStats.SpellBook[i] == spell)
                return true;
        }
        return false;
    }
    public void ClearPlayerSpellBook() { playerStats.ClearSpellBook(); }

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
    public List<Spell> GetPlayerSpellBook()
    {
        List<Spell> spelL = new List<Spell>();

        for(int i = 0; i < PlayerSpellBookSize; i ++)
        {
            if(playerStats.SpellBook[i] != null)
                spelL.Add(playerStats.SpellBook[i]);
        }

        return spelL;
    }

    public Spell GetSpellFromPlayerSpellBook(int slotIndex) => playerStats.SpellBook[slotIndex];
    public int PlayerSpellBookSize => playerStats.SpellBookSize;
    public List<Item> GetPlayerInventory() => playerStats.Inventory;
}