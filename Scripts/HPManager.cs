using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPManager : MonoBehaviour
{
    private CharacterManager character; // To find the real health
    private Image[] healthBoxes;      // To get the animators
    private bool active;             // If an animation is running, don't run another
    private int current;            // Store out the healthbars health

    // Start is called before the first frame update
    void Start()
    {
        character = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterManager>();
        healthBoxes = GameObject.Find("HealthDisplay").GetComponentsInChildren<Image>();
        active = false;
        current = character.health;
    }

    // Update is called once per frame
    void Update()
    {
        if (current > character.health)
        {
            healthBoxes[character.health].GetComponent<Animator>().SetBool("Down", true);
            current = character.health;
        }
        else if (current < character.health)
        {
            healthBoxes[current].GetComponent<Animator>().SetBool("Up", true);
            current = character.health;
        }

        if (!healthBoxes[current - 1].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Down"))
        {
            healthBoxes[current - 1].GetComponent<Animator>().SetBool("Down", false);
        }
        if (!healthBoxes[current - 1].GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Up"))
        {
            healthBoxes[current - 1].GetComponent<Animator>().SetBool("Up", false);
        }
    }
}
