using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    [StaticConstructorOnStartup]
    public static class Content
    {
        public static GameObject EnergyBallPrefab;

        static Content()
        {
            // Load content here.
            ModCore.Trace("Loading content...");

            var bundles = ModCore.Instance.Content.assetBundles;
            var bundle = bundles.GetByName("aa");
            EnergyBallPrefab = bundle.LoadAsset<GameObject>("OuterPrefab");

            ModCore.Trace("Got prefab: " + EnergyBallPrefab);
        }
    }
}
