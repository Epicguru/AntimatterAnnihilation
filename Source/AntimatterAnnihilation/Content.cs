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

        public static Texture2D Expand, Collapse;
        public static Texture2D PowerNetGraph;

        static Content()
        {
            // Load content here.
            var bundles = ModCore.Instance.Content.assetBundles;
            var bundle = bundles.GetByName("aa");
            EnergyBallPrefab = bundle.LoadAsset<GameObject>("OuterPrefab");
            EnergyBeamInPrefab = bundle.LoadAsset<GameObject>("In Beam");
            EnergyBeamOutPrefab = bundle.LoadAsset<GameObject>("Out Beam");

            Expand = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/Expand");
            Collapse = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/Collapse");
            PowerNetGraph = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/PowerNetConsole Graph");

            ModCore.Trace("Loaded content.");
        }
    }
}
