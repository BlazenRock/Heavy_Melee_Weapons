using RimWorld;
using UnityEngine;
using Verse;

namespace HeavyMelee
{
    public class Verb_Cleave : Verb, IVerbTick, IVerbCustomCommand, IVerbCooldown
    {
        public const float COOLDOWN = 12f;
        private int cooldownTicksLeft;
        public float CooldownPercentLeft => cooldownTicksLeft / (float) COOLDOWN.SecondsToTicks();

        public Command_VerbTarget GetTargetCommand(Command_VerbTarget old)
        {
            return new Command_VerbTargetCooldown(old);
        }

        public void Tick()
        {
            if (cooldownTicksLeft > 0)
                cooldownTicksLeft--;
        }

        protected override bool TryCastShot()
        {
            if (cooldownTicksLeft > 0) return false;
            foreach (var thing in GenRadial.RadialDistinctThingsAround(Caster.Position, Caster.Map, verbProps.range,
                false)) ApplyDamage(thing);
            cooldownTicksLeft = COOLDOWN.SecondsToTicks();
            return true;
        }

        public override bool Available()
        {
            return cooldownTicksLeft <= 0 && base.Available();
        }

        private void ApplyDamage(Thing target)
        {
            var damageInfo = new DamageInfo(verbProps.meleeDamageDef, verbProps.meleeDamageBaseAmount,
                verbProps.meleeArmorPenetrationBase, -1f, caster, null,
                EquipmentSource != null ? EquipmentSource.def : CasterPawn.def);
            damageInfo.SetBodyRegion(BodyPartHeight.Undefined, BodyPartDepth.Outside);
            if (HediffCompSource != null) damageInfo.SetWeaponHediff(HediffCompSource.Def);
            damageInfo.SetAngle((target.Position - CasterPawn.Position).ToVector3());
            var log = new BattleLogEntry_MeleeCombat(RulePackDef.Named("Maneuver_Slash_MeleeHit"), false, CasterPawn,
                target, ImplementOwnerType, ReportLabel, EquipmentSource?.def, HediffCompSource?.Def,
                LogEntryDefOf.MeleeAttack);
            target.TakeDamage(damageInfo).AssociateWithLog(log);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref cooldownTicksLeft, "cooldownTicksLeft_Cleave");
        }
    }

    public interface IVerbCooldown
    {
        float CooldownPercentLeft { get; }
    }

    [StaticConstructorOnStartup]
    public class Command_VerbTargetCooldown : Command_VerbTarget
    {
        public static readonly Texture2D CooldownTex =
            SolidColorMaterials.NewSolidColorTexture(new Color(1f, 1f, 1f, 0.1f));

        public Command_VerbTargetCooldown(Command_VerbTarget old)
        {
            defaultLabel = old.defaultLabel;
            defaultDesc = old.defaultDesc;
            icon = old.icon;
            iconAngle = old.iconAngle;
            iconOffset = old.iconOffset;
            tutorTag = old.tutorTag;
            verb = old.verb;
        }

        private IVerbCooldown Cooldown => verb as IVerbCooldown;

        protected override GizmoResult GizmoOnGUIInt(Rect butRect, GizmoRenderParms parms)
        {
            disabled = Cooldown.CooldownPercentLeft != 0f;
            disabledReason = disabled ? (string) "HeavyMelee.OnCooldown".Translate() : null;

            var result = base.GizmoOnGUIInt(butRect, parms);

            if (disabled)
                GUI.DrawTexture(butRect.RightPartPixels(butRect.width * Cooldown.CooldownPercentLeft), CooldownTex);

            return result;
        }
    }
}