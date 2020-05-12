using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace InGameWiki
{
    public class WikiPage
    {
        public static WikiPage CreateFromThing(ThingDef thing)
        {
            if (thing == null)
                return null;

            WikiPage p = new WikiPage();

            Log.Message(thing.LabelCap.ToString());
            p.Title = thing.LabelCap;
            p.ShortDescription = thing.DescriptionDetailed;
            p.Icon = thing.uiIcon;

            // Cost.
            if(thing.costList != null)
            {
                var cost = new SectionWikiElement();
                cost.Name = "Cost";

                foreach (var costThing in thing.costList)
                {
                    cost.Elements.Add(new WikiElement() { DefForIconAndLabel = costThing.thingDef, Text = costThing.count <= 1 ? "" : $"x{costThing.count}" });
                }
                if (cost.Elements.Count > 0)
                {
                    p.Elements.Add(cost);
                }

                // Show recipes added by this production thing.
                foreach (var rec in thing.AllRecipes)
                {
                    p.Elements.Add(WikiElement.Create(rec.defName));
                }
            }

            // Crafting (where is it crafted)
            if (thing.recipeMaker != null)
            {
                var crafting = new SectionWikiElement();
                crafting.Name = "Crafted at";

                foreach (var user in thing.recipeMaker.recipeUsers)
                {
                    crafting.Elements.Add(new WikiElement() {DefForIconAndLabel = user});
                }

                if(crafting.Elements.Count > 0)
                {
                    p.Elements.Add(crafting);
                }
            }

            // Research prerequisite.
            if (thing.researchPrerequisites != null && thing.researchPrerequisites.Count > 0)
            {
                var research = new SectionWikiElement();
                research.Name = "Research to unlock";

                foreach (var r in thing.researchPrerequisites)
                {
                    research.Elements.Add(new WikiElement() {Text = r.LabelCap});
                }

                p.Elements.Add(research);
            }

            p.Elements.Add(WikiElement.Create("This is a <b>test</b>."));

            return p;
        }

        public string Name;
        public string Title;
        public string ShortDescription;
        public Texture2D Icon;

        public List<WikiElement> Elements = new List<WikiElement>();

        private float lastHeight;
        private Vector2 scroll;
        private Vector2 descScroll;

        public string GetDisplayName()
        {
            return Name ?? Title;
        }

        public virtual void Draw(Rect maxBounds)
        {
            const int PADDING = 5;

            float topHeight = 128 + PADDING;

            // Icon.
            bool drawnIcon = Icon != null;
            Widgets.ButtonImage(new Rect(maxBounds.x + PADDING, maxBounds.y + PADDING, 128, 128), Icon, false);

            // Title.
            if (Title != null)
            {
                Text.Font = GameFont.Medium;
                float x = !drawnIcon ? maxBounds.x + PADDING : maxBounds.x + PADDING + 128 + PADDING;
                float w = !drawnIcon ? maxBounds.width - PADDING * 2 : maxBounds.width - PADDING * 2 - 128;
                Widgets.Label(new Rect(x, maxBounds.y + PADDING, w, 34), Title);
            }

            // Short description.
            if(ShortDescription != null)
            {
                Text.Font = GameFont.Small;
                float x = !drawnIcon ? maxBounds.x + PADDING : maxBounds.x + PADDING + 128 + PADDING;
                float y = Title == null ? maxBounds.y + PADDING : maxBounds.y + PADDING * 2 + 34;
                float w = !drawnIcon ? maxBounds.width - PADDING * 2 : maxBounds.width - PADDING * 2 - 128;
                float h = (maxBounds.y + PADDING + 128) - y;
                Widgets.LabelScrollable(new Rect(x, y, w, h), ShortDescription, ref descScroll, false, true, true);
            }

            Text.Font = GameFont.Small;
            maxBounds.y += topHeight;
            maxBounds.height -= topHeight;

            // Scroll view stuff.
            var whereToDraw = maxBounds;
            whereToDraw = whereToDraw.GetInner(PADDING);
            Widgets.BeginScrollView(whereToDraw, ref scroll, new Rect(maxBounds.x + PADDING, maxBounds.y + PADDING, maxBounds.width - 25 - PADDING * 2, lastHeight));
            lastHeight = 0;

            // Draw elements.
            Rect pos = whereToDraw;
            pos.width -= 18;
            foreach (var element in Elements)
            {
                if (element == null)
                    continue;

                try
                {
                    var size = element.Draw(pos);
                    pos.y += size.y + 10;
                    lastHeight += size.y + 10;
                }
                catch (Exception e)
                {
                    Log.Error($"In-game wiki exception when drawing element: {e}");
                }
            }

            Widgets.EndScrollView();
        }
    }
}
