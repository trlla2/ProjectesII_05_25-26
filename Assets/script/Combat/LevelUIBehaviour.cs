using UnityEngine;


public class LevelUIBehaviour : MonoBehaviour
{
    public void OnUpgradeElement(int i)
    {
        Debug.Log($"aaaaaaaa {(ElementType)i}");
       GameManager.Instance.PlayerStats.UpgradeElementalLevel((ElementType)i);
    }
}
