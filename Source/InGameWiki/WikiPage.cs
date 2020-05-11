using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace InGameWiki
{
    public class WikiPage
    {
        public string Title;
        public string ShortDescription;
        public Texture2D Icon;

        public List<WikiElement> Elements = new List<WikiElement>();

        public virtual void Draw(Rect bounds)
        {
            // TODO draw title, icon, description.

            // Draw elements.
            // TODO scroll.
            foreach (var page in Elements)
            {
                if (page == null)
                    return;

                Rect pos = bounds;
                try
                {
                    var size = page.Draw(pos);
                    pos.y += size.y + 10;
                }
                catch (Exception e)
                {
                    Log.Error($"In-game wiki exception when drawing element: {e}");
                }
            }
        }
    }
}
