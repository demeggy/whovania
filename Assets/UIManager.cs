using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject interactionUI;
    public Text interactionText;
    public bool interactionTextRunning;
    public float dialogueDelay;
    public string fullText;
    private string currentText = "";

    public static UIManager Instance { get; private set; }

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

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartInteraction(string msgInput)
    {
        StartCoroutine("ShowInteractionText", msgInput);
    }

    public void StopInteraction()
    {
        interactionTextRunning = false;
        interactionUI.SetActive(false);
        interactionText.text = "";
        StopCoroutine("ShowInteractionText");
    }

    IEnumerator ShowInteractionText(string output)
    {
        //Enable the Speech Bubble
        interactionTextRunning = true;
        interactionUI.SetActive(true);

        string fullText = output;

        for (int i = 0; i < fullText.Length + 1; i++)
        {
            output = fullText.Substring(0, i);
            interactionText.text = output;
            yield return new WaitForSeconds(dialogueDelay);
        }

        yield return new WaitForSeconds(2);

        //Disable the Speech Bubble
        interactionTextRunning = false;
        interactionUI.SetActive(false);
    }
}
