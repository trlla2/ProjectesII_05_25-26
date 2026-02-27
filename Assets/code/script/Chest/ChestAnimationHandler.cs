using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChestAnimationHandler : MonoBehaviour
{

    
    [SerializeField]
    private ParticleSystem closeParticles;
    [SerializeField]
    private ParticleSystemRenderer closeParticlesRenderer;
    [SerializeField]
    private List<Material> rarityStageMaterial = new List<Material>();
    private ItemRarity stageIndex = 0;

    public UnityEvent OnOpen;
    public UnityEvent OnRarityChange;



    public void CallOpen()
    {
        OnOpen.Invoke();

        closeParticles.Play();

    }

    public void CallRarityChange()
    {
        OnRarityChange.Invoke();
        stageIndex++;
        closeParticlesRenderer.material = rarityStageMaterial[(int)stageIndex];


        closeParticles.Play();
    }
}
