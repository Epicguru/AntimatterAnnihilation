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
            GUILayout.Space(120);
            bool spawn = GUILayout.Button("Spawn");

            if (spawn)
            {
                Vector3 pos = new Vector3(15, 0, 15);
                Log.Message("Spawning at " + pos);

                var beam = Object.Instantiate(Content.EnergyBeamPrefab).transform;
                beam.transform.position = pos;
                beam.localScale = Vector3.one;
                beam.eulerAngles = new Vector3(90f, 90f, 0f);
            }
        }
    }
}
