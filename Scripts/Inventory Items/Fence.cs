using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fence : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName   = " fence";
        item.placeable  = true;
        item.itemCount  = 1;
        item.itemType   = 12;
        item.placeIndex = 0;
        item.iconIndex  = 5;

        item.ingredients = new int[1];
        item.ingCount = new int[1];
        item.ingredients[0] = 1;
        item.ingCount[0] = 3;
    }
}
