using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QRDemonstrator : MonoBehaviour
{
    private string qRString;
    private Texture2D qrCode;
    private QREncoder encoder;

    private void Start()
    {
        encoder = new QREncoder();
        qRString = "";
    }

    public void Receive(string text)
    {
        if (!qRString.Equals(text))
        {
            qRString = text;
            qrCode = encoder.Encode(qRString);
        }
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        GUILayout.Label(" " + qRString, guiStyle);
        GUI.Button(new Rect(400, 400, 256, 256), qrCode, GUIStyle.none);
    }
}
