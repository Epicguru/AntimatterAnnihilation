using AntimatterAnnihilation.Verbs;
using RimWorld;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Buildings
{
    public class AATurretTop
    {
        public float RotationSpeed { get; set; } = 45f;
        public float MaxDeltaToFire { get; set; } = 3f;
        public float DeltaToTarget
        {
            get
            {
                return Mathf.DeltaAngle(CurrentRotation, AngleToTarget);
            }
        }
        public float AngleToTarget
        {
            get
            {
                return (parentTurret.CurrentOrForcedTarget.Cell.ToVector3Shifted() - parentTurret.DrawPos).AngleFlat();
            }
        }

        private float CurrentRotation
        {
            get
            {
                return this.curRotationInt;
            }
            set
            {
                this.curRotationInt = value;
                if (this.curRotationInt > 360f)
                {
                    this.curRotationInt -= 360f;
                }
                if (this.curRotationInt < 0f)
                {
                    this.curRotationInt += 360f;
                }
            }
        }
        private Building_AATurret parentTurret;
        private float curRotationInt;

        public void SetRotationFromOrientation()
        {
            this.CurrentRotation = this.parentTurret.Rotation.AsAngle;
        }

        public AATurretTop(Building_AATurret parentTurret)
        {
            this.parentTurret = parentTurret;
        }

        public void Tick()
        {
            LocalTargetInfo currentTarget = parentTurret.CurrentOrForcedTarget;
            if (currentTarget.IsValid)
            {
                RotateTo(AngleToTarget);
            }
            else
            {
                // Do nothing, just point wherever we were last pointing.
            }
        }

        public void Draw()
        {
            // TODO implement offset and size here instead of current texture-based solution.

            Vector3 b = new Vector3(this.parentTurret.def.building.turretTopOffset.x, 0f, this.parentTurret.def.building.turretTopOffset.y).RotatedBy(this.CurrentRotation);
            float turretTopDrawSize = this.parentTurret.def.building.turretTopDrawSize;
            Matrix4x4 matrix = default(Matrix4x4);
            matrix.SetTRS(this.parentTurret.DrawPos + Altitudes.AltIncVect + b, (this.CurrentRotation + (float)TurretTop.ArtworkRotation).ToQuat(), new Vector3(turretTopDrawSize, 1f, turretTopDrawSize));
            Graphics.DrawMesh(MeshPool.plane10, matrix, this.parentTurret.def.building.turretTopMat, 0);
        }

        /// <summary>
        /// The base turret uses this to check before firing.
        /// Default implementation only allows firing once the turret is rotated towards the correct position.
        /// </summary>
        /// <returns></returns>
        public virtual bool CanShootNow()
        {
            return Mathf.Abs(DeltaToTarget) <= MaxDeltaToFire;
        }

        private void RotateTo(float angle)
        {
            const float DT = 1f / 60f;

            float delta = DeltaToTarget;
            if (delta > 0)
            {
                CurrentRotation += Mathf.Min(delta, DT * RotationSpeed);
            }
            else if (delta < 0)
            {
                CurrentRotation -= Mathf.Min(-delta, DT * RotationSpeed);
            }
        }
    }

    public class PlaceWorker_ShowAATurretRadius : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
        {
            VerbProperties verbProperties = ((ThingDef)checkingDef).building.turretGunDef.Verbs.Find((VerbProperties v) => v.verbClass == typeof(Verb_Railgun));

            // Don't draw the max range, it's too large.
            //if (verbProperties.range > 0f)
            //{
            //    GenDraw.DrawRadiusRing(loc, verbProperties.range);
            //}

            if (verbProperties.minRange > 0f)
            {
                GenDraw.DrawRadiusRing(loc, verbProperties.minRange);
            }
            return true;
        }
    }
}