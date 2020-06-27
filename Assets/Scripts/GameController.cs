using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
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
    private int playersRemaining = 1;
    private int livesRemaining = 13;

    //Character vars
    public List<GameObject> rescued_players;
    public List<GameObject> rescue_nodes;

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
    public Text livesUI;

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
        //UI Updates
        UpdateUI();

        //Player Inputs
        Movement();
        UseAbility();
        Jump();
        Interact();

        //Manually switch Players if there is a companion present
        if (Input.GetKeyDown(KeyCode.Q) && playersRemaining > 1)
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
            //Follow the current player around
            mainCamera.transform.position = new Vector3(player_current.transform.position.x, player_current.transform.position.y,-10);
        }
    }

    //Player Input -------------------------------------------------------------------------------------------------------------

    void Movement()
    {
        float dirX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        player_rb.position = new Vector2(player_rb.position.x + dirX, player_rb.position.y);

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

    void Jump()
    {

        LayerMask Solid = LayerMask.GetMask("Solid");

        RaycastHit2D groundDetected = Physics2D.Raycast(player_current.transform.position, Vector2.down, 0.5f, Solid);
        Debug.DrawRay(player_current.transform.position, Vector2.down * 0.5f, Color.green, 3);

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
                // enemies within 1m of the player
                foreach (Collider2D collider in colliders)
                {
                    if(collider.gameObject.tag == "inactivePlayer")
                    {
                        //Add the inactive player to the current game if they dont exist
                        if (playersRemaining == 1)
                        {
                            Debug.Log(collider.gameObject.name + " has joined");
                            if(!player_a)
                            {
                                player_a = collider.gameObject;
                            }

                            if (!player_b)
                            {
                                player_b = collider.gameObject;
                            }
                            playersRemaining += 1;
                        }
                        else
                        {
                            //If you already have a companion, they will head off to the TARDIS ready to be used later
                            Debug.Log(collider.gameObject.name + " has been added to the Tardis");
                            rescued_players.Add(collider.gameObject);
                            collider.gameObject.transform.position = rescue_nodes[0].transform.position;
                        }
                    }

                    if (collider.gameObject.tag == "door")
                    {

                        ReturnToTardis();
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
        if (player_current == player_a)
        {
            player_current = player_b;
        }
        else
        {
            player_current = player_a;
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
            if(player_current != player_a)
            {
                SwitchPlayer();
            }            
            livesRemaining -= 1;
        }
    }

    void ReturnToTardis()
    {
        //Disable current world layer
        world_current.SetActive(false);
        //Activate tardis world layer
        world_tardis.SetActive(true);

        //Move player_a to Int node

        //Move player_b to their relevant companion node
        //Clear player_b slot
        //Set playersRemaining to 1
    }

        //UI Updates -------------------------------------------------------------------------------------------------------------

        void UpdateUI()
    {
        livesUI.text = livesRemaining.ToString();
    }

}
