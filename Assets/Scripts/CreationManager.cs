using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreationManager : MonoBehaviour
{
    public GameObject backButton;
    public GameObject continueButton;
    public GameObject startPlacementButton;
    public GameObject cancelButton;

    public GameObject urlScreen;
    public GameObject rectPlacementNotice;
    public GameObject qrPlacementNotice;
    public GameObject printingButton;

    public TMP_InputField urlInput;
    private Toggle transparencyToggle;

    private ContentParameters contentParameters = new ContentParameters();
    private Placement placementScript;

    private List<Action> actions;
    private int currentPhase;

    void Start()
    {
        placementScript = GetComponent<Placement>();
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
        currentPhase = 0;
        placementScript.enabled = false;
        placementScript.DeactivateUI();
        rectPlacementNotice.SetActive(false);
        qrPlacementNotice.SetActive(false);
        startPlacementButton.SetActive(true);
        urlScreen.SetActive(true);
    }

    public void EndUrlInput()
    {
        string userInput = urlInput.text;
        if (!string.IsNullOrEmpty(userInput) && Uri.IsWellFormedUriString(userInput, UriKind.RelativeOrAbsolute))
        {
            contentParameters.URL = userInput;
            contentParameters.Transparency = transparencyToggle;
            urlScreen.SetActive(false);
            startPlacementButton.SetActive(false);
            // TODO show success pop up
            StartRectPlacement();
        }
        else
        {
            urlInput.text = userInput + "<- FAIL because of: " +
                            Uri.IsWellFormedUriString(userInput, UriKind.RelativeOrAbsolute);
        }
    }

    public void StartRectPlacement()
    {
        placementScript.enabled = false;
        currentPhase = 1;
        cancelButton.SetActive(false);
        continueButton.SetActive(false);
        rectPlacementNotice.SetActive(true);
        qrPlacementNotice.SetActive(false);
        backButton.SetActive(true);
        placementScript.RestartPlanePlacement();
        placementScript.enabled = true;
    }

    public void EndRectPlacement()
    {
        placementScript.enabled = false;
        continueButton.SetActive(true);
        rectPlacementNotice.SetActive(false);
        cancelButton.SetActive(true);
        backButton.SetActive(false);
        placementScript.enabled = true;
    }

    public void StartQrPlacement()
    {
        placementScript.enabled = false;
        currentPhase = 2;
        cancelButton.SetActive(false);
        backButton.SetActive(true);
        qrPlacementNotice.SetActive(true);
        continueButton.SetActive(true);
        printingButton.SetActive(false);
        placementScript.StartQrMockPlacement();
        placementScript.enabled = true;
    }

    public void AskForPrint()
    {
        placementScript.enabled = false;
        currentPhase = 3;
        placementScript.FinishPlacement();
        continueButton.SetActive(false);
        printingButton.SetActive(true);
        placementScript.enabled = true;
    }


    public void ReceiveFinalParameters()
    {
        ContentParameters finalParams = placementScript.contentParameters;
        contentParameters.Offset = finalParams.Offset;
        contentParameters.Width = finalParams.Width;
        contentParameters.Height = finalParams.Height;
        placementScript.enabled = false;
        printingButton.SetActive(false);

        // TODO show success pop up and move on to other scene
    }

    public void GoBack()
    {
        int goToAction = currentPhase - 1;
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
        int goToAction = currentPhase + 1;
        if (goToAction < actions.Count)
        {
            actions[goToAction].Invoke();
        }
    }
}