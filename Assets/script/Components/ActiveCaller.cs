using UnityEngine;
using UnityEngine.Events;

public class ActiveCaller : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onEnableCall;
    [SerializeField]
    private UnityEvent onDisableCall;

    private void OnEnable() { onEnableCall.Invoke(); }
    private void OnDisable() { onDisableCall.Invoke(); }
}
