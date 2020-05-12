using UnityEngine;
using Verse;

namespace InGameWiki
{
    public class SectionWikiElement : CompoundWikiElement
    {
        public string Name = "Section Name";
        public bool Hidden = true;

        private float lastHeight;

        public override Vector2 Draw(Rect maxBounds)
        {
            Rect area = new Rect(maxBounds.x, maxBounds.y + 40, maxBounds.width, lastHeight);
            lastHeight = 0;
            Vector2 size = Vector2.zero;

            Verse.Text.Font = GameFont.Medium;
            if (Widgets.ButtonText(new Rect(maxBounds.x, maxBounds.y, 65, 32), $"{(Hidden ? "Show" : "Hide")}"))
            {
                Hidden = !Hidden;
            }
            if (Name != null)
            {
                Widgets.Label(new Rect(maxBounds.x + 70, maxBounds.y, maxBounds.width - 70, 40), Name);
            }
            Verse.Text.Font = GameFont.Small;

            size.y = 40;

            Rect pos = new Rect(area.x, area.y, area.width, 69420);

            if (!Hidden)
            {
                foreach (var element in Elements)
                {
                    var thisSize = element.Draw(pos);
                    size.y += thisSize.y + 10;
                    pos.y += thisSize.y + 10;
                    lastHeight += thisSize.y + 10;
                }
            }

            size.x = area.width;
            return size;
        }
    }
}
