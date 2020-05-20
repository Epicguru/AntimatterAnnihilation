using UnityEngine;
using Verse;

namespace InGameWiki.Internal
{
    public class UI_ImageInspector : Window
    {
        public Texture2D Image;

        public override Vector2 InitialSize
        {
            get
            {
                if (Image == null)
                    return new Vector2(256, 256);

                float longest = Mathf.Max(Image.width, Image.height);
                float scale = longest <= 128 ? 2f : 1f;
                return new Vector2(Image.width * scale, Image.height * scale);
            }
        }

        public UI_ImageInspector(Texture2D image)
        {
            Image = image;
            doCloseX = true;
            resizeable = true;
            draggable = true;
            closeOnClickedOutside = false; // Or true?
        }

        public override void DoWindowContents(Rect inRect)
        {
            if(Image == null)
            {
                Close();
                return;
            }

            Widgets.DrawTextureFitted(inRect, Image, 1f);
        }
    }
}
