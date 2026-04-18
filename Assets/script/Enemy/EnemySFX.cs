using NUnit.Framework.Constraints;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemySFX : MonoBehaviour
{
    [SerializeField]
    private AudioClip hit;
    [SerializeField]
    private AudioClip fireHit;
    [SerializeField]
    private AudioClip waterHit;
    [SerializeField]
    private AudioClip electricHit;
    [SerializeField]
    private AudioClip plantHit;

    private AudioSource aS;
    private EnemyBehaviour behaviour;


    private void Awake()
    {
        behaviour = GetComponent<EnemyBehaviour>();
        aS = GetComponent<AudioSource>();
    }

    private void Start()
    {
        behaviour.OnGetDamage += PlaySFX;
    }

    private void PlaySFX(DamageType damageType)
    {
        switch (damageType.element)
        {
            case ElementType.Red:
                aS.PlayOneShot(fireHit);
                break;
            case ElementType.Blue:
                aS.PlayOneShot(waterHit);
                break;
            case ElementType.Green:
                aS.PlayOneShot(plantHit);
                break;
            case ElementType.Yellow:
                aS.PlayOneShot(electricHit);
                break;
            case ElementType.Orange:
                aS.PlayOneShot(fireHit);
                break;
            case ElementType.Melon:
                aS.PlayOneShot(plantHit);
                break;
            default:
                aS.PlayOneShot(hit);
                break;
        }

    }
}