using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum InteractType { toggle, activate, destroyer }
    public InteractType interaction_type;

    public string interaction_text;
    public GameObject activator_key;
    public List<GameObject> activator_targets;

    public void Activate()
    {
        foreach (GameObject target in activator_targets)
        {
            switch (interaction_type)
            {
                //toggle the target on/off
                case InteractType.toggle:
                    target.SetActive(!target.activeSelf);
                    break;

                //activate it otherwise
                case InteractType.activate:
                    break;

                //destroy the targets
                case InteractType.destroyer:
                    Destroy(target);
                    break;
                default:
                    break;
            }
        }
    }

    public void UseItemOn(GameObject itemUsed, int itemIndex)
    {
        //destroy activator_target on action if item used is the key
        if(itemUsed == activator_key)
        {
            Activate(); 
            //remove the item from inventory
            GameController.Instance.DestroyItem(itemIndex);
            Debug.Log("correct key used");
        }
        else
        {
            //output saying nothing happened
            Debug.Log("incorrect key used");
        }
        
    }

}
