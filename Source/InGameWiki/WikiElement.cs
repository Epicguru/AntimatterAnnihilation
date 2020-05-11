using UnityEngine;
using Verse;

namespace InGameWiki
{
    public class WikiElement
    {
        public string Text;

        public Texture2D Image;
        public Vector2 ImageSize = new Vector2(-1, -1);
        public float ImageScale = 1f;

        public WikiPage Link;

        public bool HasText
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Text);
            }
        }

        public bool HasImage
        {
            get
            {
                return Image != null;
            }
        }

        public virtual Vector2 Draw(Rect maxBounds)
        {
            Vector2 size = Vector2.zero;

            if (HasImage)
            {
                float width = ImageSize.x < 1 ? Image.width * ImageScale : ImageSize.x;
                float height = ImageSize.y < 1 ? Image.height * ImageScale : ImageSize.y;
                Widgets.DrawTextureFitted(new Rect(maxBounds.x, maxBounds.y, width, height), Image, 1f);
                size += new Vector2(width, height);
            }

            if (HasText)
            {
                float x = maxBounds.x;
                float width = maxBounds.xMax - x;
                //Widgets.Label(new Rect(x, maxBounds.y, width, maxBounds.height), Text);

                float startY = maxBounds.y;
                float cacheStartY = startY;
                Widgets.LongLabel(x, width, Text, ref startY);
                float change = cacheStartY - startY;

                size += new Vector2(width, change);
            }

            return size;
        }
    }
}
