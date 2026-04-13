using UnityEngine;

public class SlotMachineTile : BoardTile
{

    public override void Interact()
    {
        _boardManager.OpenSlotMachineScreen(this);
        ReplaceWithPostInteraction();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
