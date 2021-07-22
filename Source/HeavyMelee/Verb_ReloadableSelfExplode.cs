using System.Collections.Generic;
using Verse;

namespace HeavyMelee
{
    public class Verb_SelfExplode : Verb
    {
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
    }
}