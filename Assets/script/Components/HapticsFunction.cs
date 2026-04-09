using UnityEngine;

public class HapticsFunction : MonoBehaviour
{
    public void GenerateHapticImpact()
    { 
        HapticsManager.Instance.GenerateHapticFeedback(HapticsManager.HapticTypes.RigidImpact);
    }
}
