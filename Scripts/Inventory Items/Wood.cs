using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " wood";
        item.placeable = false;
        item.itemCount = 1;
        item.itemType  = 1;
        item.iconIndex = 0;
    }
}
