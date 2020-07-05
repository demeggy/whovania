using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_Earth : MonoBehaviour
{
    public GameObject eye;
    public GameObject arms;
    private Transform player;
    public int lives = 4;

    // Start is called before the first frame update
    void Start()
    {
        player = GameController.Instance.player_current.transform;
    }

    // Update is called once per frame
    void Update()
    {
        //The Nestene eye must follow the player
        FollowEye();
        SpinArms();

        arms.transform.Rotate(0, 0, 5 * Time.deltaTime);
    }

    void FollowEye()
    {
        var dir = player.position - eye.transform.position;
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Debug.Log(angle);
    }

    void SpinArms()
    {

        if (player.position.y > transform.position.y)
        {
            //clockwise
            arms.transform.Rotate(0, 0, 1);
        }
        else
        {
            //counter-clockwise
            arms.transform.Rotate(0, 0, -1);
        }
    }
}
