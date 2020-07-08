using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum Special { Sonic, Shooter, Blaster, Hacker, Jumper}
    public Special special_ability;
    public string name;
    public string description;
    public GameObject torchMask;

    void UseSpecial()
    {

    }


    //Toggle the TorchMask if it enters a fogMask area of a map
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "fogMask")
        {
            if (torchMask)
            {
                torchMask.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "fogMask")
        {
            if (torchMask)
            {
                torchMask.SetActive(false);
            }
        }
    }

}
