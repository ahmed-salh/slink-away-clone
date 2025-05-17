using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellData
{
    public enum BaseType { Ground, Obstacle, WagonExit }

    public BaseType baseType = BaseType.Ground;

    public enum StairDirection { None, Up, Right, Down, Left }

    public StairDirection stairDirection = StairDirection.None;

    // A queue of passengers by color:
    public List<Color> passengerQueue = new List<Color>();

    // Wagons: first = head, rest = tails
    public List<Color> wagons = new List<Color>();
}
