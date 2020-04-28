using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    [StaticConstructorOnStartup]
    public static class Content
    {
        public static GameObject EnergyBallPrefab;
        public static GameObject EnergyBeamInPrefab;
        public static GameObject EnergyBeamOutPrefab;

        static Content()
        {
            // Load content here.
            var bundles = ModCore.Instance.Content.assetBundles;
            var bundle = bundles.GetByName("aa");
            EnergyBallPrefab = bundle.LoadAsset<GameObject>("OuterPrefab");
            EnergyBeamInPrefab = bundle.LoadAsset<GameObject>("In Beam");
            EnergyBeamOutPrefab = bundle.LoadAsset<GameObject>("Out Beam");

            ModCore.Trace("Loaded content.");
        }
    }
}
