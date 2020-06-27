using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

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

    private Camera mainCamera;
    private bool isCameraMoving;

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
        //Player Inputs
        Movement();
        UseAbility();
        Jump();
        SwitchPlayer();

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

    void SwitchPlayer()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //Toggle the current controlling player around, ensuring the colliders are changed between trigger and solid
            if(player_current == player_a)
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

        }      
    }

}
