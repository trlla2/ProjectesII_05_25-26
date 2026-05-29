using UnityEngine;
using System.Collections;

public class ExplosionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioSource pipeBombSFX;

    public void Explode(Vector2 position)
    {
        Debug.Log("KABOOOM");
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.transform.Rotate(100,0,0); //it looks better
        pipeBombSFX.Play();

    }
}
