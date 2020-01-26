using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour
{
    public InventorySlot[] inventory; // Access to the player inventory
    public List<Material> mats;      // Icons for when things are in the inventory
    //public HoldingItem holding;   // Was going to be used when you held an item in your hand
    public Canvas canvas;          // Used to bring up the crafting menu
    public bool craftingMenu;     // Tells the crafting menu if it should be there
    public int selected;         // Which item in the inventory are you holding?
    public int size;            // Size of the inventory
    private Image[] itemSlots; // Stores all the inventory slots
    private bool empty;       // Empties the hand

    // Start is called before the first frame update
    void Start()
    {
        itemSlots = GameObject.Find("InventoryHUD").GetComponentsInChildren<Image>();
        CreateInventory(size);
        selected = 0;
        craftingMenu = false;
        canvas = GameObject.FindGameObjectWithTag("CraftMenu").GetComponent<Canvas>();
        canvas.enabled = false;
        empty = true;
        //holding = GetComponent<HoldingItem>();
    }

    public void UseHand()
    {
        empty = !empty;
    }

    public void AddItem(InventorySlot temp, ref bool failed)
    { // Put a new item in the inventory
        int slot = SlotChoice(temp.itemType);
        if (slot == -1)
        { // There are no duplicates to stack onto
            slot = FindEmpty();
            if (slot == -1)
            { // There are also no free inventory slots
                failed = true;
                Debug.Log("No space in inventory");
                return;
            }
        }

        if (inventory[slot].itemType == temp.itemType)
            inventory[slot].itemCount += temp.itemCount;
        else
            inventory[slot] = temp;

        Debug.Log(temp.itemCount + inventory[slot].itemName + " added to inventory\n" + inventory[slot].itemCount + " in inventory");
    }

    public void SelectSlot(int next)
    { // Expand the slot that is selected in the inventory
        if (empty)
            return;
        if (selected + next < 0 || selected + next > 7)
            return;
        else
        { // If you aren't at the end, choose the previous/next item
            if (inventory[selected + next].itemType > 0)
                selected += next;
            else // If the next item is further than just 1 slot away, make this function recursive
                SelectSlot(next + (next ^ 0));
        }
    }

    public bool TakeItem(int type, int count, int index = -1)
    { // Remove an item from the inventory, or multiple of the same one
        if (index != -1)
            selected = index;
        else if (inventory[selected].itemType != type)
            return false;

        if (inventory[selected].itemCount - count > -1)
        { // As long as resources are sufficient, remove the items from the inventory
            inventory[selected].itemCount -= count;
            if (inventory[selected].itemCount == 0)
            { // Make the slot empty
                inventory[selected] = new Empty();
                inventory[selected].SetValues(ref inventory[selected]);
            }
            return true;
        }
        return false;
    }

    public void CraftItem(InventorySlot item)
    { // Crafting a new item
        int[] ingredSlots = new int[item.ingredients.Length];
        for (int j = 0; j < item.ingredients.Length; j++)
        { // Checking if you have all the ingredients
            int tempSlot = FindItem(item.ingredients[j]);
            if (tempSlot == -1 || inventory[tempSlot].itemCount < item.ingCount[j])
            { // You don't have the ingredients, git out
                return;
            }
            else // You're fine, continue
                ingredSlots[j] = tempSlot;
        }

        int slot = FindItem(item.itemType); // Check if there's a duplicate item to stack onto

        if (slot == -1)
        { // There are no duplicates
            for (int j = 0; j < item.ingredients.Length; j++)
            { // Gather all the ingredients and see if there is space for a new new slot
                if (inventory[ingredSlots[j]].itemCount - item.ingCount[j] == 0)
                { // If after the ingredients are used, a new slot opens up, set it as the new slot
                    slot = FindEmpty();
                }
            }
            if (FindEmpty() == -1)
                return; // If you still don't have any free slots at this point then stop crafting
        }

        for (int j = 0; j < item.ingredients.Length; j++)
        { // Now you can actually remove all the items from your inventory
            AudioSource audio = GameObject.Find("CraftHUD").GetComponent<AudioSource>();
            audio.Play();
            // Take the items out of your inventory first
            TakeItem(inventory[ingredSlots[j]].itemType, item.ingCount[j], ingredSlots[j]);
        }
        
        bool failed = false; // Doesn't matter here, but necessary for the function
        AddItem(item, ref failed); // Add the crafted item to your inventory
    }

    public InventorySlot SearchItem(int type, int itemClass)
    { // Return an item, as well as all its properties
        InventorySlot item = new Empty();
        if (itemClass == 0)
        { // Is it a resource?
            switch (type)
            {
                case 0: // Wood
                    item = new Wood();
                    break;
                case 1: // Dirt
                    item = new Dirt();
                    break;
                case 2: // Stone
                    item = new Stone();
                    break;
                case 3: // Grass
                    item = new Grass();
                    break;
            }
        }
        else
        { // Is it a tool?
            switch (type)
            {
                case 11: // Spear
                    item = new Spear();
                    break;
                case 12: // Fence
                    item = new Fence();
                    break;
                case 13: // CampFire
                    item = new CampFire();
                    break;
                case 14: // Rope
                    item = new Rope();
                    break;
                case 15: // Hammer
                    item = new Hammer();
                    break;
            }
        }
        return item;
    }

    private int FindItem(int type)
    { // See if you currently own a chosen item
        for (int i = 0; i < size; i++)
            if (inventory[i].itemType == type)
                return i;
        return -1;
    }

    private int FindEmpty()
    { // Look for a slot with the exact item type
        for (int i = 0; i < size; i++)
            if (inventory[i].itemType == 0)
                return i;
        return -1;
    }

    private int SlotChoice(int type)
    { // Look for an empty slot, or somewhere to store a similar item
        for (int i = 0; i < size; i++)
            if (inventory[i].itemType == type)
                return i;
        return -1;
    }

    private void CreateInventory(int size)
    { // Make a new inventory for the game start
        inventory = new InventorySlot[size];
        for (int i = 0; i < size; i++)
            inventory[i] = new Empty();
    }

    private void OnGUI()
    {
        if (GetComponentInParent<Game>().GetComponentInChildren<EnvironmentTile>() != null)
        {
            //GUI.Box(new Rect(50.0f, Screen.height - 100.0f, Screen.width - 100.0f, 100.0f), "Inventory");
            for (int i = 0; i < size; i++)
            {
                Image itemSlot = itemSlots[i].GetComponentInChildren<Image>();
                if (inventory[i].itemCount != 0)
                { // Only show inventory slots with things in them
                    itemSlot.enabled = true;
                    if (i != selected | empty)
                    {
                        itemSlot.rectTransform.localPosition = new Vector3(itemSlot.transform.localPosition.x, 0.0f, itemSlot.transform.localPosition.z);
                        itemSlot.material = mats[inventory[i].iconIndex];
                        itemSlot.GetComponentInChildren<Text>().text = inventory[i].itemCount + "" + inventory[i].itemName;
                    }
                    else
                    {
                        itemSlot.rectTransform.localPosition = new Vector3(itemSlot.transform.localPosition.x, 10.0f, itemSlot.transform.localPosition.z);
                        itemSlot.material = mats[inventory[i].iconIndex];
                        itemSlot.GetComponentInChildren<Text>().text = inventory[i].itemCount + "" + inventory[i].itemName;
                    }
                }
                else
                {
                    itemSlot.material = null;
                    itemSlot.GetComponentInChildren<Text>().text = "";
                    itemSlot.enabled = false;
                }
            }
            if (craftingMenu)
            { // Draw the crafting menu
                canvas.enabled = true;
                Button[] button = canvas.GetComponentsInChildren<Button>();
                InventorySlot temp;
                for (int i = 0; i < 5; i++)
                { // Loop through all craftable items
                    temp = SearchItem(i + 11, 10);
                    temp.SetValues(ref temp);
                    InventorySlot subTemp; // Since these are craftables, you'll need to store ingredients too
                    Text[] textFields = button[i].GetComponentsInChildren<Text>();
                    textFields[1].text = "";
                    for (int j = 0; j < temp.ingredients.Length; j++)
                    {
                        if (temp.ingredients[j] > 10) // Is the craftable a craftable?
                            subTemp = SearchItem(temp.ingredients[j], 10);
                        else // Otherwise just carry on as normal
                            subTemp = SearchItem(temp.ingredients[j] - 1, 0);
                        subTemp.SetValues(ref subTemp);
                        //GUI.TextField(new Rect(105 + i * 110.0f, j * 15 + 204.0f, 100.0f, 100.0f), temp.ingCount[j] + "" + subTemp.itemName, GUIStyle.none);
                        textFields[1].text += temp.ingCount[j] + "" + subTemp.itemName + "\n";
                    }
                }
            }
            else
                canvas.enabled = false;
        }
    }
}
