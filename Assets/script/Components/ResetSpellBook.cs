using UnityEngine;

public class ResetSpellBook : MonoBehaviour
{
    public void ResetSpellBookSize()
    {
        GameManager.Instance.PlayerStats.ClearSpellBook();
    }
}
