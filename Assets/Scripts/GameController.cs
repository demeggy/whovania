using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{

    //Game vars
    private bool inventoryOpen = false;

    //Player vars
    public GameObject player_current;
    public GameObject player_a;
    public GameObject player_b;
    public float speed = 3f;
    public float jump = 3f;
    private Rigidbody2D player_rb;
    private Animator player_animator;
    private bool canJump = true;
    private int jumpCounter = 0;
    private float dropDistance;
    private int playersRemaining = 1;
    private int livesRemaining = 13;

    //Inventory vars
    public List<GameObject> inventory;
    private int itemIndex;

    //Character vars
    public int rescued_players;
    public GameObject rescue_node;

    //Camera vars
    private Camera mainCamera;
    private bool isCameraMoving;

    //World vars
    public GameObject world_current;
    public GameObject world_tardis;
    public GameObject world_zygor;
    public GameObject world_skaro;
    public GameObject world_earth;
    public GameObject world_polymos;
    public GameObject world_metebelis;
    public GameObject world_gallifrey;

    //UI vars
    public Text inventoryName;
    public Text inventoryDesc;
    public Image inventorySelector;
    public List<Image> inventorySlotUI;
    public GameObject inventoryUI;

    public static GameController Instance { get; private set; }

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

        inventoryOpen = false;
        player_rb = player_current.gameObject.GetComponent<Rigidbody2D>();
        player_animator = player_current.gameObject.GetComponent<Animator>();
        mainCamera = Camera.main;


    }

    //Create a new save file
    private Save CreateSaveGameObject()
    {
        Save save = new Save();

        //save.livingTargetPositions.Add(player);

        return save;
    }

    //Save game to the created file
    public void SaveGame()
    {
        Save save = CreateSaveGameObject();

        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/gamesave.who");
        bf.Serialize(file, save);
        file.Close();

        Debug.Log("Game Saved");
    }

    //Continue a saved game
    public void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave.who"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/gamesave.who", FileMode.Open);
            Save save = (Save)bf.Deserialize(file);
            file.Close();

            //player.transform.position = save.livingTargetPositions[0];

            Debug.Log("Game Loaded");
        }
    }

    //Runtime
    private void Update()
    {
        //Player Inputs only function if the game is not paused
        if (!inventoryOpen)
        {
            Movement();
            UseAbility();
            Jump();
            PlatformDrop();
            Interact();
        }

        OpenInventory();

        //Debug Commands
        DebugShortcuts();

        //Manually switch Players if there is a companion present and they are not in the Tardis
        if (Input.GetKeyDown(KeyCode.Q) && playersRemaining > 1 && world_current != world_tardis)
        {
            SwitchPlayer();
        }

        //Camera Movement
        if (isCameraMoving)
        {
            //If the camera is not at the same pos as the current player, move it towards the new player until it's there
            if (mainCamera.transform.position != new Vector3(player_current.transform.position.x, player_current.transform.position.y, -10))
            {
                Vector3 current_playerPos = new Vector3(player_current.transform.position.x, player_current.transform.position.y, -10);
                mainCamera.transform.position = Vector3.MoveTowards(mainCamera.transform.position, current_playerPos, 1f);
            }
            else
            {
                isCameraMoving = false;
            }
        }
        else
        {
            //Follow the current player around - disabled with regional code
            //mainCamera.transform.position = new Vector3(player_current.transform.position.x, player_current.transform.position.y,-10);
        }
    }

    //Player Input -------------------------------------------------------------------------------------------------------------

    void Movement()
    {
        float dirX = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        player_rb.position = new Vector2(player_rb.position.x + dirX, player_rb.position.y);
        //player_current.transform.Translate(player_current.transform.position.x + dirX, player_current.transform.position.y, player_current.transform.position.z);

        //Flip Sprite
        if (dirX < 0)
        {
            player_current.GetComponent<SpriteRenderer>().flipX = true;
            player_animator.SetBool("isRunning", true);
        }
        else if (dirX > 0)
        {
            player_current.GetComponent<SpriteRenderer>().flipX = false;
            player_animator.SetBool("isRunning", true);
        }
        else
        {
            player_animator.SetBool("isRunning", false);
        }
    }

    void PlatformDrop()
    {
        LayerMask Platform = LayerMask.GetMask("Platform");

        if (Input.GetKeyDown(KeyCode.S))
        {
            RaycastHit2D platformDetected = Physics2D.Raycast(player_current.transform.position, Vector2.down, 0.5f, Platform);
            //Debug.DrawRay(player_current.transform.position, Vector2.down * 0.5f, Color.blue, 3);

            if (platformDetected)
            {
                player_current.transform.position = new Vector2(player_current.transform.position.x, player_current.transform.position.y - 1.5f);
            }

        }
    }

    void Jump()
    {

        LayerMask Solid = LayerMask.GetMask("Solid");

        int mask1 = 1 << LayerMask.NameToLayer("Solid");
        int mask2 = 1 << LayerMask.NameToLayer("Platform");
        int combinedMask = mask1 | mask2;

        //grounded = Physics2D.Linecast(player.position, groundCheck.position, combinedMask);

        RaycastHit2D groundDetected = Physics2D.Raycast(player_current.transform.position, Vector2.down, 0.5f, combinedMask);
        //Debug.DrawRay(player_current.transform.position, Vector2.down * 0.5f, Color.green, 0.05f);

        if (groundDetected)
        {
            canJump = true;
            jumpCounter = 0;
        }
        else
        {
            canJump = false;
        }

        if (Input.GetButtonDown("Jump") && jumpCounter < 2)
        {
            player_rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
            jumpCounter += 1;
        }
    }

    void UseAbility()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            player_animator.SetBool("isSpecial", true);
            //return the type of special ability from the playercontroller
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            player_animator.SetBool("isSpecial", false);
        }
    }

    void Interact()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            //cast a small interaction circle collider and return whatevers hit within it
            Collider2D[] colliders = Physics2D.OverlapCircleAll(player_current.transform.position, 1f);

            if (colliders.Length > 0)
            {
                foreach (Collider2D collider in colliders)
                {
                    //Pickup or activate a companion
                    if (collider.gameObject.tag == "inactivePlayer")
                    {
                        //Add the inactive player to the current game if they dont exist
                        if (playersRemaining == 1)
                        {
                            if (!player_a)
                            {
                                player_a = collider.gameObject;
                            }

                            if (!player_b)
                            {
                                player_b = collider.gameObject;
                            }
                            rescued_players += 1;
                            playersRemaining += 1;
                            Debug.Log(collider.gameObject.name + " has joined (" + playersRemaining + " players");
                        }
                        else
                        {
                            if (player_b != collider.gameObject)
                            {
                                //If you already have a different companion, they will head off to the TARDIS ready to be used later
                                Debug.Log(collider.gameObject.name + " has been added to the Tardis");
                                rescued_players += 1;
                                collider.gameObject.transform.position = new Vector2(rescue_node.transform.position.x + rescued_players, rescue_node.transform.position.y);
                            }
                        }
                    }

                    //Enter or exit the Tardis
                    if (collider.gameObject.tag == "door" && player_current == player_a)
                    {
                        //player_current.transform.position = collider.gameObject.GetComponent<Teleporter>().Target.transform.position;
                        Teleport(collider.gameObject.GetComponent<Teleporter>().Target, collider.gameObject.GetComponent<Teleporter>().door_direction.ToString());

                    }

                    //Pickup items
                    if (collider.gameObject.tag == "pickup")
                    {
                        PickupItem(collider.gameObject);
                    }

                    //Examine interactive items
                    if (collider.gameObject.tag == "interactive")
                    {
                        //return text from item description
                        Debug.Log(collider.gameObject.GetComponent<InteractiveObject>().interaction_text);
                    }
                }
            }
        }

    }

    void SwitchPlayer()
    {
        //Remove activePlayer tag from current player
        player_current.tag = "inactivePlayer";

        //Toggle the current controlling player around, ensuring the colliders are changed between trigger and solid

        //Switch to Player B (Companion)
        if (player_current == player_a)
        {
            player_current = player_b;
            player_a.GetComponent<Rigidbody2D>().gravityScale = 0;
            player_b.GetComponent<Rigidbody2D>().gravityScale = 3;
            player_a.GetComponent<BoxCollider2D>().isTrigger = true;
            player_b.GetComponent<BoxCollider2D>().isTrigger = false;
        }

        //Switch to Player A (Doctor)
        else
        {
            player_current = player_a;
            player_a.GetComponent<Rigidbody2D>().gravityScale = 3;
            player_b.GetComponent<Rigidbody2D>().gravityScale = 0;
            player_a.GetComponent<BoxCollider2D>().isTrigger = false;
            player_b.GetComponent<BoxCollider2D>().isTrigger = true;
        }

        //Move the camera to the other character
        isCameraMoving = true;

        //Force the inactive player to idle anim
        player_animator.SetBool("isRunning", false);
        player_animator.SetBool("isSpecial", false);

        //Update the player component references (potentially costly to do this constantly during runtime)
        player_rb = player_current.gameObject.GetComponent<Rigidbody2D>();
        player_animator = player_current.gameObject.GetComponent<Animator>();

        //set the new player as activePlayer tag
        player_current.tag = "activePlayer";

    }

    public void DamagePlayer()
    {
        Debug.Log("Damaging player " + playersRemaining);
        //shader flash

        //if its the secondary character (companion) injured, clear out their slot b
        if (player_current == player_b)
        {
            player_b = null;
        }

        //If there are 2 active players
        if (playersRemaining > 1)
        {
            SwitchPlayer();
            playersRemaining -= 1;
        }
        else
        {
            //Regenerate Doctor
            if (player_current != player_a)
            {
                SwitchPlayer();
            }
            livesRemaining -= 1;
        }
    }

    void Teleport(GameObject teleport_node, string direction)
    {
        //if moving into the Tardis
        if (direction == "In")
        {
            //If player_b is present, move player_b to their relevant companion node and clear the active companion slot (player_b)
            if (player_b)
            {
                player_b.transform.position = new Vector2(rescue_node.transform.position.x + rescued_players, rescue_node.transform.position.y);
                rescued_players += 1;
                player_b = null;
            }

            //Set playersRemaining to 1
            playersRemaining = 1;
        }
        else
        {
            //move active companions to tardis exterior node
            if (player_b)
            {
                player_b.transform.position = teleport_node.transform.position;
            }
        }

        //move player to the related node position
        player_current.transform.position = teleport_node.transform.position;

        //Disable current world layer
        //world_current.SetActive(false);
        //Activate tardis world layer
        //world_tardis.SetActive(true);
    }

    void Rescue()
    {
        //if player_b is null

        //player_b = gameObject;
        //rescued += 1;

        //else

        //gameObject.transform.position = new Vector2(rescuenode.transform.position.x + rescued, rescuenode.transform.position.y)
        //rescued += 1;

        //endif
    }

    void PickupItem(GameObject pickup)
    {
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i] == null)
            {
                inventory[i] = pickup;
                Debug.Log("picking " + pickup.GetComponent<Pickup>().pickup_name + " up");
                pickup.SetActive(false);

                //get pickups sprite
                Sprite pickupSprite = pickup.GetComponent<SpriteRenderer>().sprite;

                //add pickups sprite to the target slot
                inventorySlotUI[i].color = new Color(1, 1, 1, 1);
                inventorySlotUI[i].sprite = pickupSprite;
                break;
            }
        }
    }

    void OpenInventory()
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
            float menuSelect = Input.GetAxisRaw("Horizontal");

            //select right
            if (menuSelect == 1 && itemIndex < 2)
            {
                itemIndex += 1;
            }
            else if (menuSelect == -1 && itemIndex > 0)
            {
                itemIndex -= 1;
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

            //Drop Selected Item
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DropItem(itemIndex);
            }

            //Use Selected Item
            if (Input.GetKeyDown(KeyCode.E))
            {
                UseItem(itemIndex);
            }
        }
    }

    void UseItem(int itemIndex)
    {
        //unpause
        //hit for interactive objects
        Collider2D[] colliders = Physics2D.OverlapCircleAll(player_current.transform.position, 1f);

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
            inventory[itemIndex].transform.position = player_current.transform.position;
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

    // Debug Shortcut keys only ------------------------------------------
    void DebugShortcuts()
    {

        //Remove the last item from inventory
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (inventory.Count > 0)
            {
                Debug.Log(inventory.Count);
                inventory.RemoveAt(inventory.Count -1);
                inventorySlotUI[inventory.Count].color = new Color(1, 1, 1, 0);
                inventorySlotUI[inventory.Count].sprite = null;
            }
        }
    }

}
