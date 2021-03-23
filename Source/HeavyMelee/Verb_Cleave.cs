using RimWorld;
using Verse;

namespace HeavyMelee
{
    public class Verb_Cleave : Verb, IVerbTick
    {
        private int cooldownTicksLeft;

        public void Tick()
        {
            cooldownTicksLeft--;
        }

        protected override bool TryCastShot()
        {
            if (cooldownTicksLeft > 0) return false;
            foreach (var thing in GenRadial.RadialDistinctThingsAround(Caster.Position, Caster.Map, verbProps.range,
                false)) ApplyDamage(thing);
            cooldownTicksLeft = 12f.SecondsToTicks();
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
    }
}