using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellData
{
    public enum BaseType { Ground, Obstacle, WagonExit }

    public BaseType baseType = BaseType.Ground;
    private enum WagonDirection { Up, Left, Down, Right }
    private enum WagonType { None, Head, Mid, Tail }


    private WagonType _hasWagon = WagonType.None;
    public enum PassageDirection { None, Up, Right, Down, Left }

    private PassageDirection _hasPassage = PassageDirection.None;

    private WagonDirection _wagonDirection = WagonDirection.Up;


    // A queue of passengers by color:
    public Queue<Passenger> passengerQueue = new Queue<Passenger>();

    // Wagons: first = head, rest = tails
    public List<Color> wagons = new List<Color>();
}

[Serializable]
public class Passenger
{
    public string Name;

    public Material Color;

    public Passenger(string name, Material color)
    {
        Name = name;
        Color = color;
    }
}