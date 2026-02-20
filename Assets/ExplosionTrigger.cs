using UnityEngine;
using System.Collections;

public class ExplosionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionDuration = 0.5f;
    
    public void Explode()
    {
        print("KABOOOM");
        GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
    }
}
