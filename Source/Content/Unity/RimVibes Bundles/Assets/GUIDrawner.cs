using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIDrawner : MonoBehaviour
{
    private Camera uiCam;

    void Start()
    {
        uiCam = new GameObject("HEY").AddComponent<Camera>();
        uiCam.enabled = false;
    }

    void OnGUI()
    {
        GUI.Box(new Rect(0, 0, Screen.width, Screen.height), "");

        if(Event.current.type == EventType.Repaint)
        {
            uiCam.Render(); // this will render the new UI
        }
    }
}
