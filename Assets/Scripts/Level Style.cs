using UnityEngine;

[CreateAssetMenu(fileName = "SlinkAway", menuName = "New Style")]
public class LevelStyle : ScriptableObject
{
    public GameObject ground;
    public GameObject obstacle;
    public GameObject wallSide;
    public GameObject wallCorner;
    public GameObject wagonExit;
    public GameObject stair;
    public GameObject passenger;
}
