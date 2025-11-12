using UnityEngine;

public class EmptyTile : BoardTile
{
    public override void Interact()
    {
        Debug.Log($"arrived to {this.GetGridPosition()}...");
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

}
