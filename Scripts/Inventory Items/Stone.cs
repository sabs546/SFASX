using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " stone";
        item.placeable = false;
        item.itemCount = 3;
        item.itemType  = 3;
        item.iconIndex = 2;
    }
}
