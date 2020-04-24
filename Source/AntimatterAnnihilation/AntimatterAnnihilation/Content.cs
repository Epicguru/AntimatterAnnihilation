using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    [StaticConstructorOnStartup]
    public static class Content
    {
        public static GameObject Prefab;

        static Content()
        {
            // Load content here.
            ModCore.Trace("Loading content...");

            var bundles = ModCore.Instance.Content.assetBundles;
            var bundle = bundles.GetByName("aa");
            Prefab = bundle.LoadAsset<GameObject>("Prefab");

            ModCore.Trace("Got prefab: " + Prefab);
        }
    }
}
