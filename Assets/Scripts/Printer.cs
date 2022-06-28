using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour
{
    private QREncoder qrEncoder;

    private bool isClicked = false;
    private string testText = "This is a Text";
    private Texture2D qrCode;
    private string qrFileName = "qrcode.png";
    private string log;

    private void Start(){
        qrEncoder = new QREncoder();
    }

    public void Print(){
        isClicked = true;

        CreateQRCode();
        ShareQRCode();
    }

    private void CreateQRCode(){
        qrCode = qrEncoder.Encode(testText);
        string screenShotPath = Application.persistentDataPath + "/" + qrFileName;
        byte[] bytes = qrCode.EncodeToPNG();
        System.IO.File.WriteAllBytes(screenShotPath, bytes);
        log = bytes.Length/1024 + " saved to " + screenShotPath;
    }

    private void ShareQRCode(){
         //create a Toast class object
        AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");

        //create an array and add params to be passed
        object[] toastParams = new object[3];
        AndroidJavaClass unityActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        toastParams[0] = unityActivity.GetStatic<AndroidJavaObject>("currentActivity");
        toastParams[1] = testText;
        toastParams[2] = toastClass.GetStatic<int>("LENGTH_LONG");

        //call static function of Toast class, makeText
        AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", toastParams);

        //show toast
        toastObject.Call("show");
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        GUILayout.Label(" Clicked: " + isClicked + " - " + log , guiStyle);
        GUI.Button(new Rect(400, 400, 256, 256), qrCode, GUIStyle.none);
    }
}
