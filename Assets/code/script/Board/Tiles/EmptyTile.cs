using UnityEngine;

public class EmptyTile : BoardTile
{
    public override void Interact()
    {
        Debug.Log($"arrived to {this.GetGridPosition()}...");

        
    }
}
