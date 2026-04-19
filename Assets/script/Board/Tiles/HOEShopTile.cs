using UnityEngine;
using UnityEngine.Events;

public class HOEShopTile : BoardTile
{
    public UnityEvent OnInteract;

    public override void Interact()
    {
        OnInteract.Invoke();
    }

}
