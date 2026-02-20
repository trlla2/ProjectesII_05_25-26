using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    [SerializeField] float secondsTodestroy = 0.5f;

    private void Start()
    {
        Destroy(this, secondsTodestroy);
    }
}
