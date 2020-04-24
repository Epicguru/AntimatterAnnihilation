using UnityEngine;

namespace AntimatterAnnihilation
{
    public class Hook : MonoBehaviour
    {
        public static Hook Instance { get; private set; }
        public static float X, Y, Z;

        private string xString, yString, zString;

        private void Awake()
        {
            Instance = this;
        }

        private void OnGUI()
        {
            GUILayout.Space(300);
            xString = GUILayout.TextField(xString);
            yString = GUILayout.TextField(yString);
            zString = GUILayout.TextField(zString);

            float.TryParse(xString, out X);
            float.TryParse(yString, out Y);
            float.TryParse(zString, out Z);

            if (GUILayout.Button("Spawn"))
            {
                var spawned = Object.Instantiate(Content.Prefab);
                spawned.transform.position = new Vector3(X, Z, Y);
                ModCore.Trace($"Spawned at {spawned.transform.position} (swapped Y and Z)");
            }
        }
    }
}
