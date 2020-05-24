using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WikiUI : MonoBehaviour
{
    public Rect Bounds;
    public float TopHeight = 70;
    public float SideWidth = 200;
    public float SearchHeight = 50;

    public void OnGUI()
    {
        var global = Bounds;
        Rect titleArea = new Rect(global.x + 5, global.y + 5, global.width - 10, TopHeight);
        Rect pagesArea = new Rect(global.x + 5, global.y + TopHeight + 15 + SearchHeight, SideWidth - 5, global.height - 20 - TopHeight - SearchHeight);
        Rect searchArea = new Rect(global.x + 5, global.y + TopHeight + 10, SideWidth - 5, SearchHeight);
        Rect contentArea = new Rect(global.x + 5 + SideWidth, global.y + TopHeight + 10, global.width - SideWidth - 10, global.height - TopHeight - 15);

        GUI.color = Color.Lerp(Color.black, Color.clear, 0.4f);
        GUI.Box(global, "");
        GUI.Box(pagesArea, "");
        GUI.Box(searchArea, "");
        GUI.Box(titleArea, "");
        GUI.Box(contentArea, "");
        GUI.color = Color.white;

        GUI.Label(titleArea, "Some Mod Name");
    }
}
