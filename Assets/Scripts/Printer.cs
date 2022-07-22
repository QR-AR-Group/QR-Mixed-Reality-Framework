using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour
{
    private QREncoder qrEncoder;

    private void Start(){
        qrEncoder = new QREncoder();
    }

    public void Print(string text){
        string qrFilePath = CreateQRCode(text);
        ShareQRCode(qrFilePath);
    }

    private string CreateQRCode(string text){
        Texture2D qrCode = qrEncoder.Encode(text);
        string qrFilePath = Application.persistentDataPath + "/qrcode.png";
        byte[] bytes = qrCode.EncodeToPNG();
        System.IO.File.WriteAllBytes(qrFilePath, bytes);
        return qrFilePath;
    }

    private void ShareQRCode(string qrFilePath){
        AndroidJavaClass unity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer");
        AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject> ("currentActivity");

        AndroidJavaClass photoPrinterClass = new AndroidJavaClass("androidx.print.PrintHelper");
        AndroidJavaObject photoPrinter = new AndroidJavaObject("androidx.print.PrintHelper", currentActivity);

        photoPrinter.Call("setScaleMode", photoPrinterClass.GetStatic<int>("SCALE_MODE_FIT"));

        AndroidJavaClass bitmapFactory = new AndroidJavaClass("android.graphics.BitmapFactory");
        AndroidJavaObject bitmap = bitmapFactory.CallStatic<AndroidJavaObject>("decodeFile", qrFilePath);

        photoPrinter.Call("printBitmap", "Print QR Code", bitmap);
    }


    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        //GUILayout.Label(" Clicked: " + isClicked + " - " + log , guiStyle);
        //GUI.Button(new Rect(400, 400, 256, 256), qrCode, GUIStyle.none);
    }
}
