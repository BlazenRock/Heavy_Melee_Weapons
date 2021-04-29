using RimWorld;
using Verse;

namespace HeavyMelee
{
    public class Verb_MeleeCleave : Verb_MeleeAttackDamage
    {
        protected override DamageWorker.DamageResult ApplyMeleeDamageToTarget(LocalTargetInfo target)
        {
            var res = base.ApplyMeleeDamageToTarget(target);
            foreach (var thing in GenRadial.RadialDistinctThingsAround(caster.Position, caster.Map, 1.9f, false))
            {
                var log = new BattleLogEntry_MeleeCombat(maneuver.combatLogRulesHit, true, CasterPawn, thing,
                    ImplementOwnerType, tool.labelUsedInLogging ? tool.label : "",
                    EquipmentSource?.def,
                    HediffCompSource?.Def, maneuver.logEntryDef);
                Find.BattleLog.Add(log);
                base.ApplyMeleeDamageToTarget(thing).AssociateWithLog(log);
            }

            return res;
        }

        public override void DrawHighlight(LocalTargetInfo target)
        {
            base.DrawHighlight(target);
            GenDraw.DrawRadiusRing(caster.Position, 1.9f);
        }
    }
}