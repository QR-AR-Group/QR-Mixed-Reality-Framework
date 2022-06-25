using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour
{
    private bool isClicked = false;

    private bool isFocus = false;

    private string screenShotName;
    private bool isProcessing = false;
    private string shareSubject, shareMessage;

    public string toastText = "This is a Toast";

    public void Print(){
        isClicked = true;

        screenShotName =  "qr-code.png";

        shareSubject = "Print QR-Code";
        shareMessage = "Print QR-Code";

        ShareQRCode();
    }

    private void ShareQRCode(){
         //create a Toast class object
        AndroidJavaClass toastClass =
                    new AndroidJavaClass("android.widget.Toast");

        //create an array and add params to be passed
        object[] toastParams = new object[3];
        AndroidJavaClass unityActivity =
          new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        toastParams[0] =
                     unityActivity.GetStatic<AndroidJavaObject>
                               ("currentActivity");
        toastParams[1] = toastText;
        toastParams[2] = toastClass.GetStatic<int>
                               ("LENGTH_LONG");

        //call static function of Toast class, makeText
        AndroidJavaObject toastObject =
                        toastClass.CallStatic<AndroidJavaObject>
                                      ("makeText", toastParams);

        //show toast
        toastObject.Call("show");
    }

    void OnApplicationFocus(bool focus){
        isFocus = focus;
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        GUILayout.Label(" Clicked: " + isClicked, guiStyle);
    }
}
