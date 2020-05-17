using InGameWiki;
using RimWorld;
using System;
using UnityEngine;
using Verse;
using Object = UnityEngine.Object;

namespace AntimatterAnnihilation
{
    public class Hook : MonoBehaviour
    {
        public static Hook Instance { get; private set; }

        private ModWiki wiki;

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
                    Log.Message("Spawning at " + pos);

                    var beam = Object.Instantiate(Content.EnergyBeamInPrefab).transform;
                    beam.transform.position = pos;
                    beam.localScale = Vector3.one;
                    beam.eulerAngles = new Vector3(90f, 90f, 0f);
                }

                if (openWiki)
                {
                    if (wiki == null)
                    {
                        wiki = ModWiki.Create(ModCore.Instance);
                        wiki.WikiTitle = "Antimatter Annihilation";
                    }

                    WikiWindow.Open(this.wiki);
                }

                if (Find.CurrentMap != null)
                {
                    float points = StorytellerUtility.DefaultThreatPointsNow(Find.CurrentMap);
                    GUILayout.Label("Current map raid points: " + points);
                }
            }
            catch (Exception e)
            {
                Log.Warning($"GUI draw error: {e}");
            }
        }
    }
}
