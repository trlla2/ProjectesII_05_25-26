using UnityEngine;

public class HapticsManager : MonoBehaviour
{
    public static HapticsManager instance { get; private set; }

    public enum HapticTypes
    {
        Selection = 0,    
        Success = 1,      
        Warning = 2,      
        Failure = 3,      
        LightImpact = 4,  
        MediumImpact = 5, 
        HeavyImpact = 6,  
        RigidImpact = 7,  
        SoftImpact = 8,
    }

    private void Awake()
    {
        if (instance != null && instance != this) 
            Destroy(this.gameObject);
        else
            instance = this;
    }


    public void GenerateHapticFeedback(HapticTypes hapticType)
    {
        Debug.Log("Generating Haptic Feedback: " + hapticType);
        Solo.MOST_IN_ONE.MOST_HapticFeedback.Generate((Solo.MOST_IN_ONE.MOST_HapticFeedback.HapticTypes)hapticType);
    }

}
