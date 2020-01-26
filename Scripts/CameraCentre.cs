using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCentre : MonoBehaviour
{
    public GameObject player;
    public GameObject environment;
    public Environment environmentObj;
    // Start is called before the first frame update
    void Start()
    {
        environment = GameObject.FindGameObjectWithTag("Map");
        environmentObj = environment.GetComponent<Environment>();
        transform.Rotate(10.0f, 0.0f, 0.0f);
        GetComponent<Camera>().fieldOfView = 15.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");
        if (environmentObj.playerTile != null)
            transform.position = new Vector3(player.transform.position.x, 189.0f, player.transform.position.z - 190.0f);
    }
}
