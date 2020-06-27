using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 3f;
    public float jump = 3f;
    private Rigidbody2D rb;
    private Animator animator;

    public bool wallJump = false;
    public bool doubleJump = false;
    public bool dash = false;

    private bool canJump = true;
    private int jumpCounter = 0;

    public bool isActivePlayer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Only check for the below inputs if this is the current active playing character
        if (isActivePlayer)
        {
            Movement();
            Special();
            Jump();
        }        
    }

    void Movement()
    {
        float dirX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        rb.position = new Vector2(rb.position.x + dirX, rb.position.y);

        //Flip Sprite
        if (dirX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            animator.SetBool("isRunning", true);
        }
        else if (dirX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            animator.SetBool("isRunning", true);
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    void Jump()
    {

        LayerMask Solid = LayerMask.GetMask("Solid");

        RaycastHit2D groundDetected = Physics2D.Raycast(transform.position, Vector2.down, 0.5f, Solid);
        Debug.DrawRay(transform.position, Vector2.down * 0.5f, Color.green, 3);

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
            rb.AddForce(Vector2.up * jump, ForceMode2D.Impulse);
            jumpCounter += 1;
        }
    }

    void Special()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetBool("isSonic", true);
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            animator.SetBool("isSonic", false);
        }

    }

    void SwitchPlayer()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            //Get 'other' player from GameController
            isActivePlayer = false;
            //Switch camera to follow other player
            //Set isActivePlayer on other player to true
        }
    }
}
