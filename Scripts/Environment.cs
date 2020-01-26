using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Environment : MonoBehaviour
{
    [SerializeField] private List<EnvironmentTile> AccessibleTiles;   // Tiles you can walk on, so basically just grass
    [SerializeField] private List<EnvironmentTile> InaccessibleTiles; // Tiles you can't walk on, but are breakable
    [SerializeField] private List<EnvironmentTile> UnbreakableTiles;  // Tiles that you can neither walk on nor break, so water
    [SerializeField] private List<GameObject> PlacedTiles;            // Tiles that can be down
    [SerializeField] private List<GameObject> NPC;                   // Everything else that moves
    [SerializeField] private List<GameObject> Tools;                  // Items you can hold in your hand
    [SerializeField] private Vector2Int Size;                         // Size of the lists
    [SerializeField] private float AccessiblePercentage;              // How many tiles will be accessible and inaccessible

    public struct Properties
    { // Note: For listType 0/1/2/3/4 = empty/accessible/inaccessible/unbreakable/placed
        public int listType; // Used to check what list the tile belongs to
        public int tileType; // Used to check where in the list the tile goes
        public Vector2 pos;  // Used to identify the tile based on position
    }
    
    public  Properties[][] properties;   // Accessing the properties of the tile
    public  EnvironmentTile playerTile;  // Accessing the tile of the player
    private InventoryManager invManager; // Accessing the inventory manager
    private CharacterManager characterMgr;
    private EnvironmentTile[][] mMap;
    private List<EnvironmentTile> mAll;
    private List<EnvironmentTile> mToBeTested;
    private List<EnvironmentTile> mLastSolution;

    private readonly Vector3 NodeSize = Vector3.one * 9.0f; 
    private const float TileSize = 10.0f;
    private const float TileHeight = 2.5f;

    public EnvironmentTile Start { get; private set; }

    public GameObject HoldItem(int type)
    {
        return Tools[type];
    }

    public void SwapTile(int x, int y)
    { // Function used when you want to break tile in the world
        invManager = GameObject.FindGameObjectWithTag("Player").GetComponent<InventoryManager>();
        characterMgr = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterManager>();
        InventorySlot currentSlot = invManager.inventory[invManager.selected];
        if (properties[x][y].listType == 1)
        { // Is it a breakable tile? If so, turn it into that
            mMap[x][y].GetComponent<AudioSource>().Play();
            Destroy(mMap[x][y].child.gameObject);
            mMap[x][y].replacement.transform.localScale += new Vector3(1.0f, 1.0f, 1.0f);
            properties[x][y].listType = 3; // 3 is collectible
        }
        else if (properties[x][y].listType == 3)
        { // Decide what kinda collectible it turns into
            InventorySlot inv = new Empty();
            if (properties[x][y].tileType < 4)
                inv = new Wood();
            else if (properties[x][y].tileType < 8)
                inv = new BigWood();
            else if (properties[x][y].tileType < 14)
                inv = new Dirt();
            else if (properties[x][y].tileType < 20)
                inv = new Stone();
            else if (properties[x][y].tileType < 21)
                inv = new Grass();

            inv.SetValues(ref inv); // necessary to add it to the list
            bool failed = false;    // Checks if it failed to add
            // Adds the item to the inventory through the manager
            invManager.AddItem(inv, ref failed);

            if (!failed)
            { // Turn it into travesable land
                Destroy(mMap[x][y].replacement.gameObject);
                properties[x][y].listType = 0;
                properties[x][y].tileType = 0;
                mMap[x][y].IsAccessible = true;
            }
        }
        else if (properties[x][y].listType == 0 && currentSlot.placeable)
        { // If you're clicking on flat land and there's nothing to break, place down whatever is in your inventory
            if (invManager.TakeItem(currentSlot.itemType, 1, invManager.selected))
            { // If you can place it down then do it, otherwise carry on
                mMap[x][y].child = PlacedTiles[currentSlot.placeIndex].transform;
                Instantiate(PlacedTiles[currentSlot.placeIndex].transform, mMap[x][y].transform);
                properties[x][y].listType = 4;
                properties[x][y].tileType = currentSlot.placeIndex;
                mMap[x][y].IsAccessible = false;
            }
        }
    }

    private void Awake()
    {
        mAll = new List<EnvironmentTile>();
        mToBeTested = new List<EnvironmentTile>();
    }

    private void OnDrawGizmos()
    {
        // Draw the environment nodes and connections if we have them
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    if (mMap[x][y].Connections != null)
                    {
                        for (int n = 0; n < mMap[x][y].Connections.Count; ++n)
                        {
                            Gizmos.color = Color.blue;
                            Gizmos.DrawLine(mMap[x][y].Position, mMap[x][y].Connections[n].Position);
                        }
                    }

                    // Use different colours to represent the state of the nodes
                    Color c = Color.white;
                    if ( !mMap[x][y].IsAccessible )
                    {
                        c = Color.red;
                    }
                    else
                    {
                        if(mLastSolution != null && mLastSolution.Contains( mMap[x][y] ))
                        {
                            c = Color.green;
                        }
                        else if (mMap[x][y].Visited)
                        {
                            c = Color.yellow;
                        }
                    }

                    Gizmos.color = c;
                    Gizmos.DrawWireCube(mMap[x][y].Position, NodeSize);
                }
            }
        }
    }

    private void Generate()
    {
        // Setup the map of the environment tiles according to the specified width and height
        // Generate tiles from the list of accessible and inaccessible prefabs using a random
        // and the specified accessible percentage
        mMap = new EnvironmentTile[Size.x][];
        properties = new Properties[Size.x][];

        int halfWidth = Size.x / 2;
        int halfHeight = Size.y / 2;
        Vector3 position = new Vector3( -(halfWidth * TileSize), 0.0f, -(halfHeight * TileSize) );
        bool start = false;

        // Initial map value setup
        for ( int x = 0; x < Size.x; ++x)
        {
            mMap[x] = new EnvironmentTile[Size.y];
            properties[x] = new Properties[Size.y];
            for ( int y = 0; y < Size.y; ++y)
            {
                if (x == Size.x / 2 && y == Size.y / 2)
                    start = true;
                bool isAccessible = start || Random.value < AccessiblePercentage;
                List<EnvironmentTile> tiles = isAccessible ? AccessibleTiles : InaccessibleTiles;
                if (tiles == AccessibleTiles)
                    properties[x][y].listType = 0;
                else
                    properties[x][y].listType = 1;

                properties[x][y].tileType = Random.Range(0, tiles.Count);

                if (properties[x][y].listType == 1 && properties[x][y].tileType > 20)
                { // Is it unbreakabable?
                    properties[x][y].tileType -= 21;
                    properties[x][y].listType = 2;
                    tiles = UnbreakableTiles;
                }
                else if (properties[x][y].listType == 1 && properties[x][y].tileType == 20)
                { // Grass tile should just be picked up, not broken down
                    properties[x][y].listType = 3;
                }

                EnvironmentTile prefab = tiles[properties[x][y].tileType];
                EnvironmentTile tile = Instantiate(prefab, position, Quaternion.identity, transform); // Instantiate it
                // Give it all its properties
                tile.Position = new Vector3( position.x + (TileSize / 2), TileHeight, position.z + (TileSize / 2)); // Give it a position
                if (properties[x][y].listType == 0)
                {
                    tile.IsAccessible = true; // Make the tile something you can move on
                    if (properties[x][y].tileType > 0)
                    {
                        Transform[] grass = tile.GetComponentsInChildren<Transform>();
                        for (int i = 1; i < grass.Length; ++i)
                            grass[i].Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
                    }
                }
                else
                {
                    tile.IsAccessible = false; // Or not
                    if (properties[x][y].listType == 1 || properties[x][y].listType == 2)
                        tile.child.Rotate(0.0f, Random.Range(0.0f, 360.0f), 0.0f, Space.World);
                }

                tile.gameObject.name = string.Format("Tile({0},{1})", x, y); // Name it

                properties[x][y].pos.x = tile.Position.x;
                properties[x][y].pos.y = tile.Position.z;

                tile.xID = x;
                tile.yID = y;
                
                mMap[x][y] = tile;
                mAll.Add(tile);

                if (start)
                {
                    playerTile = tile.GetComponent<EnvironmentTile>();
                    Start = tile;
                    mMap[x][y].occupant = GameObject.FindGameObjectWithTag("Player");
                }

                position.z += TileSize;
                start = false;
            }
            position.x += TileSize;
            position.z = -(halfHeight * TileSize);
        }

        GetComponentInParent<Game>().GetComponentInChildren<DoomsdayTimer>().enabled = true;

        //-----------------------------------------------------------------
        // NPC setup
        //-------
        for (int x = 0; x < Size.x; x++)
        { // Prepare for chickens
            for (int y = 0; y < Size.y; y++)
            {
                if (Random.value > 0.999f)
                { // It has to be really low because otherwise it gets super laggy
                    GameObject chicken; // This is all just the general creation process
                    chicken = Instantiate(NPC[0], GetComponentInParent<Game>().transform);
                    chicken.transform.position = new Vector3(mMap[x][y].Position.x, 2.0f, mMap[x][y].Position.z);
                    chicken.GetComponent<CharacterManager>().currentTile = mMap[x][y];
                    chicken.GetComponent<Character>().CurrentPosition = mMap[x][y];
                    mMap[x][y].occupant = chicken;
                    // Give it a tile too, so I can keep track of it for later
                    chicken.GetComponent<CharacterManager>().currentTile.xID = x;
                    chicken.GetComponent<CharacterManager>().currentTile.yID = y;
                }
            }
        }
    }

    public void DoomsdaySpawner()
    { // When the game goes all evil, start spawning skeletons
        Timer spawnTime = gameObject.AddComponent<Timer>();
        spawnTime.timeLimit = 10.0f;
        // Spawn them randomly
        Vector2Int ID = new Vector2Int(Random.Range(0, Size.x), Random.Range(0, Size.y));

        if (mMap[ID.x][ID.y].occupant != null && properties[ID.x][ID.y].listType != 0) // If that fails try again
            ID = new Vector2Int(Random.Range(0, Size.x), Random.Range(0, Size.y));

        // Similar to the chicken process, this is all just making a skeleton
        GameObject skeleton = Instantiate(NPC[1], GetComponentInParent<Game>().transform);
        skeleton.transform.position = new Vector3(mMap[ID.x][ID.y].Position.x, 2.0f, mMap[ID.x][ID.y].Position.z);
        skeleton.GetComponent<CharacterManager>().currentTile = mMap[ID.x][ID.y];
        skeleton.GetComponent<Character>().CurrentPosition = mMap[ID.x][ID.y];
        mMap[ID.x][ID.y].occupant = skeleton;
        // Give it a position too for tracking
        skeleton.GetComponent<CharacterManager>().currentTile.xID = ID.x;
        skeleton.GetComponent<CharacterManager>().currentTile.yID = ID.y;
    }

    public void DamageOccupant(EnvironmentTile tile, int damage)
    { // Damage whoever is on the tile, the attack system essentially
        tile.occupant.GetComponent<CharacterManager>().TakeDamage(damage);
        tile.attacked = false;
    }

    private EnvironmentTile FindATile(int xID, int yID)
    { // Just quickly search for a tile on the map if you need to manipulate it
        return mMap[xID][yID];
    }

    public EnvironmentTile PickATile(EnvironmentTile currentTile)
    { // Move to a random tile near you
        int x = Random.Range(-2, 3);
        int y = Random.Range(-2, 3);
        if (currentTile.xID + x < 0 || currentTile.xID + x >= Size.x)
            x = 0;
        if (currentTile.yID + y < 0 || currentTile.yID + y >= Size.y)
            y = 0;

        return mMap[currentTile.xID + x][currentTile.yID + y];
    }

    private void SetupConnections()
    {
        // Currently we are only setting up connections between adjacnt nodes
        for (int x = 0; x < Size.x; ++x)
        {
            for (int y = 0; y < Size.y; ++y)
            {
                EnvironmentTile tile = mMap[x][y];
                tile.Connections = new List<EnvironmentTile>();
                if (x > 0)
                {
                    tile.Connections.Add(mMap[x - 1][y]);
                }

                if (x < Size.x - 1)
                {
                    tile.Connections.Add(mMap[x + 1][y]);
                }

                if (y > 0)
                {
                    tile.Connections.Add(mMap[x][y - 1]);
                }

                if (y < Size.y - 1)
                {
                    tile.Connections.Add(mMap[x][y + 1]);
                }
            }
        }
    }

    private float Distance(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the length of the connection between these two nodes to find the distance, this 
        // is used to calculate the local goal during the search for a path to a location
        float result = float.MaxValue;
        EnvironmentTile directConnection = a.Connections.Find(c => c == b);
        if (directConnection != null)
        {
            result = TileSize;
        }
        return result;
    }

    private float Heuristic(EnvironmentTile a, EnvironmentTile b)
    {
        // Use the locations of the node to estimate how close they are by line of sight
        // experiment here with better ways of estimating the distance. This is used  to
        // calculate the global goal and work out the best order to prossess nodes in
        return Vector3.Distance(a.Position, b.Position);
    }

    public void GenerateWorld()
    {
        Generate();
        SetupConnections();
    }

    public void CleanUpWorld()
    {
        if (mMap != null)
        {
            for (int x = 0; x < Size.x; ++x)
            {
                for (int y = 0; y < Size.y; ++y)
                {
                    Destroy(mMap[x][y].gameObject);
                }
            }
        }
    }

    public List<EnvironmentTile> Solve(EnvironmentTile begin, EnvironmentTile destination)
    {
        List<EnvironmentTile> result = null;
        if (begin != null && destination != null)
        {
            // Nothing to solve if there is a direct connection between these two locations
            EnvironmentTile directConnection = begin.Connections.Find(c => c == destination);
            if (directConnection == null)
            {
                // Set all the state to its starting values
                mToBeTested.Clear();

                for( int count = 0; count < mAll.Count; ++count )
                {
                    mAll[count].Parent = null;
                    mAll[count].Global = float.MaxValue;
                    mAll[count].Local = float.MaxValue;
                    mAll[count].Visited = false;
                }

                // Setup the start node to be zero away from start and estimate distance to target
                EnvironmentTile currentNode = begin;
                currentNode.Local = 0.0f;
                currentNode.Global = Heuristic(begin, destination);

                // Maintain a list of nodes to be tested and begin with the start node, keep going
                // as long as we still have nodes to test and we haven't reached the destination
                mToBeTested.Add(currentNode);

                while (mToBeTested.Count > 0 && currentNode != destination)
                {
                    // Begin by sorting the list each time by the heuristic
                    mToBeTested.Sort((a, b) => (int)(a.Global - b.Global));

                    // Remove any tiles that have already been visited
                    mToBeTested.RemoveAll(n => n.Visited);

                    // Check that we still have locations to visit
                    if (mToBeTested.Count > 0)
                    {
                        // Mark this note visited and then process it
                        currentNode = mToBeTested[0];
                        currentNode.Visited = true;

                        // Check each neighbour, if it is accessible and hasn't already been 
                        // processed then add it to the list to be tested 
                        for (int count = 0; count < currentNode.Connections.Count; ++count)
                        {
                            EnvironmentTile neighbour = currentNode.Connections[count];

                            if (!neighbour.Visited && neighbour.IsAccessible)
                            {
                                mToBeTested.Add(neighbour);
                            }

                            // Calculate the local goal of this location from our current location and 
                            // test if it is lower than the local goal it currently holds, if so then
                            // we can update it to be owned by the current node instead 
                            float possibleLocalGoal = currentNode.Local + Distance(currentNode, neighbour);
                            if (possibleLocalGoal < neighbour.Local)
                            {
                                neighbour.Parent = currentNode;
                                neighbour.Local = possibleLocalGoal;
                                neighbour.Global = neighbour.Local + Heuristic(neighbour, destination);
                            }
                        }
                    }
                }

                // Build path if we found one, by checking if the destination was visited, if so then 
                // we have a solution, trace it back through the parents and return the reverse route
                if (destination.Visited)
                {
                    result = new List<EnvironmentTile>();
                    EnvironmentTile routeNode = destination;

                    while (routeNode.Parent != null)
                    {
                        result.Add(routeNode);
                        routeNode = routeNode.Parent;
                    }
                    result.Add(routeNode);
                    result.Reverse();

                    Debug.LogFormat("Path Found: {0} steps {1} long", result.Count, destination.Local);
                }
                else
                {
                    Debug.LogWarning("Path Not Found");
                }
            }
            else
            {
                result = new List<EnvironmentTile>();
                result.Add(begin);
                result.Add(destination);
                Debug.LogFormat("Direct Connection: {0} <-> {1} {2} long", begin, destination, TileSize);
            }
        }
        else
        {
            Debug.LogWarning("Cannot find path for invalid nodes");
        }

        mLastSolution = result;

        return result;
    }
}
