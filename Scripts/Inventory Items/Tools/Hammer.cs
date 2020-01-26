using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hammer : InventorySlot
{
    public override void SetValues(ref InventorySlot item)
    {
        item.itemName  = " hammer";
        item.placeable = false;
        item.itemCount = 25;
        item.itemType  = 15;
        item.power     = 2;
        item.iconIndex = 8;

        item.ingredients = new int[2];
        item.ingCount = new int[2];
        item.ingredients[0] = 1;
        item.ingredients[1] = 3;
        item.ingCount[0] = 2;
        item.ingCount[1] = 3;
    }
}
