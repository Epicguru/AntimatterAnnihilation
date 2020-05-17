using UnityEngine;
using Verse;

namespace InGameWiki
{
    public class WikiElement
    {
        public static WikiElement Create(string text)
        {
            return Create(text, null, null);
        }

        public static WikiElement Create(Texture2D image, Vector2? imageSize = null)
        {
            return Create(null, image, imageSize);
        }

        public static WikiElement Create(string text, Texture2D image, Vector2? imageSize = null)
        {
            return new WikiElement()
            {
                Text = text,
                Image = image,
                ImageSize = imageSize ?? new Vector2(-1, -1)
            };
        }

        public string Text;
        public Texture2D Image;
        public bool AutoFitImage = false;
        public Vector2 ImageSize = new Vector2(-1, -1);
        public float ImageScale = 1f;
        public GameFont FontSize = GameFont.Small;
        public Def DefForIconAndLabel;

        public string PageLink;
        public (ModWiki wiki, WikiPage page) PageLinkReal;
        public bool IsLinkBroken { get; private set; }

        public bool HasText
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Text) || PageLink != null;
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

            var old = Verse.Text.Font;
            Verse.Text.Font = FontSize;

            Vector2 imageOffset = Vector2.zero;
            if (HasImage)
            {
                if (!AutoFitImage)
                {
                    float width = ImageSize.x < 1 ? Image.width * ImageScale : ImageSize.x;
                    float height = ImageSize.y < 1 ? Image.height * ImageScale : ImageSize.y;
                    Widgets.DrawTextureFitted(new Rect(maxBounds.x, maxBounds.y, width, height), Image, 1f);
                    size += new Vector2(width, height);
                    imageOffset.x = width;
                }
                else
                {
                    float baseWith = Image.width;

                    if(baseWith <= maxBounds.width)
                    {
                        float width = Image.width;
                        float height = Image.height;
                        Widgets.DrawTextureFitted(new Rect(maxBounds.x, maxBounds.y, width, height), Image, 1f);
                        size += new Vector2(width, height);
                        imageOffset.x = width;
                    }
                    else
                    {
                        float width = maxBounds.width;
                        float height = Image.height * (width / Image.width);
                        Widgets.DrawTextureFitted(new Rect(maxBounds.x, maxBounds.y, width, height), Image, 1f);
                        size += new Vector2(width, height);
                        imageOffset.x = width;
                    }
                }
            }

            if (DefForIconAndLabel != null)
            {
                //TooltipHandler.TipRegion(rect, (TipSignal)def.description);
                var rect = new Rect(maxBounds.x, maxBounds.y, 200, 32);
                Widgets.DefLabelWithIcon(rect, DefForIconAndLabel);
                if (Widgets.ButtonInvisible(rect, true))
                {
                    WikiWindow.CurrentActive?.GoToPage(this.DefForIconAndLabel, true);
                }
                size.y += 32;
                imageOffset.x = 200;
                imageOffset.y = 6;
            }

            if (HasText)
            {
                bool isLink = PageLink != null;

                float x = maxBounds.x + imageOffset.x;
                float width = maxBounds.xMax - x;

                float startY = maxBounds.y + imageOffset.y;
                float cacheStartY = startY;

                if (isLink && PageLinkReal.page == null && !IsLinkBroken)
                {
                    var found = ModWiki.TryFindPage(PageLink);
                    if (found.page == null)
                    {
                        IsLinkBroken = true;
                    }
                    else
                    {
                        PageLinkReal = found;
                    }
                }

                string linkText = IsLinkBroken ? $"<color=#ff2b2b><b><i>Link (broken, please report): [{PageLink}]</i></b></color>" : $"<color=#9c9c9c><b><i>Link:</i></b></color>{PageLinkReal.page?.Title}";
                string txt = isLink ? linkText : Text;

                Widgets.LongLabel(x, width, txt, ref startY);
                float change = startY - cacheStartY;

                if (isLink)
                {
                    Rect bounds = new Rect(x, cacheStartY, width, change);
                    Widgets.DrawHighlightIfMouseover(bounds);
                    if (!IsLinkBroken && Widgets.ButtonInvisible(bounds))
                    {
                        // Go to link.
                        ModWiki.OpenPage(PageLinkReal.wiki, PageLinkReal.page);
                    }
                }

                size += new Vector2(width, 0);
                if (size.y < change)
                    size.y = change;
            }

            Verse.Text.Font = old;

            return size;
        }
    }
}
