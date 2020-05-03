using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Defs
{
    [StaticConstructorOnStartup]
    public class AATurretDef : ThingDef
    {
        public string turretTopTexturePath;
        public float turretTurnSpeed;
        public Vector2 turretTopSize;
    }
}
