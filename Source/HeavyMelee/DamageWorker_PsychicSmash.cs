using System.Linq;
using RimWorld;
using Verse;

namespace HeavyMelee
{
    public class DamageWorker_PsychicSmash : DamageWorker_Blunt
    {
        public override DamageResult Apply(DamageInfo dinfo, Thing thing)
        {
            var result = base.Apply(dinfo, thing);
            if (thing is Pawn pawn && dinfo.Weapon != null && dinfo.Instigator is Pawn pawn2 && !pawn.Dead &&
                !pawn.Downed)
            {
                var weapon = pawn2.equipment.AllEquipmentListForReading.Concat(pawn2.apparel.WornApparel)
                    .FirstOrDefault(t => t.def == dinfo.Weapon);
                var comp = weapon.TryGetComp<CompPsychicShock>();
                if (comp != null && comp.ShockOnNext)
                {
                    var hediff = HediffMaker.MakeHediff(HediffDefOf.PsychicShock, pawn);
                    pawn.RaceProps.body.GetPartsWithTag(BodyPartTagDefOf.ConsciousnessSource)
                        .TryRandomElement(out var part);
                    pawn.health.AddHediff(hediff, part, null);
                    comp.ShockOnNext = false;
                }
            }

            return result;
        }
    }

    public class CompProperties_PsychicShock : CompProperties
    {
        public float PsyfocusCost;

        public CompProperties_PsychicShock()
        {
            compClass = typeof(CompPsychicShock);
        }
    }

    public class CompPsychicShock : ThingComp
    {
        public bool ShockOnNext;
        public CompProperties_PsychicShock Props => props as CompProperties_PsychicShock;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref ShockOnNext, "ShockOnNext");
        }
    }
}