using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldingItem : MonoBehaviour
{
    public GameObject held;
    public bool equipped;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (equipped)
        { // Instantiate it here

        }
        else
        { // Delete it

        }
    }

    public void HoldItem()
    { // Put an item in the hand
        //equipped = true;
    }
}
