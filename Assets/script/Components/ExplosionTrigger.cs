using UnityEngine;
using System.Collections;

public class ExplosionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionDuration = 0.5f;
    
    public void Explode(Vector2 position)
    {
        Debug.Log("KABOOOM");
        GameObject explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
    }
}
