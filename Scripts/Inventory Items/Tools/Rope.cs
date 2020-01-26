using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rope : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " rope";
        item.placeable = false;
        item.itemCount = 1;
        item.itemType  = 14;
        item.iconIndex = 7;

        item.ingredients = new int[1];
        item.ingCount = new int[1];
        item.ingredients[0] = 4;
        item.ingCount[0] = 2;
    }
}
