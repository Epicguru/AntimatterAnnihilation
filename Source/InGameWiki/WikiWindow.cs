using UnityEngine;
using Verse;

namespace InGameWiki
{
    public class WikiWindow : Window
    {
        public static WikiWindow Open(ModWiki wiki)
        {
            if (wiki == null)
                return null;
            if (CurrentActive != null)
            {
                Log.Message("There is already an open wiki page.");
                return null;
            }

            var created = new WikiWindow(wiki);
            CurrentActive = created;
            Find.WindowStack?.Add(created);

            return created;
        }
        public static WikiWindow CurrentActive { get; private set; }

        public ModWiki Wiki;
        public int TopHeight = 34;
        public int SearchHeight = 34;
        public int SideWidth = 260;
        public WikiPage CurrentPage { get; set; }
        public override Vector2 InitialSize => new Vector2(900, 800);
        public string SearchText = "";

        private Vector2 scroll;
        private float lastHeight;

        protected WikiWindow(ModWiki wiki)
        {
            this.Wiki = wiki;
            resizeable = true;
            doCloseButton = true;
            draggable = true;
            drawShadow = true;
            onlyOneOfTypeAllowed = true;
            absorbInputAroundWindow = false;
            preventCameraMotion = false;
        }

        public override void DoWindowContents(Rect maxBounds)
        {
            var global = new Rect(maxBounds.x, maxBounds.y, maxBounds.width, maxBounds.height - 50);
            Rect titleArea = new Rect(global.x, global.y, global.width, TopHeight);
            Rect searchArea = new Rect(global.x, global.y + TopHeight + 5, SideWidth, SearchHeight);
            Rect pagesArea = new Rect(global.x, global.y + TopHeight + 10 + SearchHeight, SideWidth, global.height - 10 - TopHeight - SearchHeight);
            Rect contentArea = new Rect(global.x + SideWidth + 5, global.y + TopHeight + 5, global.width - SideWidth - 5, global.height - TopHeight - 5);

            Widgets.DrawBoxSolid(pagesArea, Color.white * 0.4f);
            Widgets.DrawBox(pagesArea);
            Widgets.DrawBox(titleArea);
            Widgets.DrawBox(contentArea);

            // Title
            Text.Font = GameFont.Medium;
            Widgets.Label(titleArea, Wiki.ModTitle);

            // Search box.
            SearchText = Widgets.TextField(searchArea, SearchText);

            // Draw all pages list.
            Widgets.BeginScrollView(pagesArea, ref scroll, new Rect(pagesArea.x, pagesArea.y, pagesArea.width, lastHeight));
            lastHeight = 0;

            foreach (var page in Wiki.Pages)
            {
                if (page == null)
                    continue;

                Widgets.ButtonImage(new Rect(pagesArea.x + 4, pagesArea.y + 4 + lastHeight + 5, 24, 24), page.Icon, Color.white, Color.white, false);
                bool clicked = Widgets.ButtonText(new Rect(pagesArea.x + 28, pagesArea.y + 4 + lastHeight, pagesArea.width - 28 - 4, 40), page.GetDisplayName());
                if (clicked)
                {
                    CurrentPage = page;
                }

                lastHeight += 32 + 5;
            }

            Widgets.EndScrollView();

            // Current page.
            CurrentPage?.Draw(contentArea);
        }

        public override void PreClose()
        {
            CurrentActive = null;
            base.PreClose();
        }

        public bool GoToPage(Def def, bool openInspectWindow = false)
        {
            if (def == null)
                return false;

            var page = Wiki.GetPage(def.defName);
            if (page == null)
            {
                if (openInspectWindow)
                {
                    ModWiki.OpenInspectWindow(def);
                    return true;
                }
                return false;
            }
            else
            {
                this.CurrentPage = page;
                return true;
            }
        }
    }
}
