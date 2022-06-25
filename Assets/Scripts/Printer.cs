using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Printer : MonoBehaviour
{
    private string text;
    public void Print(){
        text = "Button pressed";
    }

    void OnGUI()
    {
        GUIStyle guiStyle = new GUIStyle();
        guiStyle.fontSize = 50;
        GUILayout.Label(" Hello " + text, guiStyle);
    }
}
