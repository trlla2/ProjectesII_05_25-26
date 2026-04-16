using UnityEngine;

public class SlotMachineTile : BoardTile
{

    public override void Interact()
    {
        _boardManager.OpenSlotMachineScreen(this);
        //ReplaceWithPostInteraction();
    }
}
