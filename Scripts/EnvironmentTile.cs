using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentTile : MonoBehaviour
{
    public List<EnvironmentTile> Connections { get; set; }
    public EnvironmentTile Parent { get; set; }
    public Vector3 Position { get; set; }
    public float Global { get; set; }
    public float Local { get; set; }
    public bool Visited { get; set; }
    public bool IsAccessible { get; set; }
    public int xID;
    public int yID;
    public float hardness;
    public Transform child;
    public Transform replacement;
    public GameObject occupant;
    public bool attacked;
}
