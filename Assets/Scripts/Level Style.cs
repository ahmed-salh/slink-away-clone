using UnityEngine;

[CreateAssetMenu(fileName = "SlinkAway", menuName = "New Style")]
public class LevelStyle : ScriptableObject
{
    public GameObject groundPrefab;
    public GameObject obstaclePrefab;
    public GameObject wallSidePrefab;
    public GameObject wallCornerPrefab;
}
