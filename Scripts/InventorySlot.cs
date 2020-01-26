using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InventorySlot
{
    public string itemName;
    public bool   placeable;
    public int    itemCount;
    public int    itemType;
    public int    placeIndex;
    public int    iconIndex;
    public int    power;
    public int[]  ingredients;
    public int[]  ingCount;
    public Transform heldItem;
    
    public virtual void SetValues(ref InventorySlot item) {}
}
