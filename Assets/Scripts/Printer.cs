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
    private string qrFilePath;

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
        qrFilePath = Application.persistentDataPath + "/" + qrFileName;
        byte[] bytes = qrCode.EncodeToPNG();
        System.IO.File.WriteAllBytes(qrFilePath, bytes);
        log = bytes.Length/1024 + " saved to " + qrFilePath;
    }

    private void ShareQRCode(){
        AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");

        AndroidJavaClass photoPrinterClass = new AndroidJavaClass("androidx.print.PrintHelper");
        AndroidJavaObject photoPrinter = new AndroidJavaObject("androidx.print.PrintHelper", currentActivity);

        photoPrinter.Call("setScaleMode", photoPrinterClass.GetStatic<int>("SCALE_MODE_FIT"));

        AndroidJavaClass bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory");
        AndroidJavaObject bitmap = bitmapFactory.CallStatic<AndroidJavaObject>("decodeFile", qrFilePath);

        photoPrinter.Call("printBitmap", "Print QR Code", bitmap);
        log = "should have been printed";
    }
    

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        GUILayout.Label(" Clicked: " + isClicked + " - " + log , guiStyle);
        GUI.Button(new Rect(400, 400, 256, 256), qrCode, GUIStyle.none);
    }
}
