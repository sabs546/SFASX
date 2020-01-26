using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spear : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " spear";
        item.placeable = false;
        item.itemCount = 1;
        item.itemType  = 11;
        item.power     = 1;
        item.iconIndex = 4;

        item.ingredients = new int[3];
        item.ingCount = new int[3];
        item.ingredients[0] = 1;
        item.ingredients[1] = 3;
        item.ingredients[2] = 14;
        item.ingCount[0] = 2;
        item.ingCount[1] = 1;
        item.ingCount[2] = 2;
    }
}
