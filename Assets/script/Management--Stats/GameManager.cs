using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private PlayerStats playerStats;
    public PlayerStats PlayerStats => playerStats;

    private int stageLevel = 1;
    private int _maxUnlockedStage = 1;
    [SerializeField] public int MaxStageLevel { get; private set; } = 4;
    public int StageLevel => stageLevel;
    public int MaxUnlockedStage => _maxUnlockedStage;

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
                for (int i = 0; i < GameManager.Instance.PlayerStats.SpellBookSize && i < thisDebugStats.Spells.Count; i++)
                {
                    if (thisDebugStats.Spells[i] == null)
                        continue;
                    DEBUG_Stats instanceDebugStats = GameManager.Instance.gameObject.GetComponent<DEBUG_Stats>();
                    if (instanceDebugStats != null)
                        instanceDebugStats.SetDebugSpellList(thisDebugStats.Spells);
                    GameManager.Instance.PlayerStats.AddSpellToSpellBook(thisDebugStats.Spells[i], i);
                }
            }
            Destroy(this.gameObject);
            return;
        }

        //This reads the screen's actual refresh rate and matches it automatically
        Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (stageLevel == 1)
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
        if (stageLevel > _maxUnlockedStage)
            _maxUnlockedStage = stageLevel;
    }

    public void ResetStageLevel()
    {
        stageLevel = 1;
    }

    public void SetStageLevel(int level)
    {
        stageLevel = Mathf.Clamp(level, 1, MaxStageLevel);
    }

    public void SetMaxUnlockedStage(int level)
    {
        _maxUnlockedStage = Mathf.Clamp(level, 1, MaxStageLevel);
    }

    public StageLevelData GetStageData(int level)
    {
        if (level < 0 || level >= _stageDatas.Count) return null;
        return _stageDatas[level];
    }

    /// <summary>
    /// Returns the 1-based index of the current stage among boss stages only.
    /// Stages whose StageLevelData.HasBoss is false are not counted.
    /// Returns 0 if the current stage has no boss.
    /// </summary>
    public int GetBossSceneIndex()
    {
        StageLevelData current = GetCurrentStageData();
        if (current == null || !current.HasBoss) return 0;

        int bossIndex = 0;
        for (int i = 0; i <= stageLevel && i < _stageDatas.Count; i++)
        {
            if (_stageDatas[i] != null && _stageDatas[i].HasBoss)
                bossIndex++;
        }
        return bossIndex;
    }
}