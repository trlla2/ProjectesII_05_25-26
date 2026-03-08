using UnityEngine;

public class HallOfEgressHandler : MonoBehaviour
{
    private void Start()
    {
       GameManager.Instance.PlayerStats.ClearSpellBook();
    }

   
}
