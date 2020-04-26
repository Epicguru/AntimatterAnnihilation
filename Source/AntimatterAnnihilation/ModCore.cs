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
