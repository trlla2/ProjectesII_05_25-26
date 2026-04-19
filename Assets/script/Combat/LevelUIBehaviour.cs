using UnityEngine;


public class LevelUIBehaviour : MonoBehaviour
{
    public void OnUpgradeElement(int i)
    {
       GameManager.Instance.PlayerStats.UpgradeElementalLevel((ElementType)i);
    }
}
