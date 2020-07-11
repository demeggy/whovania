using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PixellateTransition : MonoBehaviour
{
    public float screenX;
    public float screenY;

    [Range(1, 10)]
    public int pixellation;

    // Start is called before the first frame update
    void Start()
    {
        pixellation = 1;
        screenX = Screen.currentResolution.width;
        screenY = Screen.currentResolution.height;
    }

    // Update is called once per frame
    void Update()
    {
        Screen.SetResolution(Screen.currentResolution.width / pixellation, Screen.currentResolution.height / pixellation, true);
    }
}

