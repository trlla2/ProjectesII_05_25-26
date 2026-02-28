using UnityEngine;
using UnityEngine.Events;

public class Debug_OnAwake : MonoBehaviour
{
    public UnityEvent OnAwakeCall;


    private void Start()
    {
        OnAwakeCall.Invoke();
    }
}
