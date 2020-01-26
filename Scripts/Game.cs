using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Game : MonoBehaviour
{
    [SerializeField] private Camera MainCamera;
    [SerializeField] private Character Character;
    [SerializeField] private Canvas Menu;
    [SerializeField] private Canvas Hud;
    [SerializeField] private Transform CharacterStart;
    
    private InventoryManager inventoryMgr;  // To deal with picking up and using items
    private CharacterManager characterMgr; // Decide when the character is busy or not
    private RaycastHit[] mRaycastHits;    // Pick a tile on the map
    private Environment mMap;            // Main map to interact with
    private Vector2Int tempID;          // For the timer when you break objects
    private Character mCharacter;      // The player
    private bool invasion;            // Has the dangerous part started yet?

    private readonly int NumberOfRaycastHits = 1;

    void Start()
    {
        mRaycastHits = new RaycastHit[NumberOfRaycastHits];
        mMap = GetComponentInChildren<Environment>();
        mCharacter = Instantiate(Character, transform);
        inventoryMgr = GetComponentInChildren<InventoryManager>();
        characterMgr = GetComponentInChildren<CharacterManager>();
        ShowMenu(true);
    }

    private void Update()
    {
        // Check to see if the player has clicked a tile and if they have, try to find a path to that 
        // tile. If we find a path then the character will move along it to the clicked tile. 

        if (characterMgr.health < 0 && Input.GetMouseButtonDown(0))
        {
            ShowSettings();
        }

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
        { // Don't move and stuff when the UI is being clicked
            Ray screenClick = MainCamera.ScreenPointToRay(Input.mousePosition);
            InventorySlot current = inventoryMgr.inventory[inventoryMgr.selected];
            int hits = Physics.RaycastNonAlloc(screenClick, mRaycastHits);
            if (hits > 0)
            {
                EnvironmentTile tile = mRaycastHits[0].transform.GetComponent<EnvironmentTile>();
                tempID = new Vector2Int(tile.xID, tile.yID);
                if (tile != null)
                {
                    Environment tileProperties = tile.GetComponentInParent<Environment>();
                    if (current.itemType == 4 && tile.occupant == true &&
                        characterMgr.health < characterMgr.GetMaxHP())
                    { // Eat grass for health
                        tile.occupant.GetComponent<CharacterManager>().TakeDamage(-1);
                        inventoryMgr.TakeItem(4, 1, inventoryMgr.selected);
                    }

                    if (CheckPositions(mCharacter.CurrentPosition, tile))
                    { // Is he near the tile that needs breaking
                        if (tileProperties.properties[tile.xID][tile.yID].listType != 2 && tileProperties.properties[tile.xID][tile.yID].listType != 4 &&
                            (current.itemType < 10 || current.itemType == 15 || current.placeable))
                        { // Check if the tile is actually breakable, and you're not holding a tool (Hammer doesn't count)
                            if (tileProperties.properties[tile.xID][tile.yID].listType == 1)
                            { // Tiles that need time to break start a timer
                                InventorySlot item = inventoryMgr.SearchItem(tileProperties.properties[tile.xID][tile.yID].tileType, 0);
                                if (current.itemType != 15)
                                    gameObject.AddComponent<Timer>().timeLimit = tile.hardness;
                                else
                                    gameObject.AddComponent<Timer>().timeLimit = tile.hardness / item.power;
                                mCharacter.GetComponent<Animator>().SetBool("pickup", true);
                                characterMgr.characterState = 2; // Start working
                                AudioSource audio = GameObject.FindGameObjectWithTag("Player").GetComponent<AudioSource>();
                                audio.Play();
                            }
                            else if (tileProperties.properties[tile.xID][tile.yID].listType == 3)
                            { // Otherwise they get collected instantly
                                tileProperties.SwapTile(tempID.x, tempID.y); // Pick up the tile
                                characterMgr.characterState = 0; // Set character back to idle
                            }
                        }
                        if (tileProperties.properties[tile.xID][tile.yID].listType == 4 &&
                            tileProperties.properties[tile.xID][tile.yID].tileType == 1)
                        {
                            GameObject.FindGameObjectWithTag("Player").AddComponent<HealTimer>().StartClock(1.0f, 5);
                            tileProperties.properties[tile.xID][tile.yID].listType = 3;
                        }
                    }

                    if (tileProperties.properties[tile.xID][tile.yID].listType == 0)
                    { // Just walk if it's open land
                        if (inventoryMgr.inventory[inventoryMgr.selected].placeable)
                        {
                            if (CheckPositions(mCharacter.CurrentPosition, tile))
                            {
                                tileProperties.SwapTile(tempID.x, tempID.y); // Replace the tile
                                characterMgr.characterState = 0; // Character is no longer busy
                            }
                        }
                        else if (current.itemType >= 10)
                        {
                            if (tile.occupant != null && CheckPositions(mCharacter.CurrentPosition, tile))
                            {
                                mMap.DamageOccupant(tile, 1);
                                characterMgr.characterState = 2; // Character is hitting something
                            }
                        }
                        List<EnvironmentTile> route = mMap.Solve(mCharacter.CurrentPosition, tile);
                        mCharacter.GoTo(route);
                        mCharacter.GetComponent<Animator>().SetBool("walking", true);
                    }
                    else if (tileProperties.properties[tile.xID][tile.yID].listType == 4)
                    {
                        tileProperties.SwapTile(tempID.x, tempID.y);
                        characterMgr.characterState = 0; // Character is no longer busy
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1) && !EventSystem.current.IsPointerOverGameObject(1))
        {
            inventoryMgr.UseHand();
        }
        
        if (GetComponent<Timer>() == null && characterMgr.characterState == 2)
        { // When the timer is up, break the tile
            GetComponentInChildren<Environment>().SwapTile(tempID.x, tempID.y); // Replace the tile
            mCharacter.GetComponent<Animator>().SetBool("pickup", false); // Stop the pickup animation
            characterMgr.characterState = 0; // Character is no longer busy
        }

        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        { // Scroll backwards through the inventory
            inventoryMgr.SelectSlot(-1);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") > 0)
        { // Scroll forwards through the inventory
            inventoryMgr.SelectSlot(1);
        }

        if (invasion && GetComponentInChildren<Timer>() == null)
        {
            GetComponentInChildren<Environment>().DoomsdaySpawner();
            GetComponentsInChildren<CharacterManager>();
        }

        //if (inventoryMgr.inventory[inventoryMgr.selected].itemType >= 10)
        //{ // Displayer the item in the players hand
        //    inventoryMgr.holding.HoldItem();
        //}
    }

    public void StartInvasion()
    { // Start the dangerous part
        invasion = true;
    }

    public bool CheckInvasion()
    {
        return invasion;
    }

    public void preCraft(int itemType)
    { // Allows the crafting button to be used
        InventorySlot item = inventoryMgr.SearchItem(itemType, 10); // Search for the correct item to be crafted
        item.SetValues(ref item); // Get the values for it
        inventoryMgr.CraftItem(item); // The manager will put it in your inventory
    }

    public bool CheckPositions(EnvironmentTile firstTile, EnvironmentTile otherTile)
    { // Check if you're next to the tile you're breaking
        for (int i = -1; i < 2; i++)
            for (int j = -1; j < 2; j++)
                if (firstTile.xID == otherTile.xID + i && firstTile.yID == otherTile.yID + j)
                    return true;
        return false;
    }

    public void ShowSettings()
    {
        Canvas canvas = GameObject.Find("SettingsHUD").GetComponent<Canvas>();
        canvas.enabled = !canvas.enabled;
    }

    public void ChangeSettings(int level)
    {
        Light light = GameObject.Find("Directional Light").GetComponent<Light>();
        Animator[] animator = GetComponentInChildren<Environment>().GetComponentsInChildren<Animator>();
        switch (level)
        {
            case 0: // None of the below
                light.shadows = LightShadows.None;
                for (int i = 0; i < animator.Length; ++i)
                {
                    animator[i].enabled = false;
                    animator[i].GetComponent<MeshRenderer>().enabled = false;
                }
                break;
            case 1: // Shadows
                light.shadows = LightShadows.Soft;
                for (int i = 0; i < animator.Length; ++i)
                {
                    animator[i].enabled = false;
                    animator[i].GetComponent<MeshRenderer>().enabled = false;
                }
                break;
            case 2: // Shadows + grass
                light.shadows = LightShadows.Soft;
                for (int i = 0; i < animator.Length; ++i)
                {
                    animator[i].enabled = false;
                    animator[i].GetComponent<MeshRenderer>().enabled = true;
                }
                break;
            case 3: // Shadows + grass + animations
                light.shadows = LightShadows.Soft;
                for (int i = 0; i < animator.Length; ++i)
                {
                    animator[i].enabled = true;
                    animator[i].GetComponent<MeshRenderer>().enabled = true;
                }
                break;
        }
    }

    public void ShowMenu(bool show)
    {
        AudioSource audio = GameObject.Find("HomeButton").GetComponent<AudioSource>();
        audio.Play();
        if (Menu != null && Hud != null)
        {
            Menu.enabled = show;
            Hud.enabled = !show;

            if( show )
            {
                mCharacter.transform.position = CharacterStart.position;
                mCharacter.transform.rotation = CharacterStart.rotation;
                mMap.CleanUpWorld();
            }
            else
            {
                mCharacter.transform.position = mMap.Start.Position;
                mCharacter.transform.rotation = Quaternion.identity;
                mCharacter.CurrentPosition = mMap.Start;
            }
        }
    }

    public void ShowCrafting()
    { // Enable the crafting menu
        inventoryMgr.craftingMenu = !inventoryMgr.craftingMenu;
        AudioSource audio = GameObject.Find("CraftButton").GetComponent<AudioSource>();
        audio.Play();
    }

    public bool CheckMenu()
    {
        return Menu.enabled;
    }

    public void Generate()
    {
        GameObject.Find("HealthDisplay").GetComponent<Canvas>().enabled = true;
        mMap.GenerateWorld();
    }

    public void Exit()
    {
#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
