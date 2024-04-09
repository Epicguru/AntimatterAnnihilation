using UnityEngine;

namespace AntimatterAnnihilation
{
    public class Hook : MonoBehaviour
    {
        public static Hook Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }
    }
}
