using System.Collections.Generic;
using System.IO;
using Verse;

namespace InGameWiki
{
    public class ModWiki
    {
        public static void OpenInspectWindow(Def def)
        {
            if (def != null)
                Find.WindowStack.Add((Window)new Dialog_InfoCard(def));
        }

        public string ModTitle = "Your Mod Name Here";

        public List<WikiPage> Pages = new List<WikiPage>();

        public void GenerateFromMod(Mod mod)
        {
            foreach (var def in mod.Content.AllDefs)
            {
                if (!(def is ThingDef thingDef))
                    continue;

                bool shouldAdd = AutogenPageFilter(thingDef);
                if (!shouldAdd)
                    continue;

                var page = WikiPage.CreateFromThingDef(thingDef); 
                this.Pages.Add(page);
            }

            string dir = Path.Combine(mod.Content.RootDir, "Wiki");
            PageParser.AddAllFromDirectory(this, dir);
        }

        public virtual bool AutogenPageFilter(ThingDef def)
        {
            if (def == null)
                return false;

            if (def.IsBlueprint)
                return false;

            if (def.projectile != null)
                return false;

            if (def.entityDefToBuild != null)
                return false;

            if (def.weaponTags?.Contains("TurretGun") ?? false)
                return false;

            return true;
        }

        public WikiPage GetPage(string defName)
        {
            if (defName == null)
                return null;

            foreach (var page in Pages)
            {
                if (page != null && page.Def?.defName == defName)
                    return page;
            }
            return null;
        }
    }
}
