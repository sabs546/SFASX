using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{ // Keep track of the character
    public EnvironmentTile currentTile; // Tile of the entity
    public EnvironmentTile targetTile; // Tile of the player
    public int characterState;        // If the entity is moving or not
    public int health;               // Health of the entity

    public int xID;                // mMap X position
    public int yID;               // mMap Y position
    private Character character; // Used for pathfinding
    private GameObject player;  // Used to target the player
    private Animator animator; // To animate the character
    private Environment map;  // The main map
    private float timeLimit; // Movement timers runout time
    private bool timeUp;    // Movement timer
    private int maxHP;     // Can't go over this number

    // Start is called before the first frame update
    void Start()
    {
        characterState = 0;
        maxHP = health;
        GameObject.Find("HealthDisplay").GetComponent<HPManager>().enabled = true;
        animator = GetComponent<Animator>();
        character = GetComponent<Character>();
        map = GetComponentInParent<Game>().GetComponentInChildren<Environment>();
        if (GetComponent<InventoryManager>() == null) // You have no inventory, you're not the player
            player = GameObject.FindGameObjectWithTag("Player");
        timeUp = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
        {
            animator.SetBool("die", true);
        }

        if (player != null)
        { // If it's an NPC then we need to auto-pilot it a bit
            if (timeUp)
            { // Pick a spot to go to
                List<EnvironmentTile> route = map.Solve(currentTile, targetTile);
                if (GetComponentInParent<Game>().CheckInvasion())
                { // Is the world ending? Go kill the player
                    targetTile = player.GetComponent<CharacterManager>().currentTile;
                    if (GetComponentInParent<Game>().CheckPositions(currentTile, targetTile))
                    {
                        player.GetComponent<CharacterManager>().TakeDamage(1);
                        animator.SetBool("attacking", true);
                    }
                    timeUp = false;
                }
                else // Otherwise you're cool
                {
                    route = map.Solve(currentTile, map.PickATile(currentTile));
                    animator.SetBool("attacking", false);
                    animator.SetBool("walking", true);
                }

                // Go to the spot on the map, meanwhile reset the timer
                character.GoTo(route, false);

                timeUp = false;
                timeLimit = Random.Range(5.0f, 10.0f);
                animator.SetBool("walking", true);
            }
            else
            { // Timer not up? Keep waiting
                timeLimit -= Time.deltaTime;
                if (timeLimit <= 0.0f)
                    timeUp = true;
            }
        }
        else if (currentTile == null)
            currentTile = map.playerTile;
    }

    public void TakeDamage(int damage)
    {
        if (health - damage > maxHP)
            return;

        if (health > 0) // If you're not dead
            health -= damage;
        else
            if (player != null)
            Destroy(gameObject);
    }

    public int GetMaxHP()
    {
        return maxHP;
    }
}
