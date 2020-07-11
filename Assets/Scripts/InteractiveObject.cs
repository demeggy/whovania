using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    public enum InteractType { toggle, activate, destroyer }
    public InteractType interaction_type;

    private Material defaultMaterial;
    public Material outlineSprite;
    public float detection_range = 1f;

    public string text_a;
    public string text_b;
    public bool isActivated;
    [Tooltip("Which object allows you to activate this interactor?")]
    public GameObject activator_key;
    [Tooltip("Which objects are affected when this interactor is successfully activated?")]
    public List<GameObject> activator_targets;

    private void Start()
    {
        defaultMaterial = GetComponent<SpriteRenderer>().material;
    }

    private void Update()
    {
        ToggleShader();
    }

    private void ToggleShader()
    {
        //Toggle Interaction Shader
        float dist = Vector2.Distance(transform.position, GameObject.FindGameObjectWithTag("Player").transform.position);

        if (dist < detection_range)
        {
            GetComponent<SpriteRenderer>().material = outlineSprite;
        }
        else
        {
            GetComponent<SpriteRenderer>().material = defaultMaterial;
        }
    }

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
            isActivated = true;
            InventoryManager.Instance.DestroyItem(itemIndex);
        }
        else
        {
            //output saying nothing happened
            Debug.Log("incorrect key used");
        }
        
    }

}
