using AntimatterAnnihilation.ThingComps;
using AntimatterAnnihilation.Utils;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace AntimatterAnnihilation.Buildings
{
	[StaticConstructorOnStartup]
	public abstract class Building_AATurret : Building_Turret
    {
        public float ScreenShakeOnShoot { get; set; } = 0f;
        public bool HasLOS
        {
            get
            {
				if (!CurrentTarget.IsValid)
					return false;

                bool flag = AttackVerb.TryFindShootLineFromTo(this.Position, CurrentTarget, out _);
                return flag;
            }
        }
		public bool DoBetterTargetFind { get; protected set; } = true;
        public bool Active
		{
			get
			{
				return (this.powerComp == null || this.powerComp.PowerOn) && (this.dormantComp == null || this.dormantComp.Awake) && (this.initiatableComp == null || this.initiatableComp.Initiated);
			}
		}
        public CompAutoAttack AutoAttack
        {
            get
            {
                return this.GetComp<CompAutoAttack>();
            }
        }
		public bool AutoAttackEnabled
		{
			get
            {
                return AutoAttack == null || AutoAttack.AutoAttackEnabled;
            }
		}
		public CompEquippable GunCompEq
		{
			get
			{
				return this.gun.TryGetComp<CompEquippable>();
			}
		}
        public override LocalTargetInfo CurrentTarget
		{
			get
			{
				return this.currentTargetInt;
			}
		}
        public LocalTargetInfo CurrentOrForcedTarget
		{
			get
            {
                if (forcedTarget.IsValid)
                    return forcedTarget;

                return CurrentTarget;
            }
		}
        public override Verb AttackVerb
		{
			get
			{
				return this.GunCompEq.PrimaryVerb;
			}
		}
        public bool IsMannable
		{
			get
			{
				return this.mannableComp != null;
			}
		}
        public bool IsInBurst
        {
			get
            {
                return isInBurst;
            }
        }

        private bool WarmingUp
		{
			get
			{
				return this.burstWarmupTicksLeft > 0;
			}
		}
        private bool PlayerControlled
		{
			get
			{
				return (base.Faction == Faction.OfPlayer || this.MannedByColonist) && !this.MannedByNonColonist;
			}
		}
        private bool CanSetForcedTarget
		{
			get
			{
				return true;
				//return this.mannableComp != null && this.PlayerControlled;
			}
		}
        private bool CanToggleHoldFire
		{
			get
			{
				return true;
				//return this.PlayerControlled;
			}
		}
        private bool IsMortar
		{
			get
			{
				return this.def.building.IsMortar;
			}
		}
        private bool IsMortarOrProjectileFliesOverhead
		{
			get
			{
				return this.AttackVerb.ProjectileFliesOverhead() || this.IsMortar;
			}
		}
        private bool CanExtractShell
		{
			get
			{
				if (!this.PlayerControlled)
				{
					return false;
				}
				CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
				return compChangeableProjectile != null && compChangeableProjectile.Loaded;
			}
		}
        private bool MannedByColonist
		{
			get
			{
				return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction == Faction.OfPlayer;
			}
		}
        private bool MannedByNonColonist
		{
			get
			{
				return this.mannableComp != null && this.mannableComp.ManningPawn != null && this.mannableComp.ManningPawn.Faction != Faction.OfPlayer;
			}
		}

        private StringBuilder stringBuilder = new StringBuilder();
        private bool isInBurst;

        public Building_AATurret()
        {
			this.top = CreateTop();
        }

        public virtual AATurretTop CreateTop()
        {
			return new AATurretTop(this);
		}

		public override void SpawnSetup(Map map, bool respawningAfterLoad)
		{
			base.SpawnSetup(map, respawningAfterLoad);
			this.dormantComp = base.GetComp<CompCanBeDormant>();
			this.initiatableComp = base.GetComp<CompInitiatable>();
			this.powerComp = base.GetComp<CompPowerTrader>();
			this.mannableComp = base.GetComp<CompMannable>();
			if (!respawningAfterLoad)
			{
				this.top.SetRotationFromOrientation(); // Puts the turret top facing the same direction as rotation. Not really necessary.
				this.burstCooldownTicksLeft = this.def.building.turretInitialCooldownTime.SecondsToTicks();
			}
        }

		public override void PostMake()
		{
			base.PostMake();
			this.MakeGun();
		}

		public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
		{
			base.DeSpawn(mode);
			this.ResetCurrentTarget();
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref this.burstCooldownTicksLeft, "burstCooldownTicksLeft", 0, false);
			Scribe_Values.Look(ref this.burstWarmupTicksLeft, "burstWarmupTicksLeft", 0, false);

            Scribe_Values.Look(ref this.top.curRotationInt, "aa_turretTopRot");
            Scribe_Values.Look(ref isInBurst, "aa_isInBurst");

            Scribe_TargetInfo.Look(ref this.currentTargetInt, "currentTarget");
			Scribe_Values.Look(ref this.holdFire, "holdFire", false, false);
			Scribe_Deep.Look(ref this.gun, "gun", Array.Empty<object>());
			BackCompatibility.PostExposeData(this);
			if (Scribe.mode == LoadSaveMode.PostLoadInit)
			{
				this.UpdateGunVerbs();
			}

            if (top != null)
                top.ExposeData();
            else
                Log.Error("Could not expose data for turret top: turret top is null.");
        }
		
		public override bool ClaimableBy(Faction by, StringBuilder reason = null)
		{
			return base.ClaimableBy(by, reason) && (this.mannableComp == null || this.mannableComp.ManningPawn == null) && (!this.Active || this.mannableComp != null) && (((this.dormantComp == null || this.dormantComp.Awake) && (this.initiatableComp == null || this.initiatableComp.Initiated)) || (this.powerComp != null && !this.powerComp.PowerOn));
		}

        public override void OrderAttack(LocalTargetInfo targ)
		{
			if (!targ.IsValid)
			{
				if (this.forcedTarget.IsValid)
				{
					this.ResetForcedTarget();
				}
				return;
			}
			if ((targ.Cell - base.Position).LengthHorizontal < this.AttackVerb.verbProps.EffectiveMinRange(targ, this))
			{
				Messages.Message("MessageTargetBelowMinimumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
				return;
			}
			if ((targ.Cell - base.Position).LengthHorizontal > this.AttackVerb.verbProps.range)
			{
				Messages.Message("MessageTargetBeyondMaximumRange".Translate(), this, MessageTypeDefOf.RejectInput, false);
				return;
			}

			this.forcedTarget = targ;

            if (this.holdFire)
			{
				Messages.Message("MessageTurretWontFireBecauseHoldFire".Translate(this.def.label), this, MessageTypeDefOf.RejectInput, false);
			}
		}

		public override void Tick()
		{
			base.Tick();

			// This stuff is only used by mortars - currently not needed for Antimatter Annihilation but I'll leave it in just in case.
			if (this.CanExtractShell && this.MannedByColonist)
			{
				CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
				if (!compChangeableProjectile.allowedShellsSettings.AllowedToAccept(compChangeableProjectile.LoadedShell))
				{
					this.ExtractShell();
				}
			}

			if (this.forcedTarget.IsValid && !this.CanSetForcedTarget)
			{
				this.ResetForcedTarget();
			}
			if (!this.CanToggleHoldFire)
			{
				this.holdFire = false;
			}
			if (this.forcedTarget.ThingDestroyed)
			{
				this.ResetForcedTarget();
			}

            if (this.Active && (this.mannableComp == null || this.mannableComp.MannedNow) && base.Spawned)
            {
                this.GunCompEq.verbTracker.VerbsTick();

				if(!this.IsStunned)
                    this.top.Tick();

                if (!this.IsStunned && this.AttackVerb.state != VerbState.Bursting)
                {
                    if (this.WarmingUp)
                    {
                        this.burstWarmupTicksLeft--;

                        if (this.burstWarmupTicksLeft <= 0)
                        {
                            if (top.CanShootNow())
								this.BeginBurst();
                        }
                    }
                    else
                    {
                        if (this.burstCooldownTicksLeft > 0)
                        {
                            this.burstCooldownTicksLeft--;
                        }
                        if (this.burstCooldownTicksLeft <= 0 && this.IsHashIntervalTick(10))
                        {
                            this.TryStartShootSomething(true);
                        }
                    }
                    return;
                }
            }
            else
            {
                this.ResetCurrentTarget();
            }
		}

		public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
		{
            if (dinfo.Def == AADefOf.AnnihilationExplosionRailgun_AA || dinfo.Def == AADefOf.AnnihilationExplosion_AA)
            {
                //Log.Message($"This {this.LabelCap} took Annihilate explosion damage, reducing to 50%");
                dinfo.SetAmount(dinfo.Amount * 0.5f);
            }

			base.PreApplyDamage(ref dinfo, out absorbed);
		}

		protected void TryStartShootSomething(bool canBeginBurstImmediately)
		{
            if (!base.Spawned || (this.holdFire && this.CanToggleHoldFire) || (this.AttackVerb.ProjectileFliesOverhead() && base.Map.roofGrid.Roofed(base.Position)) || !this.AttackVerb.Available())
            {
                this.ResetCurrentTarget();
                return;
            }
            bool isValid = this.currentTargetInt.IsValid;
            if (this.forcedTarget.IsValid)
            {
                this.currentTargetInt = this.forcedTarget;
            }
            else
            {
				bool allowedToFindNew = true;
                if (DoBetterTargetFind)
                {
					Thing thing = currentTargetInt.Thing;
					// Allowed to find new target if current target is invalid, destroyed or downed, or line of sight is gone.
					allowedToFindNew = !isValid || (thing != null && thing.Destroyed) || (thing is Pawn pawn && pawn.Downed) || !HasLOS;
				}

                if (allowedToFindNew && AutoAttackEnabled)
                    this.currentTargetInt = this.TryFindNewTarget();
            }
            if (!isValid && this.currentTargetInt.IsValid)
            {
                SoundDefOf.TurretAcquireTarget.PlayOneShot(new TargetInfo(base.Position, base.Map, false));
            }
            if (!this.currentTargetInt.IsValid)
            {
                this.ResetCurrentTarget();
                return;
            }
            float randomInRange = this.def.building.turretBurstWarmupTime.RandomInRange;
            if (randomInRange > 0f)
            {
                this.burstWarmupTicksLeft = randomInRange.SecondsToTicks();
                return;
            }

            if (canBeginBurstImmediately)
            {
                if(top.CanShootNow())
                    this.BeginBurst();
                return;
            }
            this.burstWarmupTicksLeft = 1;
		}

        protected LocalTargetInfo TryFindNewTarget()
		{
			IAttackTargetSearcher attackTargetSearcher = this.TargSearcher();
			Faction faction = attackTargetSearcher.Thing.Faction;
			float range = this.AttackVerb.verbProps.range;
			Building t;
			if (Rand.Value < 0.5f && this.AttackVerb.ProjectileFliesOverhead() && faction.HostileTo(Faction.OfPlayer) && base.Map.listerBuildings.allBuildingsColonist.Where(delegate (Building x)
			{
				float num = this.AttackVerb.verbProps.EffectiveMinRange(x, this);
				float num2 = (float)x.Position.DistanceToSquared(this.Position);
				return num2 > num * num && num2 < range * range;
			}).TryRandomElement(out t))
			{
				return t;
			}
			TargetScanFlags targetScanFlags = TargetScanFlags.NeedThreat | TargetScanFlags.NeedAutoTargetable;
			if (!this.AttackVerb.ProjectileFliesOverhead())
			{
				targetScanFlags |= TargetScanFlags.NeedLOSToAll;
				targetScanFlags |= TargetScanFlags.LOSBlockableByGas;
			}
            if (this.AttackVerb.IsIncendiary_Ranged())
            {
                targetScanFlags |= TargetScanFlags.NeedNonBurning;
            }
            return (Thing)AttackTargetFinder.BestShootTargetFromCurrentPosition(attackTargetSearcher, targetScanFlags, new Predicate<Thing>(this.IsValidTarget), 0f, 9999f);
		}

		private IAttackTargetSearcher TargSearcher()
		{
			if (this.mannableComp != null && this.mannableComp.MannedNow)
			{
				return this.mannableComp.ManningPawn;
			}
			return this;
		}

		private bool IsValidTarget(Thing t)
		{
			Pawn pawn = t as Pawn;
			if (pawn != null)
			{
				if (this.AttackVerb.ProjectileFliesOverhead())
				{
					RoofDef roofDef = base.Map.roofGrid.RoofAt(t.Position);
					if (roofDef != null && roofDef.isThickRoof)
					{
						return false;
					}
				}
				if (this.mannableComp == null)
				{
					return !GenAI.MachinesLike(base.Faction, pawn);
				}
				if (pawn.RaceProps.Animal && pawn.Faction == Faction.OfPlayer)
				{
					return false;
				}
			}
			return true;
		}

		protected void BeginBurst()
		{
			this.AttackVerb.TryStartCastOn(this.CurrentTarget, false, true);
			base.OnAttackedTarget(this.CurrentTarget);
            //if (isInBurst)
            //    Log.Error("BeginBurst() but isInBurst was true?!");
			isInBurst = true;
        }

		protected void BurstComplete()
		{
			this.burstCooldownTicksLeft = this.BurstCooldownTime().SecondsToTicks();
            //if (!isInBurst)
            //    Log.Error("BurstComplete() but isInBurst was false?!");

			isInBurst = false;
		}

		protected float BurstCooldownTime()
		{
			if (this.def.building.turretBurstCooldownTime >= 0f)
			{
				return this.def.building.turretBurstCooldownTime;
			}
			return this.AttackVerb.verbProps.defaultCooldownTime;
		}

		public override string GetInspectString()
        {
            stringBuilder.Clear();

			string inspectString = base.GetInspectString();
			if (!inspectString.NullOrEmpty())
			{
				stringBuilder.AppendLine(inspectString);
			}

			// Not necessary IMHO, the visual gizmo already shows it quite clearly.
			//if (this.AttackVerb.verbProps.minRange > 0f)
			//{
			//	stringBuilder.AppendLine("MinimumRange".Translate() + ": " + this.AttackVerb.verbProps.minRange.ToString("F0"));
			//}
			if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
			{
				stringBuilder.AppendLine("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
			}

            stringBuilder.AppendLine("CanFireIn".Translate() + $": {this.burstCooldownTicksLeft.TicksToSeconds():F1} seconds.");

            if (Prefs.DevMode)
            {
                var target = CurrentOrForcedTarget;
                stringBuilder.Append("Target: ");
                stringBuilder.AppendLine($"{(target.IsValid ? "valid" : "invalid")}, {target.Label}: {target.ToString()}");

                stringBuilder.Append("Top can shoot: ");
                stringBuilder.AppendLine(top.CanShootNow().ToString());

                stringBuilder.Append("Auto attack: ");
                stringBuilder.AppendLine(AutoAttackEnabled.ToString());
            }

            CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
			if (compChangeableProjectile != null)
			{
				if (compChangeableProjectile.Loaded)
				{
					stringBuilder.AppendLine("ShellLoaded".Translate(compChangeableProjectile.LoadedShell.LabelCap, compChangeableProjectile.LoadedShell));
				}
				else
				{
					stringBuilder.AppendLine("ShellNotLoaded".Translate());
				}
			}
			return stringBuilder.ToString().TrimEndNewlines();
		}

        public override void DynamicDrawPhaseAt(DrawPhase phase, Vector3 drawLoc, bool flip = false)
        {
            base.DynamicDrawPhaseAt(phase, drawLoc, flip);

			if (phase == DrawPhase.Draw)
                top.Draw();

        }

        public override void DrawExtraSelectionOverlays()
		{
			float range = this.AttackVerb.verbProps.range;
			if (range < 90f)
			{
				GenDraw.DrawRadiusRing(base.Position, range);
			}
			float num = this.AttackVerb.verbProps.EffectiveMinRange(true);
			if (num < 90f && num > 0.1f)
			{
				GenDraw.DrawRadiusRing(base.Position, num);
			}
			if (this.WarmingUp)
			{
				int degreesWide = (int)((float)this.burstWarmupTicksLeft * 0.5f);
				GenDraw.DrawAimPie(this, this.CurrentTarget, degreesWide, (float)this.def.size.x * 0.5f);
			}
			if (this.forcedTarget.IsValid && (!this.forcedTarget.HasThing || this.forcedTarget.Thing.Spawned))
			{
				Vector3 vector;
				if (this.forcedTarget.HasThing)
				{
					vector = this.forcedTarget.Thing.TrueCenter();
				}
				else
				{
					vector = this.forcedTarget.Cell.ToVector3Shifted();
				}
				Vector3 a = this.TrueCenter();
				vector.y = AltitudeLayer.MetaOverlays.AltitudeFor();
				a.y = vector.y;
				GenDraw.DrawLineBetween(a, vector, Building_TurretGun.ForcedTargetLineMat);
			}
		}

		public override IEnumerable<Gizmo> GetGizmos()
		{
			// TODO fix up.
			foreach (Gizmo gizmo in base.GetGizmos())
			{
				yield return gizmo;
			}
			if (this.CanExtractShell)
			{
				CompChangeableProjectile compChangeableProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
				yield return new Command_Action
				{
					defaultLabel = "CommandExtractShell".Translate(),
					defaultDesc = "CommandExtractShellDesc".Translate(),
					icon = compChangeableProjectile.LoadedShell.uiIcon,
					iconAngle = compChangeableProjectile.LoadedShell.uiIconAngle,
					iconOffset = compChangeableProjectile.LoadedShell.uiIconOffset,
					iconDrawScale = GenUI.IconDrawScale(compChangeableProjectile.LoadedShell),
					action = this.ExtractShell
				};
			}
			CompChangeableProjectile changeProjectile = this.gun.TryGetComp<CompChangeableProjectile>();
			if (changeProjectile != null)
			{
				StorageSettings storeSettings = changeProjectile.GetStoreSettings();
				foreach (Gizmo gizmo in StorageSettingsClipboard.CopyPasteGizmosFor(storeSettings))
				{
					yield return gizmo;
				}
			}
			if (this.CanSetForcedTarget)
			{
				Command_VerbTarget forceAttack = new Command_VerbTarget();
				forceAttack.defaultLabel = "CommandSetForceAttackTarget".Translate();
				forceAttack.defaultDesc = "CommandSetForceAttackTargetDesc".Translate();
				forceAttack.icon = ContentFinder<Texture2D>.Get("UI/Commands/Attack", true);
				forceAttack.verb = this.AttackVerb;
				forceAttack.hotKey = KeyBindingDefOf.Misc4;
				forceAttack.drawRadius = false;
				if (base.Spawned && this.IsMortarOrProjectileFliesOverhead && base.Position.Roofed(base.Map))
				{
					forceAttack.Disable("CannotFire".Translate() + ": " + "Roofed".Translate().CapitalizeFirst());
				}
				yield return forceAttack;
			}
			if (this.forcedTarget.IsValid)
			{
				Command_Action stopForcedAttack = new Command_Action();
				stopForcedAttack.defaultLabel = "CommandStopForceAttack".Translate();
				stopForcedAttack.defaultDesc = "CommandStopForceAttackDesc".Translate();
				stopForcedAttack.icon = ContentFinder<Texture2D>.Get("UI/Commands/Halt", true);
				stopForcedAttack.action = delegate ()
				{
					this.ResetForcedTarget();
					SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
				};
				if (!this.forcedTarget.IsValid)
				{
					stopForcedAttack.Disable("CommandStopAttackFailNotForceAttacking".Translate());
				}
				stopForcedAttack.hotKey = KeyBindingDefOf.Misc5;
				yield return stopForcedAttack;
			}
			if (this.CanToggleHoldFire)
			{
				yield return new Command_Toggle
				{
					defaultLabel = "CommandHoldFire".Translate(),
					defaultDesc = "CommandHoldFireDesc".Translate(),
					icon = ContentFinder<Texture2D>.Get("UI/Commands/HoldFire", true),
					hotKey = KeyBindingDefOf.Misc6,
					toggleAction = delegate ()
					{
						this.holdFire = !this.holdFire;
						if (this.holdFire)
						{
							this.ResetForcedTarget();
						}
					},
					isActive = (() => this.holdFire)
				};
			}
		}

		private void ExtractShell()
		{
			GenPlace.TryPlaceThing(this.gun.TryGetComp<CompChangeableProjectile>().RemoveShell(), base.Position, base.Map, ThingPlaceMode.Near, null, null, default(Rot4));
		}

		private void ResetForcedTarget()
		{
			this.forcedTarget = LocalTargetInfo.Invalid;
            this.currentTargetInt = LocalTargetInfo.Invalid; // Added to fix railgun still firing after stopping forced attack.
			this.burstWarmupTicksLeft = 0;
			if (this.burstCooldownTicksLeft <= 0)
			{
				this.TryStartShootSomething(false);
			}

		}

		private void ResetCurrentTarget()
		{
			this.currentTargetInt = LocalTargetInfo.Invalid;
			this.burstWarmupTicksLeft = 0;
		}

		public void MakeGun()
		{
			this.gun = ThingMaker.MakeThing(this.def.building.turretGunDef, null);
			this.UpdateGunVerbs();
		}

		private void UpdateGunVerbs()
		{
			List<Verb> allVerbs = this.gun.TryGetComp<CompEquippable>().AllVerbs;
			for (int i = 0; i < allVerbs.Count; i++)
			{
				Verb verb = allVerbs[i];
				verb.caster = this;
				verb.castCompleteCallback = new Action(this.BurstComplete);
			}
		}

        public Thing gun;
		public AATurretTop top;

		protected LocalTargetInfo currentTargetInt = LocalTargetInfo.Invalid;
        protected CompPowerTrader powerComp;
        protected CompCanBeDormant dormantComp;
        protected CompInitiatable initiatableComp;
		protected CompMannable mannableComp;
		protected int burstWarmupTicksLeft;
		protected int burstCooldownTicksLeft;

		private bool holdFire;
    }
}
