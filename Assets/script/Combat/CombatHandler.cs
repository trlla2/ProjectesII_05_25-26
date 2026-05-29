using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    private EnemyBehaviour enemyInCombat = null;

    private SpellHoverHandler spellHoverHandler;

    //Boss
    public BossManager bossManager;
    [HideInInspector] public bool isBossBattle = false;
    [HideInInspector] public ElementType BossWeakElement;

    [Header("Debug")]
    [SerializeField] private bool isEnemyTypeForced = false;
    [SerializeField] private EnemyType forcedEnemyType;

    // Events
    public delegate void GetOnEndBattle(bool isWin, int exp, int gold, Spell drop);
    public event GetOnEndBattle OnEndBattle;

    public event System.Action OnPlayerHitRecived;
    public event System.Action OnPlayerDead;
    public event System.Action OnPlayerHealed;
    public event System.Action OnPlayerHitEvaded;

    public event System.Action<EnemyBehaviour> OnSetEnemyReference;

    public void Awake()
    {
        GameManager.Instance.SetCombatHandler(this);
    }

    private void Start()
    {
        if (bossManager != null)
        {
            isBossBattle = true;
            BossWeakElement = bossManager.WeakElement;
        }
    }

    public void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.CombatHandler == this)
            GameManager.Instance.SetCombatHandler(null);
    }

    public void SetEnemyRef(EnemyBehaviour enemyRef)
    {
        enemyInCombat = enemyRef;

        EnemyType typeToUse = ResolveEnemyType();
        if (typeToUse == null)
        {
            Debug.LogError("CombatHandler: no se pudo resolver un EnemyType para este combate.");
            return;
        }

        enemyInCombat.SetEnemyType(typeToUse);
        OnSetEnemyReference?.Invoke(enemyRef);
        enemyInCombat.StartBattle();
    }

    private EnemyType ResolveEnemyType()
    {
        if (isEnemyTypeForced && forcedEnemyType != null)
            return forcedEnemyType;

        return GameManager.Instance.ClearPendingEnemy();
    }

    public void DeleteEnemyRef() { enemyInCombat = null; }

    public void AttackBoss(float percent) { enemyInCombat?.dealBossDamage(percent); }

    public void AttackEnemy(ElementType element) { enemyInCombat?.DealDamage(GameManager.Instance.PlayerStats.AttackWithElement(element)); }

    public void AttackPlayer(float damage)
    {
        if (GameManager.Instance.PlayerStats.EvadeAttack())
        {
            OnPlayerHitEvaded?.Invoke();
            return;
        }

        if (GameManager.Instance.PlayerStats.SpikeDamage > 0)
            enemyInCombat?.DealDamage(new DamageType(ElementType.EMPTY, GameManager.Instance.PlayerStats.SpikeDamage));

        GameManager.Instance.PlayerStats.Damage(damage);

        OnPlayerHitRecived?.Invoke();

        if (!GameManager.Instance.PlayerStats.IsAlive())
            PlayerDead();
    }

    public void HealPlayer(int amount)
    {
        GameManager.Instance.PlayerStats.Heal(amount);
        OnPlayerHealed?.Invoke();
    }

    public void PlayerDead()
    {
        enemyInCombat?.StopBattle();
        OnPlayerDead?.Invoke();
        OnEndBattle?.Invoke(false, 0, 0, null);
    }

    public void OnEnemyDead()
    {
        int exp = enemyInCombat.GetEnemyType().ExpReward;

        Spell drop = enemyInCombat.GetEnemyType().GetSpellDrop();

        GameManager.Instance.PlayerStats.AddExp(exp);
        if(drop != null)
            GameManager.Instance.PlayerStats.AddKnownSpell(drop);

        int gold = enemyInCombat.GetEnemyType().GoldReward;
        GameManager.Instance.PlayerStats.AddCurrency(CurrencyType.GOLD, gold);

        DeleteEnemyRef();
        //_boardClickHandler.HandleBoardRaycast(true);
        OnEndBattle?.Invoke(true, exp, gold, drop);
    }

    public void SetHoverHandlerRef(SpellHoverHandler spellHover) { spellHoverHandler = spellHover; }
    public void DeleteHoverHandlerRef() { spellHoverHandler = null; }
    public void HoverSpell(Spell spell)
    {
        spellHoverHandler.SetupSpellUI(spell);
        spellHoverHandler.Show();
    }
    public void UnHoverSpell() { spellHoverHandler.UnShow(); }

    public EnemyBehaviour GetEnemyRef() => enemyInCombat;
}
