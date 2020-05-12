using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    public class ModCore : Mod
    {
        public static ModCore Instance { get; private set; }

        public ModCore(ModContentPack content) : base(content)
        {
            Instance = this;
            Log("Hello, world!");

            AddHook();
            Trace("Added hook.");

            Harmony harmony = new Harmony("epicguru.AntimatterAnnihilation");
            harmony.PatchAll();
            Log($"Patched {harmony.GetPatchedMethods().Count()} methods.");
        }

        private void LoadSomethingFromTheBundle()
        {
            // Get the first asset bundle that Rimworld automatically loads.
            var firstBundle = Content.assetBundles.loadedAssetBundles[0];

            // Get a custom prefab out of the bundle.
            var customPrefab = firstBundle.LoadAsset<GameObject>("MyPrefab");

            // Do whatever you want with the prefab. You could spawn it, grab a shader off it, etc.
            // ...
            var exampleShader = customPrefab.GetComponent<MeshRenderer>().material.shader;
        }

        private void AddHook()
        {
            var go = new GameObject("AntimatterAnnihilation Hook");
            go.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(go);

            go.AddComponent<Hook>();
        }

        public static void Log(string msg)
        {
          Verse.Log.Message(msg ?? "<null>");

        }

        public static void Trace(string msg)
        {
            Verse.Log.Message(msg ?? "<null>");
        }
    }
}
