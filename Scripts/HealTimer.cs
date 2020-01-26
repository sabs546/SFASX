using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealTimer : MonoBehaviour
{
    private float timeLimit;
    private float timeMax;
    public int repeatCount;
    CharacterManager characterMgr;
    // Start is called before the first frame update
    void Start()
    {
        characterMgr = GetComponent<CharacterManager>();
    }

    // Update is called once per frame
    void Update()
    {
        timeLimit -= Time.deltaTime;
        if (timeLimit <= 0)
        {
            repeatCount--;
            if (characterMgr.health < characterMgr.GetMaxHP())
                characterMgr.health += 1;
            else
                Destroy(GetComponent<HealTimer>());

            timeLimit = timeMax;
            if (repeatCount == 0)
            {
                Destroy(GetComponent<HealTimer>());
            }
        }
    }

    public void StartClock(float limit, int count)
    {
        timeLimit = limit;
        repeatCount = count;
        timeMax = timeLimit;
    }
}
