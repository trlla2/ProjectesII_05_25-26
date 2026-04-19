using UnityEngine;

public class LevelUpVFX : MonoBehaviour
{
    public ParticleSystem levelUpParticles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       GameManager.Instance.PlayerStats.OnLevelUp += PlayParticles;
    }

    void PlayParticles()
    {
        if (levelUpParticles == null) return;
        levelUpParticles.Play();
    }
}
