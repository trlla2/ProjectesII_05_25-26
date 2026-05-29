using UnityEngine;

// Listens to CombatHandler events and triggers cosmetic feedback only.
// Owns nothing from the gameplay state — pure presentation.
[RequireComponent(typeof(CombatHandler))]
public class PlayerCombatFeedback : MonoBehaviour
{
    [SerializeField] private float hitShakeIntensity = 0.2f;

    private CombatHandler combatHandler;
    private CameraShake cameraShake;

    private void Awake()
    {
        combatHandler = GetComponent<CombatHandler>();
    }

    private void OnEnable()
    {
        if (combatHandler == null) return;
        combatHandler.OnPlayerHitRecived += HandlePlayerHit;
    }

    private void OnDisable()
    {
        if (combatHandler == null) return;
        combatHandler.OnPlayerHitRecived -= HandlePlayerHit;
    }

    private void HandlePlayerHit()
    {
        if (HapticsManager.Instance != null)
            HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.HeavyImpact);

        if (cameraShake == null && Camera.main != null)
            cameraShake = Camera.main.GetComponent<CameraShake>();

        if (cameraShake != null)
            cameraShake.Shake(hitShakeIntensity);
    }
}
