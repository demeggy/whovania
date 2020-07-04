using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    public GameObject Target;
    public enum Direction { In, Out}
    public Direction door_direction;
}
