using UnityEngine;

public class DestroyOnTiime : MonoBehaviour
{
    public float time = 1.0f;
    private void Start()
    {
        Destroy(gameObject, time);
    }
}
