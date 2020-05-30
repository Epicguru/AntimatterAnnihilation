using AntimatterAnnihilation.Effects;
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
        public static GameObject UpBeamPrefab;
        public static GameObject MeguminChargePrefab;

        public static Texture2D Expand, Collapse;
        public static Texture2D PowerNetGraph;
        public static Texture2D PowerLevelLow, PowerLevelMedium, PowerLevelHigh;
        public static Texture2D AutoAttackIcon, CancelIcon, ArrowIcon;

        static Content()
        {
            // Load content here.
            var bundles = ModCore.Instance.Content.assetBundles;
            var bundle = bundles.GetByName("aa");
            EnergyBallPrefab = bundle.LoadAsset<GameObject>("OuterPrefab");
            EnergyBeamInPrefab = bundle.LoadAsset<GameObject>("In Beam");
            EnergyBeamOutPrefab = bundle.LoadAsset<GameObject>("Out Beam");
            UpBeamPrefab = bundle.LoadAsset<GameObject>("UpBeam");
            MeguminChargePrefab = bundle.LoadAsset<GameObject>("MegCharge");
            ExplosionEffectManager.Prefab = bundle.LoadAsset<GameObject>("LargeAntimatterExplosion");
            RailgunEffectSpawner.Prefab = bundle.LoadAsset<GameObject>("RailgunEffect");

            Expand = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/Expand");
            Collapse = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/Collapse");
            PowerNetGraph = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/PowerNetConsole Graph");
            PowerLevelLow = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/PowerLevelLowIcon");
            PowerLevelMedium = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/PowerLevelMediumIcon");
            PowerLevelHigh = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/PowerLevelHighIcon");
            AutoAttackIcon = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/AutoAttackIcon");
            CancelIcon = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/CancelIcon");
            ArrowIcon = ContentFinder<Texture2D>.Get("AntimatterAnnihilation/UI/ArrowIcon");

            ModCore.Trace("Loaded content.");
        }
    }
}
