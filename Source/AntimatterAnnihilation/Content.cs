using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    [StaticConstructorOnStartup]
    public static class Content
    {
        public static GameObject EnergyBallPrefab;
        public static GameObject EnergyBeamPrefab;

        static Content()
        {
            // Load content here.
            var bundles = ModCore.Instance.Content.assetBundles;
            var bundle = bundles.GetByName("aa");
            EnergyBallPrefab = bundle.LoadAsset<GameObject>("OuterPrefab");
            EnergyBeamPrefab = bundle.LoadAsset<GameObject>("Beam");

            ModCore.Trace("Loaded content.");
        }
    }
}
