using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Empty : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " empty";
        item.placeable = false;
        item.itemCount = 0;
        item.itemType  = 0;
    }
}
