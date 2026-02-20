using UnityEngine;

public class CombatHandler : MonoBehaviour
{
    private EnemyBehaviour enemyInCombat;

    private SpellHoverHandler spellHoverHandler;

    private int expMultiplyer = 4;
    private int moneyMultiplyer = 4;

    //Boss
    public BossManager bossManager;
    [HideInInspector] public bool isBossBattle = false;
    [HideInInspector] public ElementType BossWeakElement;

    [Header("Debug")]
    [SerializeField] private bool isEnemyTypeForced = false;
    [SerializeField] private EnemyType forcedEnemyType;

    // Events
    public delegate void GetOnEndBattle(bool isWin, int exp, int money);
    public event GetOnEndBattle OnEndBattle;

    public event System.Action OnPlayerHitRecived;
    public event System.Action OnPlayerHealed;
    public event System.Action OnPlayerHitEvaded;

    public event System.Action<EnemyBehaviour> OnSetEnemyReference;

    public void Awake()
    {
        G.GM.SetCombatHandler(this);
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
        if (G.GM != null && G.GM.CombatHandler == this)
            G.GM.SetCombatHandler(null);
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
        enemyInCombat.StartBattle();
        OnSetEnemyReference?.Invoke(enemyRef);
    }

    private EnemyType ResolveEnemyType()
    {
        if (isEnemyTypeForced && forcedEnemyType != null)
            return forcedEnemyType;

        return G.GM.ClearPendingEnemy();
    }

    public void DeleteEnemyRef() { enemyInCombat = null; }

    public void AttackBoss(float percent) { enemyInCombat?.dealBossDamage(percent); }

    public void AttackEnemy(ElementType element) { enemyInCombat?.DealDamage(G.GM.PlayerStats.AttackWithElement(element)); }

    public void AttackPlayer(float damage)
    {
        if (G.GM.PlayerStats.EvadeAttack())
        {
            OnPlayerHitEvaded?.Invoke();
            return;
        }

        if (G.GM.PlayerStats.SpikeDamage > 0)
            enemyInCombat?.DealDamage(new DamageType(ElementType.EMPTY, G.GM.PlayerStats.SpikeDamage));

        G.GM.PlayerStats.Damage(damage);
        Camera.main.GetComponent<CameraShake>().Shake(0.2f);

        OnPlayerHitRecived?.Invoke();

        if (!G.GM.PlayerStats.IsAlive())
            OnPlayerDead();
    }

    public void HealPlayer(int amount)
    {
        G.GM.PlayerStats.Heal(amount);
        OnPlayerHealed?.Invoke();
    }

    public void OnPlayerDead()
    {
        enemyInCombat?.StopBattle();
        OnEndBattle?.Invoke(false, 0, 0);
    }

    public void OnEnemyDead()
    {
        int exp = G.GM.StageLevel * expMultiplyer;
        int money = G.GM.StageLevel * moneyMultiplyer;

        G.GM.PlayerStats.AddExp(exp);
        G.GM.PlayerStats.AddMoney(money);

        DeleteEnemyRef();
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

    public EnemyBehaviour GetEnemyRef() => enemyInCombat;
}
