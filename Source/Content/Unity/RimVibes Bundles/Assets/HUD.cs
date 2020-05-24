using UnityEngine;

public class HUD : MonoBehaviour
{
    private void OnGUI()
    {
        DrawHUD(new Rect(-450 * 0.5f, -138 * 0.5f, 450, 138));
    }

    public enum HUDAnchor : byte
    {
        Left,
        Right,
        Top,
        Bottom,
        Free
    }

    [Range(0.5f, 2f)]
    public float Scale = 1f;
    public HUDAnchor Anchor;
    public Vector2 Offset;
    public Texture2D AlbumArt;
    public Texture2D VolumeIcon;
    public Texture2D SettingsIcon;
    public Texture2D PauseButton, PlayButton;
    public Texture2D NextButton, PreviousButton;
    public Texture2D ShuffleButtonNormal, ShuffleButtonActive;
    public Texture2D RepeatButtonNormal, RepeatButtonActive, RepeatButtonOne;

    private bool isPlaying;
    private bool isShuffling;
    private float normalizedTime;
    private float volume;
    private byte repeatMode;

    private void DrawHUD(Rect rect)
    {
        float scale = Scale;
        float realHeight = 138 * scale * 0.5f;
        float realWidth = 450 * scale * 0.5f;

        Vector2 offset = Offset;
        Vector2 pos = Vector3.zero;
        const float Padding = 10f;
        switch (Anchor)
        {
            case HUDAnchor.Right:
                pos = offset + new Vector2(Screen.width - realWidth - Padding, Screen.height * 0.5f);
                break;

            case HUDAnchor.Left:
                pos = offset + new Vector2(Padding + realWidth, Screen.height * 0.5f);
                break;

            case HUDAnchor.Top:
                pos = offset + new Vector2(Screen.width * 0.5f, Padding + realHeight);
                break;

            case HUDAnchor.Bottom:
                pos = offset + new Vector2(Screen.width * 0.5f, Screen.height - Padding - realHeight);
                break;

            case HUDAnchor.Free:
                pos = offset + new Vector2(Screen.width, Screen.height) * 0.5f;
                break;
        }

        var oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.TRS(pos, Quaternion.identity, Vector3.one * scale);
        GUI.Box(rect, "");
        rect.x += 5;
        rect.y += 5;
        rect.width -= 10;
        rect.height -= 10;
        
        GUI.DrawTexture(new Rect(rect.x, rect.y, 128, 128), AlbumArt, ScaleMode.StretchToFill, true, 0f, Color.white, 0f, 3f);
        MoveRight(138);

        SetFontSize(22);
        GUI.Label(new Rect(rect.x, rect.y, rect.xMax - rect.x, 30), "Pickin' on Series - Oppression (The Human Experience Remix)");
        MoveDown(28);
        SetFontSize(18);
        GUI.Label(new Rect(rect.x, rect.y, rect.xMax - rect.x, 30), "<b>The Human Experience</b>");
        MoveDown(28);

        GUI.color = new Color(0, 0, 0, 0);
        if (GUI.Button(new Rect(rect.x + (rect.width - 32) * 0.5f, rect.y, 32, 32), ""))
            TogglePlay();
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(rect.x + (rect.width - 32) * 0.5f, rect.y, 32, 32), isPlaying ? PauseButton : PlayButton);

        GUI.DrawTexture(new Rect(rect.x + (rect.width - 32) * 0.5f - 35, rect.y + 8, 16, 16), PreviousButton);
        GUI.DrawTexture(new Rect(rect.x + (rect.width - 32) * 0.5f + 35 + 16, rect.y + 8, 16, 16), NextButton);

        GUI.color = new Color(0, 0, 0, 0);
        if (GUI.Button(new Rect(rect.x + (rect.width - 32) * 0.5f - 70, rect.y + 8, 16, isShuffling ? 24 : 16), ""))
            ToggleShuffle();
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(rect.x + (rect.width - 32) * 0.5f - 70, rect.y + 8, 16, isShuffling ? 24 : 16), isShuffling ? ShuffleButtonActive : ShuffleButtonNormal);

        GUI.color = new Color(0, 0, 0, 0);
        if (GUI.Button(new Rect(rect.x + (rect.width - 32) * 0.5f + 84, rect.y + 8, 16, repeatMode != 0 ? 24 : 16), ""))
            ToggleRepeatMode();
        GUI.color = Color.white;
        GUI.DrawTexture(new Rect(rect.x + (rect.width - 32) * 0.5f + 84, rect.y + 8, 16, repeatMode != 0 ? 24 : 16), repeatMode == 0 ? RepeatButtonNormal : repeatMode == 1 ? RepeatButtonActive : RepeatButtonOne);

        MoveDown(38);

        normalizedTime = GUI.HorizontalSlider(new Rect(rect.x + 30, rect.y, rect.width - 60, 20), normalizedTime, 0f, 1f);
        SetFontSize(12);
        GUI.Label(new Rect(rect.x, rect.y - 4, 30, 20), "2:33");
        GUI.Label(new Rect(rect.xMax - 25, rect.y - 4, 30, 20), "5:70");
        MoveDown(30);

        volume = GUI.HorizontalSlider(new Rect(rect.xMax - 80, rect.yMax - 15, 70, 20), volume, 0f, 1f);
        GUI.DrawTexture(new Rect(rect.xMax - 100, rect.yMax - 17, 16, 16), VolumeIcon);

        SetFontSize(12);
        GUI.Label(new Rect(rect.x, rect.y - 13, rect.width, 20), "Up Next: Lose Yourself");

        GUI.Button(new Rect(rect.xMax - 24 + 5, rect.yMax + 10, 24, 24), SettingsIcon);

        GUI.matrix = oldMatrix;

        void MoveRight(float amount)
        {
            rect.x += amount;
            rect.width -= amount;
        }
        void MoveDown(float amount)
        {
            rect.y += amount;
            rect.height -= amount;
        }
    }

    private void TogglePlay()
    {
        isPlaying = !isPlaying;
    }

    private void ToggleShuffle()
    {
        isShuffling = !isShuffling;
    }

    private void ToggleRepeatMode()
    {
        repeatMode++;
        if (repeatMode == 3)
            repeatMode = 0;
    }

    private void SetFontSize(int size)
    {
        GUI.skin.label.fontSize = size;
    }
}
