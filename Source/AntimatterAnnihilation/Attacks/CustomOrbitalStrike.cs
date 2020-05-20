using AntimatterAnnihilation.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace AntimatterAnnihilation.Attacks
{
	/// <summary>
	/// Re-implementation of PowerBeam.cs class from vanilla with flexible parameters.
	/// I don't like ripping large chunks of code like this but honestly there isn't really any point in re-writing what already works perfectly in vanilla.
	/// </summary>
	public class CustomOrbitalStrike : OrbitalStrike
	{
        private static List<Thing> tmpThings = new List<Thing>();
		private static FieldInfo fieldInfo;
		private static FieldInfo fieldInfo2;

        public event Action<CustomOrbitalStrike> OnStrikeOver;
        public float Radius = 15f;
        public int UpdatesPerTick = 4;
		public float ArmorPen = 0.35f;
		public DamageDef DamageDef;
        public IntRange DamageAmountRange = new IntRange(65, 100);
        public IntRange CorpseDamageAmountRange = new IntRange(5, 10);

		public override void StartStrike()
		{
			base.StartStrike();
			MakeCustomPowerBeamMote(base.Position, base.Map);

            SetAngle(0);
        }

        public static void MakeCustomPowerBeamMote(IntVec3 cell, Map map)
        {
            Mote mote = (Mote)ThingMaker.MakeThing(AADefOf.Mote_MeguminBeam_AA);
            mote.exactPosition = cell.ToVector3Shifted();
            mote.Scale = 90f;
            mote.rotationRate = 1.2f;
            GenSpawn.Spawn((Thing)mote, cell, map, WipeMode.Vanish);
        }

		public void SetAngle(float a)
        {
            // Use reflection to set angle to 0 (straight down.)
            if (fieldInfo == null)
                fieldInfo = typeof(OrbitalStrike).GetField("angle", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo.SetValue(this, a);

			// Also apply immediately to component.
            if (fieldInfo2 == null)
                fieldInfo2 = typeof(CompOrbitalBeam).GetField("angle", BindingFlags.Instance | BindingFlags.NonPublic);
			fieldInfo2.SetValue(base.GetComp<CompOrbitalBeam>(), a);
		}

		public override void Tick()
		{
			base.Tick();
			if (base.Destroyed)
            {
                OnStrikeOver?.Invoke(this);
                return;
			}
			
			for (int i = 0; i < UpdatesPerTick; i++)
			{
				this.UpdateDamage();
			}
		}

		private void UpdateDamage()
		{
			IntVec3 c = (from x in GenRadial.RadialCellsAround(base.Position, Radius, true)
						 where x.InBounds(base.Map)
						 select x).RandomElementByWeight((IntVec3 x) => 1f - Mathf.Min(x.DistanceTo(base.Position) / Radius, 1f) + 0.05f);

			FireUtility.TryStartFireIn(c, base.Map, Rand.Range(0.1f, 0.925f));
			tmpThings.Clear();
			tmpThings.AddRange(c.GetThingList(base.Map));
			for (int i = 0; i < tmpThings.Count; i++)
			{
				int damage = (tmpThings[i] is Corpse) ? CorpseDamageAmountRange.RandomInRange : DamageAmountRange.RandomInRange;
				Pawn pawn = tmpThings[i] as Pawn;
				BattleLogEntry_DamageTaken log = null;
				if (pawn != null)
				{
					log = new BattleLogEntry_DamageTaken(pawn, RulePackDefOf.DamageEvent_PowerBeam, this.instigator as Pawn);
					Find.BattleLog.Add(log);
				}
				tmpThings[i].TakeDamage(new DamageInfo(DamageDef, (float)damage, ArmorPen, -1f, this.instigator, null, this.weaponDef, DamageInfo.SourceCategory.ThingOrUnknown, null)).AssociateWithLog(log);
			}
			tmpThings.Clear();
		}
    }
}
