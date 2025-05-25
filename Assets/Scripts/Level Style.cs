using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SlinkAway", menuName = "New Style")]
public class LevelStyle : ScriptableObject
{
    public GameObject wallSide;
    public GameObject wallCorner;

    public GameObject ground;
    public GameObject obstacle;
    public GameObject wagonExit;

    public GameObject passage;
    public GameObject passenger;

    public GameObject wagonHead;
    public GameObject wagonMid;
    public GameObject wagonTail;

}

