using System;
using System.Collections.Generic;
using UnityEngine;

public class CreationManager : MonoBehaviour
{
    public GameObject backButton;
    public GameObject continueButton;

    public GameObject urlScreen;
    public GameObject rectPlacementNotice;
    public GameObject qrPlacementNotice;
    public GameObject printingScreen;

    //private InputField urlInput;

    private ContentParameters contentParameters = new ContentParameters();
    private Placement placementScript;

    private List<Action> actions;
    private int currentAction;
    
    void Start()
    {
        placementScript = GetComponent<Placement>();
        //urlInput = urlScreen.GetComponent<InputField>();
        actions = new List<Action>();
        actions.Add(StartUrlInput);
        actions.Add(StartRectPlacement);
        actions.Add(StartQrPlacement);
        actions.Add(AskForPrint);

        continueButton.SetActive(false);
        backButton.SetActive(true);
        StartUrlInput();
    }

    public void StartUrlInput()
    {
        currentAction = 0;
        placementScript.enabled = false;
        rectPlacementNotice.SetActive(false);
        qrPlacementNotice.SetActive(false);
        urlScreen.SetActive(true);
    }

    public void EndUrlInput(string url)
    {
        if (Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute))
        {
            contentParameters.URL = url;
            urlScreen.SetActive(false);
            // TODO show success pop up
            StartRectPlacement();
        }
        else
        {
            // TODO show error pop up
        }
    }

    public void StartRectPlacement()
    {
        currentAction = 1;
        continueButton.SetActive(false);
        rectPlacementNotice.SetActive(true);
        qrPlacementNotice.SetActive(false);
        placementScript.RestartPlanePlacement();
    }
    
    public void EndRectPlacement()
    {
        continueButton.SetActive(true);
        rectPlacementNotice.SetActive(false);
    }

    public void StartQrPlacement()
    {
        currentAction = 2;
        placementScript.StartQrMockPlacement();
        qrPlacementNotice.SetActive(true);
        continueButton.SetActive(true);
        printingScreen.SetActive(false);
    }

    public void AskForPrint()
    {
        currentAction = 3;
        placementScript.FinishPlacement();
        continueButton.SetActive(false);
        printingScreen.SetActive(true);
    }
    

    public void ReceiveFinalParameters()
    {
        ContentParameters finalParams = placementScript.contentParameters;
        contentParameters.Offset = finalParams.Offset;
        contentParameters.Width = finalParams.Width;
        contentParameters.Height = finalParams.Height;
        placementScript.enabled = false;
       
        // TODO show success pop up and move on to other scene
    }

    public void GoBack()
    {
        int goToAction = currentAction - 1;
        if (goToAction >= 0)
        {
            actions[goToAction].Invoke();
        }
        else
        {
            // Go to previous Scene
            // SceneManager.LoadScene(previousScene); 
        }
    }

    public void Continue()
    {
        int goToAction = currentAction + 1;
        if (goToAction < actions.Count)
        {
            actions[goToAction].Invoke();
        }
    }
}