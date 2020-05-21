using InGameWiki;
using Verse;

namespace AntimatterAnnihilation
{
    [StaticConstructorOnStartup]
    internal static class Wiki
    {
        static Wiki()
        {
            // Create wiki here since defs is now loaded.
            var wiki = ModWiki.Create(ModCore.Instance);
            wiki.WikiTitle = "Antimatter Annihilation";

            // Optional - save a reference to the wiki.
            ModCore.Instance.Wiki = wiki;
        }
    }
}
