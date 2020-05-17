using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Verse;

namespace InGameWiki
{
    public class ModWiki
    {
        public static IReadOnlyList<ModWiki> AllWikis
        {
            get
            {
                return allWikis;
            }
        }
        private static List<ModWiki> allWikis = new List<ModWiki>();
        
        public static void Patch(Harmony harmonyInstance)
        {
            if (harmonyInstance == null)
                return;

            var method = typeof(Dialog_InfoCard).GetMethod("DoWindowContents", BindingFlags.Public | BindingFlags.Instance);
            if (method == null)
            {
                Log.Error($"Failed to get method Dialog_InfoCard.DoWindowContents to patch. Did Rimworld update in a major way?");
                return;
            }

            var postfix = typeof(InspectorPatch).GetMethod("InGameWikiPostfix", BindingFlags.NonPublic | BindingFlags.Static);
            if (postfix == null)
            {
                Log.Error("Failed to get local patch method...");
                return;
            }
            var patch = new HarmonyMethod(postfix);

            bool canPatch = true;
            var patches = Harmony.GetPatchInfo(method);
            if(patches != null)
            {
                foreach (var pf in patches.Postfixes)
                {
                    if (pf.PatchMethod.Name == "InGameWikiPostfix")
                    {
                        canPatch = false;
                        break;
                    }
                    else
                    {
                        Log.Warning($"There is already a postfix on Dialog_InfoCard.DoWindowContents: {pf.PatchMethod.Name} by {pf.owner}. This could affect functionality of wiki patch.");
                    }
                }
            }

            if (canPatch)
            {
                harmonyInstance.Patch(method, postfix: patch);
                Log.Message($"Patched dialog window for in-game wiki (caused by {harmonyInstance.Id}).");
            }
        }

        public static void OpenInspectWindow(Def def)
        {
            if (def != null)
                Find.WindowStack.Add(new Dialog_InfoCard(def));
        }

        public static ModWiki Create(Mod mod)
        {
            if(mod == null)
            {
                Log.Error("Cannot pass in null mod to create wiki.");
                return null;
            }

            var wiki = new ModWiki();
            wiki.GenerateFromMod(mod);
            wiki.Mod = mod;

            allWikis.Add(wiki);

            return wiki;
        }

        public static (ModWiki wiki, WikiPage page) TryFindPage(Def def)
        {
            if (def == null)
                return (null, null);

            foreach (var wiki in AllWikis)
            {
                var found = wiki.FindPageFromDef(def.defName);
                if (found != null)
                    return (wiki, found);
            }

            return (null, null);
        }

        public static (ModWiki wiki, WikiPage page) TryFindPage(string pageID)
        {
            if (pageID == null)
                return (null, null);

            foreach (var wiki in AllWikis)
            {
                var found = wiki.FindPageFromID(pageID);
                if (found != null)
                    return (wiki, found);
            }

            return (null, null);
        }

        public static void OpenPage(ModWiki wiki, WikiPage page)
        {
            if (WikiWindow.CurrentActive != null && WikiWindow.CurrentActive.Wiki == wiki)
            {
                WikiWindow.CurrentActive.CurrentPage = page;
            }
            else
            {
                WikiWindow.Open(wiki, page);
            }
        }

        public string WikiTitle = "Your Mod Name Here";
        public List<WikiPage> Pages = new List<WikiPage>();
        public Mod Mod { get; private set; }

        private ModWiki()
        {

        }

        private void GenerateFromMod(Mod mod)
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

        public WikiPage FindPageFromDef(string defName)
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

        public WikiPage FindPageFromID(string pageID)
        {
            if (pageID == null)
                return null;

            foreach (var page in Pages)
            {
                if (page != null && page.ID == pageID)
                    return page;
            }
            return null;
        }
    }
}
