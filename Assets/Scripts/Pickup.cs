using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    public enum PickupType { item, secret, points};
    public string pickup_name;
    public string pickup_description;
    public int points_value;

    private void Update()
    {

    }

}
