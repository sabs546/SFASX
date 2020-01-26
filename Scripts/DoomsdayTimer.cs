using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DoomsdayTimer : MonoBehaviour
{
    public float timeLimit;
    private float originalLimit;
    private bool timeCrossed;
    // Start is called before the first frame update
    void Start()
    {
        originalLimit = timeLimit;
        timeCrossed = false;
    }

    // Update is called once per frame
    void Update()
    {
        timeLimit -= Time.deltaTime;
        if (timeLimit <= 0.0f && !timeCrossed)
        {
            timeCrossed = true;
            GetComponentInParent<Game>().StartInvasion();
            GetComponent<AudioSource>().Play();
        }
    }

    private void OnGUI()
    {
        GetComponentInChildren<Text>().text = "Time left: " + timeLimit;
        if (timeLimit < 0.0f)
        {
            GetComponentInChildren<Text>().color = new Color(1.0f, 0.0f, 0.0f);
        }
    }
}
