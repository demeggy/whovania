using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{

    public enum Monster_Type { dalek, cyberman, zygon, auton }
    public Monster_Type monster_type;

    public float speed;
    public float damage;

    public Vector2 startingDirection;
    private Vector2 currentDirection;
    private Vector2 rayDirection;
    private float offset;

    public List<GameObject> checkpoints;

    private void Start()
    {
        currentDirection = startingDirection;
        rayDirection = startingDirection;

        //flip the sprite to face the right direction on spawn
        if (startingDirection == new Vector2(1,0))
        {
            offset = 0.5f;
            //GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            offset = -0.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    void Move()
    {
        LayerMask SolidLayer = LayerMask.GetMask("Solid");
        float hitDist = 0.5f;

        Vector2 groundCheck = new Vector2(transform.position.x + offset, transform.position.y - 0.5f);

        RaycastHit2D hitDown = Physics2D.Raycast(groundCheck, Vector2.down, 0.5f, SolidLayer);
        Debug.DrawRay(groundCheck, Vector2.down * 0.5f, Color.blue, 0.05f);

        RaycastHit2D hitForward = Physics2D.Raycast(transform.position, rayDirection, hitDist, SolidLayer);
        Debug.DrawRay(transform.position, rayDirection * hitDist, Color.red, 0.05f);

        //run in starting direction, if it hits a solid or theres no floor, turn back
        transform.Translate(currentDirection * speed * Time.deltaTime);

        if (hitForward)
        {
            ChangeDirection();
        }

        if (!hitDown)
        {
            ChangeDirection();
        }

    }

    void ChangeDirection()
    {
        //rotate enemy and invert the raycasting
        transform.Rotate(0, 180, 0);
        offset = offset * -1;
        rayDirection = rayDirection * -1;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("hit something");

        if (collision.gameObject.tag == "activePlayer")
        {
            Debug.Log("hit active player");
            GameController.Instance.DamagePlayer();
        }
    }

}
