using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    public GameObject currentPlayer;
    public GameObject playerA;
    public GameObject playerB;
    public GameObject companionNode;
    public GameObject torchMask;

    private bool canJump;
    private int jumpCounter;
    public float speed = 3f;
    public float jump = 3f;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!InventoryManager.Instance.inventoryOpen)
        {
            Movement();
            //UseAbility();
            Jump();
            PlatformDrop();
            Interact();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                SwitchCharacters();
            }
        }        
    }

    void Movement()
    {
        float dirX = Input.GetAxisRaw("Horizontal") * speed * Time.deltaTime;
        transform.position = new Vector2(transform.position.x + dirX, transform.position.y);
        //player_current.transform.Translate(player_current.transform.position.x + dirX, player_current.transform.position.y, player_current.transform.position.z);

        //Flip Sprite
        if (dirX < 0)
        {
            //player_current.GetComponent<SpriteRenderer>().flipX = true;
            transform.rotation = Quaternion.Euler(0, 180, 0);
            playerA.GetComponent<Animator>().SetBool("isRunning", true);

            //if player b is present, animate them
            if (playerB)
            {
                playerB.GetComponent<Animator>().SetBool("isRunning", true);
            }

            UIManager.Instance.StopInteraction();
        }
        else if (dirX > 0)
        {
            //player_current.GetComponent<SpriteRenderer>().flipX = false;
            transform.rotation = Quaternion.Euler(0, 0, 0);
            playerA.GetComponent<Animator>().SetBool("isRunning", true);

            //if player b is present, animate them
            if (playerB)
            {
                playerB.GetComponent<Animator>().SetBool("isRunning", true);
            }

            UIManager.Instance.StopInteraction();
        }
        else
        {
            playerA.GetComponent<Animator>().SetBool("isRunning", false);

            //if player b is present, animate them
            if (playerB)
            {
                playerB.GetComponent<Animator>().GetComponent<Animator>().SetBool("isRunning", false);
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

        RaycastHit2D groundDetected = Physics2D.Raycast(transform.position, Vector2.down, 0.75f, combinedMask);
        Debug.DrawRay(transform.position, Vector2.down * 0.75f, Color.green, 0.05f);

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
            GetComponent<Rigidbody2D>().AddForce(Vector2.up * jump, ForceMode2D.Impulse);
            jumpCounter += 1;
            UIManager.Instance.StopInteraction();
        }
    }

    void PlatformDrop()
    {
        LayerMask Platform = LayerMask.GetMask("Platform");

        if (Input.GetKeyDown(KeyCode.S))
        {
            RaycastHit2D platformDetected = Physics2D.Raycast(transform.position, Vector2.down, 0.75f, Platform);
            Debug.DrawRay(transform.position, Vector2.down * 0.75f, Color.blue, 3);

            if (platformDetected)
            {
                StartCoroutine("DropFromPlatform");
            }

        }
    }

    IEnumerator DropFromPlatform()
    {
        GetComponent<Collider2D>().isTrigger = true;
        yield return new WaitForSeconds(0.35f);
        GetComponent<Collider2D>().isTrigger = false;
    }

    void Interact()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            //cast a small interaction circle collider and return whatevers hit within it
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 1f);

            if (colliders.Length > 0)
            {
                foreach (Collider2D collider in colliders)
                {
                    GameObject obj = collider.gameObject;

                    //Pickup or activate a companion
                    if (collider.gameObject.tag == "inactivePlayer")
                    {
                        //if there is no companion, add the target to the playerB slot, and position/rotate the object at the companion Node behind the Doctor
                        if (!playerB)
                        {

                            //add the companion to the companion slots if they've never been picked up before
                            if (GameController.Instance.companionSlots.IndexOf(obj) < 0)
                            {
                                GameController.Instance.companionSlots.Add(obj);
                            }

                            UIManager.Instance.StartInteraction(obj.GetComponent<CharacterInfo>().charInfo + "\n" + obj.GetComponent<CharacterInfo>().charName + " has joined!");
                            playerB = obj;
                            playerB.transform.parent = transform;
                            playerB.transform.position = companionNode.transform.position;
                            playerB.transform.rotation = transform.rotation;
                            playerB.tag = "activePlayer";
                        }
                        else
                        {
                            //send this object to the Tardis
                        }
                    }

                    //Enter or exit the Tardis
                    if (collider.gameObject.tag == "door" && currentPlayer == playerA)
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
                        if (UIManager.Instance.interactionTextRunning == false)
                        {
                            //if the item has not been activated before, run the pre-activation text
                            if (!collider.gameObject.GetComponent<InteractiveObject>().isActivated)
                            {
                                //start this by calling the UIManager
                                UIManager.Instance.StartInteraction(collider.gameObject.GetComponent<InteractiveObject>().text_a);
                            }
                            else
                            {
                                UIManager.Instance.StartInteraction(collider.gameObject.GetComponent<InteractiveObject>().text_b);
                            }

                        }
                    }
                }
            }
        }

    }

    void PickupItem(GameObject pickup)
    {
        for (int i = 0; i < InventoryManager.Instance.inventory.Count; i++)
        {
            if (InventoryManager.Instance.inventory[i] == null)
            {
                InventoryManager.Instance.inventory[i] = pickup;
                Debug.Log("picking " + pickup.GetComponent<Pickup>().pickup_name + " up");
                pickup.SetActive(false);

                //get pickups sprite
                Sprite pickupSprite = pickup.GetComponent<SpriteRenderer>().sprite;

                //add pickups sprite to the target slot
                InventoryManager.Instance.inventorySlotUI[i].color = new Color(1, 1, 1, 1);
                InventoryManager.Instance.inventorySlotUI[i].sprite = pickupSprite;
                break;
            }
        }
    }

    void AddCompanion(GameObject companion)
    {
        if (!playerB)
        {
            playerB = companion;
            playerB.transform.position = new Vector2(playerA.transform.position.x - 0.25f, playerA.transform.position.y);
            playerA.GetComponent<SpriteRenderer>().sortingOrder = 10;
            playerB.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }
        else
        {
            //send companion to Tardis
        }
    }

    void SwitchCharacters()
    {
        //Switch characters around

        //toggle position
        Vector2 aPos = playerA.transform.position;
        Vector2 bPos = playerB.transform.position;

        playerA.transform.position = bPos;
        playerB.transform.position = aPos;

        //toggle sort order
        if(playerA.GetComponent<SpriteRenderer>().sortingOrder == 1)
        {
            playerA.GetComponent<SpriteRenderer>().sortingOrder = 10;
        }
        else
        {
            playerA.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }

        if (playerB.GetComponent<SpriteRenderer>().sortingOrder == 1)
        {
            playerB.GetComponent<SpriteRenderer>().sortingOrder = 10;
        }
        else
        {
            playerB.GetComponent<SpriteRenderer>().sortingOrder = 1;
        }

        //toggle currentPlayer
        if(currentPlayer == playerA)
        {
            currentPlayer = playerB;
        }
        else
        {
            currentPlayer = playerA;
        }
    }

    void UseAbility()
    {

    }

    public void Teleport(GameObject teleport_node, string direction)
    {
        //if moving into the Tardis
        if (direction == "In")
        {
            //If player_b is present, move player_b to their relevant companion node and clear the active companion slot (player_b)
            //if (player_b)
            //{
            //    player_b.transform.position = new Vector2(rescue_node.transform.position.x + rescued_players, rescue_node.transform.position.y);
            //    rescued_players += 1;
            //    player_b = null;
            //}

            ////Set playersRemaining to 1
            //playersRemaining = 1;
        }
        else
        {
            //move active companions to tardis exterior node
            //if (player_b)
            //{
            //    player_b.transform.position = teleport_node.transform.position;
            //}
        }

        //move player to the related node position
        transform.position = teleport_node.transform.position;

        //if companion is present
        //move playerB gameObject to Tardis Interior
        //set parent of playerB to null
        //set playerB to null

        if (playerB)
        {
            RemoveCompanion();
        }

    }

    void RemoveCompanion()
    {
        //Get the companions slots node position from the SlotPos list, and send them to that Vector
        int i = GameController.Instance.companionSlots.IndexOf(playerB);
        Debug.Log(i);
        Vector2 sendToPos = GameController.Instance.companionSlotPos[i].transform.position;
        playerB.transform.position = sendToPos;

        playerB.tag = "inactivePlayer";
        playerB.transform.parent = null;
        playerB = null;
    }

    //Toggle the TorchMask if it enters a fogMask area of a map
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "fogMask")
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
