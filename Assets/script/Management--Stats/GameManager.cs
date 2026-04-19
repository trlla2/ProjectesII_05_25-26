using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private PlayerStats playerStats;
    public PlayerStats PlayerStats => playerStats;

    private int stageLevel = 1;
    public int StageLevel => stageLevel;

    [SerializeField] private List<StageLevelData> _stageDatas;

    private CombatHandler cH;
    public CombatHandler CombatHandler => cH;

    private EnemyType _pendingEnemy;
    public EnemyType PendingEnemy => _pendingEnemy;

    [Header("Debug")] 
    public List<Item> _debugItemsInInventory;

    public event System.Action OnEnterBoard;
    public event System.Action OnEnterBattle;
    public event System.Action OnPlayerMove;


    
    private void Awake()
    {
        if (Instance != null)
        {
            DEBUG_Stats thisDebugStats = this.gameObject.GetComponent<DEBUG_Stats>();
            if (thisDebugStats != null && thisDebugStats.HasDebugSpells())
            {
                GameManager.Instance.PlayerStats.ClearSpellBook();
                for (int i = 0; i < GameManager.Instance.PlayerStats.SpellBookSize; i++)
                {
                    if (thisDebugStats.Spells[i] == null)
                        continue;
                    GameManager.Instance.gameObject.GetComponent<DEBUG_Stats>().SetDebugSpellList(thisDebugStats.Spells);
                    GameManager.Instance.PlayerStats.AddSpellToSpellBook(thisDebugStats.Spells[i], i);
                }
            }
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        playerStats = new PlayerStats();
    }

    public void SetCombatHandler(CombatHandler cHRef) { cH = cHRef; }
    public void EnterBoard() { OnEnterBoard?.Invoke(); }
    public void EnterBattle() { OnEnterBattle?.Invoke(); }

    public void SetPendingEnemy(EnemyType type)
    {
        _pendingEnemy = type;
    }

    public EnemyType ClearPendingEnemy()
    {
        EnemyType e = _pendingEnemy;
        _pendingEnemy = null;
        return e;
    }

    public void NotifyPlayerMove() { OnPlayerMove?.Invoke(); }

    public void Destroy()
    {
        Destroy(this.gameObject);
    }

    public StageLevelData GetCurrentStageData()
    {
        return _stageDatas[stageLevel];
    }

    public void AdvanceStageLevel()
    {
        stageLevel++;
    }

}