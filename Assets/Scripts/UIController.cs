using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    //UI vars
    public Text livesUI;
    public Text inventoryName;
    public Text inventoryDesc;
    public GameObject gameUI;
    public GameObject pauseUI;

    //Toggle Pause Menu
    public void TogglePause()
    {
        pauseUI.SetActive(!pauseUI.activeSelf);

        //    inventorySlotPauseUI[i].sprite = inventory[i].GetComponent<SpriteRenderer>().sprite;
        //    inventorySlotPauseUI[i].color = new Color(1, 1, 1, 1);
        //    inventoryName[i].text = inventory[i].GetComponent<Pickup>().pickup_name;
        //    inventoryDesc[i].text = inventory[i].GetComponent<Pickup>().pickup_description;
    }

}
