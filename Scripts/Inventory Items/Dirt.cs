using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dirt : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " dirt";
        item.placeable = false;
        item.itemCount = 5;
        item.itemType  = 2;
        item.iconIndex = 1;
    }
}
