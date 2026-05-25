using UnityEngine;

[RequireComponent(typeof(EnemyBehaviour))]
public class EnemyVFXHandler : MonoBehaviour
{
    [SerializeField] private ParticleSystem deathParticles;
    [SerializeField] private ParticleSystem hiddenEnemyAttackSelect;

    private EnemyBehaviour behaviour;

    private void Awake()
    {
        behaviour = GetComponent<EnemyBehaviour>();
    }

    private void Start()
    {
        behaviour.OnGetDamage += OnDamageReceived;
        behaviour.OnEnemyDeath += OnDeath;
        behaviour.OnSpellAboutToExecute += OnSpellAboutToExecute;
    }

    private void OnDestroy()
    {
        if (behaviour == null) return;

        behaviour.OnGetDamage -= OnDamageReceived;
        behaviour.OnEnemyDeath -= OnDeath;
        behaviour.OnSpellAboutToExecute -= OnSpellAboutToExecute;
    }

    private void Update()
    {
        UpdateSpellVisuals();
    }

    private void OnDamageReceived(DamageType damageType)
    {
        deathParticles?.Play();
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.MediumImpact);
    }

    private void OnDeath()
    {
        deathParticles?.Play();
    }

    // The outgoing spell is passed — play VFX if it was a hidden spell retargeting
    private void OnSpellAboutToExecute(IEnemySpell outgoingSpell)
    {
        if (outgoingSpell is HiddenDestroySpell && hiddenEnemyAttackSelect != null)
            hiddenEnemyAttackSelect.Play();
    }

    private void UpdateSpellVisuals()
    {
        if (behaviour == null || behaviour.IsTimersPaused || behaviour.IsFrozen) return;

        IEnemySpell activeSpell = behaviour.ActiveSpell;
        if (activeSpell == null) return;

        if (activeSpell is TimedDestroySpellBase timedSpell)
            timedSpell.UpdateVisuals();
        else if (activeSpell is MultipleTimedSpellBase multiSpell)
            multiSpell.UpdateVisuals();
        else if (activeSpell is BlockerSpell blockerSpell)
            blockerSpell.UpdateVisuals();
        else if (activeSpell is SwapperSpell swapperSpell)
            swapperSpell.UpdateVisuals();
    }
}
