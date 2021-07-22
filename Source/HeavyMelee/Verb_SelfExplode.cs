using System.Collections.Generic;
using Verse;

namespace HeavyMelee
{
    public class Verb_SelfExplode : Verb
    {
        private bool forced;

        public override Verb GetVerb
        {
            get
            {
                forced = true;
                return base.GetVerb;
            }
        }

        protected override bool TryCastShot()
        {
            DoExplode();
            return true;
        }

        protected virtual void DoExplode()
        {
            DamageWorker_Flame_30Degrees.ExplosionOriginator = CasterPawn;
            GenExplosion.DoExplosion(caster.Position, caster.Map, verbProps.range, verbProps.meleeDamageDef, caster,
                verbProps.meleeDamageBaseAmount, ignoredThings: new List<Thing> {caster});
        }

        public override bool Available()
        {
            if (!base.Available()) return false;
            if (!forced)
                return CasterIsPawn && (CasterPawn.mindState?.enemyTarget?.Position.AdjacentTo8WayOrInside(Caster.Position) ?? false);
            forced = false;
            return true;
        }
    }
}