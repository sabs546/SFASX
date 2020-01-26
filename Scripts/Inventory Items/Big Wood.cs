using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigWood : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " wood";
        item.placeable = false;
        item.itemCount = 3;
        item.itemType  = 1;
        item.iconIndex = 0;
    }
}
