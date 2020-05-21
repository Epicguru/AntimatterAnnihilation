using AntimatterAnnihilation.AI;
using AntimatterAnnihilation.Effects;
using InGameWiki;
using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation
{
    public class Hook : MonoBehaviour
    {
        public static Hook Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void OnGUI()
        {
            try
            {
                GUILayout.Space(120);
                bool spawn = GUILayout.Button("Spawn");
                bool openWiki = GUILayout.Button("Show wiki window");

                if (spawn)
                {
                    Vector3 pos = new Vector3(15, 0, 15);
                    var thing = new UpBeam(Find.CurrentMap, pos);
                    thing.SetVisScale(1f);
                }

                if (openWiki)
                {
                    WikiWindow.Open(ModCore.Instance.Wiki);
                }

                if (Find.CurrentMap != null && Current.ProgramState == ProgramState.Playing)
                {
                    float points = StorytellerUtility.DefaultThreatPointsNow(Find.CurrentMap);
                    GUILayout.Label("Current map raid points: " + points);
                    GUILayout.Label("Tick rate: " + Find.TickManager.TickRateMultiplier);
                }

                if (Find.CurrentMap != null)
                {
                    var grid = AI_AvoidGrid.Ensure(Find.CurrentMap);
                    GUILayout.Label("Current map avoidance providers: " + grid.AvoidanceProviders.Count);
                }
            }
            catch (Exception e)
            {
                Log.Warning($"GUI draw error: {e}");
            }
        }
    }
}
