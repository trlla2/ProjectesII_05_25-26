using UnityEngine;
using UnityEngine.Events;

public class ActiveCaller : MonoBehaviour
{
    [SerializeField]
    private UnityEvent onAwakeCall;
    [SerializeField]
    private UnityEvent onStartCall;
    [SerializeField]
    private UnityEvent onEnableCall;
    [SerializeField]
    private UnityEvent onDisableCall;
    [SerializeField]
    private UnityEvent onDestroyCall;

    private void Awake() { onAwakeCall.Invoke(); }
    private void Start() { onStartCall.Invoke(); }
    private void OnEnable() { onEnableCall.Invoke(); }
    private void OnDisable() { onDisableCall.Invoke(); }
    private void OnDestroy() { onDestroyCall.Invoke(); }
}
