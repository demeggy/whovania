using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{

    public List<GameObject> inventory;
    private int itemIndex;
    private bool switchingItems;
    public bool inventoryOpen = false;

    public Text inventoryName;
    public Text inventoryDesc;
    public Image inventorySelector;
    public List<Image> inventorySlotUI;
    public GameObject inventoryUI;

    public static InventoryManager Instance { get; private set; }

    //Create Singleton for referencing script
    private void Awake()
    {
        //Create singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        OpenInventory();
    }

    public void OpenInventory()
    {
        //Toggle inventory menu on and off
        if (Input.GetKeyDown(KeyCode.Return))
        {
            inventoryOpen = !inventoryOpen;
            inventoryUI.SetActive(!inventoryUI.activeSelf);

            //Reset Inventory selection
            itemIndex = 0;
            inventorySelector.transform.position = inventorySlotUI[0].transform.position;
        }

        NavigateInventory();
    }

    void NavigateInventory()
    {
        //Navigate the Inventory Menu
        if (inventoryOpen)
        {
            if (switchingItems == false)
            {
                float menuSelect = Input.GetAxisRaw("Horizontal");

                if (menuSelect == 1 && itemIndex < 2)
                {
                    switchingItems = true;
                    itemIndex += 1;
                    StartCoroutine("SwitchItem");
                }
                else if (menuSelect == -1 && itemIndex > 0)
                {
                    switchingItems = true;
                    itemIndex -= 1;
                    StartCoroutine("SwitchItem");
                }

                //move the selector to the transform of the next invslot
                inventorySelector.transform.position = inventorySlotUI[itemIndex].transform.position;
                //return the name and description of index [i] from inventory
                if (inventory.Count > 0)
                {
                    if (itemIndex < inventory.Count)
                    {
                        if (inventory[itemIndex] != null)
                        {
                            inventoryName.text = inventory[itemIndex].GetComponent<Pickup>().pickup_name;
                            inventoryDesc.text = inventory[itemIndex].GetComponent<Pickup>().pickup_description;
                        }
                        else
                        {
                            inventoryName.text = "";
                            inventoryDesc.text = "";
                        }
                    }
                }
            }

            //Drop Selected Item
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DropItem(itemIndex);

            }

            //Use Selected Item
            if (Input.GetKeyDown(KeyCode.E))
            {
                UseItem(itemIndex);

                //Remove later - this will close the inventory window when item is used
                inventoryOpen = !inventoryOpen;
                inventoryUI.SetActive(!inventoryUI.activeSelf);

                //Reset Inventory selection
                itemIndex = 0;
                inventorySelector.transform.position = inventorySlotUI[0].transform.position;
            }
        }
    }

    //Delay between selecting items within inventory screen
    IEnumerator SwitchItem()
    {

        yield return new WaitForSeconds(0.5f);
        switchingItems = false;
    }

    void UseItem(int itemIndex)
    {

        //hit for interactive objects
        Collider2D[] colliders = Physics2D.OverlapCircleAll(GameObject.FindGameObjectWithTag("Player").transform.position, 1f);

        if (colliders.Length > 0)
        {
            foreach (Collider2D collider in colliders)
            {
                GameObject obj = collider.gameObject;
                Debug.Log(obj.name);

                //call UseItemOn on all interactive objects returned
                if (obj.tag == "interactive")
                {
                    //pass the current selected item into the target objects script
                    obj.GetComponent<InteractiveObject>().UseItemOn(inventory[itemIndex], itemIndex);
                }
            }
        }
    }

    public void DropItem(int itemIndex)
    {
        //move the item to position
        if (inventory[itemIndex] != null)
        {
            inventory[itemIndex].transform.position = GameObject.FindGameObjectWithTag("Player").transform.position;
            inventory[itemIndex].SetActive(true);
            //remove the item from the list
            inventory[itemIndex] = null;
            //remove from UI
            inventorySlotUI[itemIndex].color = new Color(1, 1, 1, 0);
            inventorySlotUI[itemIndex].sprite = null;
        }
    }

    public void DestroyItem(int itemIndex)
    {
        //remove the item from the list
        inventory[itemIndex] = null;
        //remove from UI
        inventorySlotUI[itemIndex].color = new Color(1, 1, 1, 0);
        inventorySlotUI[itemIndex].sprite = null;
    }

}
