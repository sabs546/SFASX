using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFire : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName   = " campfire";
        item.placeable  = true;
        item.itemCount  = 1;
        item.itemType   = 13;
        item.placeIndex = 1;
        item.iconIndex  = 6;

        item.ingredients = new int[2];
        item.ingCount = new int[2];
        item.ingredients[0] = 1;
        item.ingredients[1] = 3;
        item.ingCount[0] = 1;
        item.ingCount[1] = 1;
    }
}
