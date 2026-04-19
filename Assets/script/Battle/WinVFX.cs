using UnityEngine;

public class WinVFX : MonoBehaviour
{

    [SerializeField] private ParticleSystem winParticles;
    void Start()
    {
        GameManager.Instance.CombatHandler.OnEndBattle += PlayParticles;
    }

    void PlayParticles(bool isWin, int exp, int gold, Spell drop)
    {
        if (isWin && winParticles != null)
            winParticles.Play();
    }
}
