using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A custom mouse cursor for video recordings
public class CustomMouse : MonoBehaviour
{
    public Texture2D mouseTexture;

    void Start()
    {
        //A custom mouse cursor for recording
        Cursor.SetCursor(mouseTexture, Vector2.zero, CursorMode.ForceSoftware);
    }

    
    void Update()
    {
        
    }
}
