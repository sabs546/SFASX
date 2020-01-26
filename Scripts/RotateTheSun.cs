using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTheSun : MonoBehaviour
{
    public float rotateSpeed;
    Light light;

    // Start is called before the first frame update
    void Start()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(0.0f, rotateSpeed, 0.0f, Space.World);
        transform.Rotate(0.0f, rotateSpeed, 0.0f);
        light.intensity = 0.5f;
        if (GameObject.Find("Game").GetComponent<Game>().CheckInvasion())
        {
            if (light.color.g >= 0.0f)
                light.color = new Color(1.0f, light.color.g - 1.0f * Time.deltaTime, 0.0f);
            if (light.color.g < 0.01f)
                light.color = new Color(1.0f, 0.0f, 0.0f);
        }
    }
}
