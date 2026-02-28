using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ChestAnimationHandler : MonoBehaviour
{

    [Header("Closed Stuff")]
    [SerializeField]
    private ParticleSystem closeParticles;
    [SerializeField]
    private ParticleSystemRenderer closeParticlesRenderer;
    [SerializeField]
    private List<Material> rarityStageMaterial = new List<Material>();
    private ItemRarity stageIndex = 0;

    [Header("Open Stuff")]
    [SerializeField]
    private ParticleSystem openParticles;
    [SerializeField]
    private ParticleSystemRenderer openParticlesRenderer;

    [SerializeField]
    private GameObject commonBeam;
    [SerializeField]
    private GameObject rareBeam;
    [SerializeField]
    private GameObject epicBeam;
    [SerializeField]
    private GameObject legendBeam;

    [SerializeField]
    private GameObject itemSprite;

    [SerializeField]
    private UnityEvent OnOpen;
    [SerializeField]
    private UnityEvent OnRarityChange;

    private Item item;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void LoadItem(Item i)
    {
        item = i;

        switch (i.Rarity)
        {
            case ItemRarity.COMMON:
                animator.SetBool("Rare", false);
                animator.SetBool("Epic", false);
                animator.SetBool("Legend", false);
                break;
            case ItemRarity.RARE:
                animator.SetBool("Rare", true);
                animator.SetBool("Epic", false);
                animator.SetBool("Legend", false);
                break;
            case ItemRarity.EPIC:
                animator.SetBool("Rare", true);
                animator.SetBool("Epic", true);
                animator.SetBool("Legend", false);
                break;
            case ItemRarity.LEGENDARY:
                animator.SetBool("Rare", true);
                animator.SetBool("Epic", true);
                animator.SetBool("Legend", true);
                break;
        }


        animator.SetBool("Open", true);

    }
    public void CallOpen()
    {
        switch (item.Rarity)
        {
            case ItemRarity.COMMON:
                commonBeam.SetActive(true);
                break;
            case ItemRarity.RARE:
                rareBeam.SetActive(true);
                break;
            case ItemRarity.EPIC:
                epicBeam.SetActive(true);
                break;
            case ItemRarity.LEGENDARY:
                legendBeam.SetActive(true);
                break;
        }

        itemSprite.GetComponent<SpriteRenderer>().sprite = item.Sprite;

        itemSprite.SetActive(true);

        openParticlesRenderer.material = rarityStageMaterial[(int)item.Rarity];
        openParticles.Play(true);

    }

    public void CallRarityChange()
    {
        OnRarityChange.Invoke();
        stageIndex++;
        Debug.Log($"{rarityStageMaterial[(int)stageIndex].name} on: {stageIndex}");
        closeParticlesRenderer.material = rarityStageMaterial[(int)stageIndex];
        closeParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        closeParticles.Play(true);
    }
}
