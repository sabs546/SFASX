using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " grass";
        item.placeable = false;
        item.itemCount = 1;
        item.itemType  = 4;
        item.iconIndex = 3;
    }
}
