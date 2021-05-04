using System.Collections.Generic;
using Reloading;
using Verse;

namespace HeavyMelee
{
    public class Verb_ReloadableSelfExplode : Verb, IReloadingVerb
    {
        public IReloadable Reloadable
        {
            get
            {
                if (EquipmentSource != null &&
                    EquipmentSource.AllComps.FirstOrFallback(comp => comp is IReloadable) is IReloadable r1)
                    return r1;

                if (HediffCompSource?.parent != null &&
                    HediffCompSource.parent.comps.FirstOrFallback(comp => comp is IReloadable) is IReloadable r2
                )
                    return r2;

                return null;
            }
        }

        protected override bool TryCastShot()
        {
            if (Reloadable != null && Reloadable.ShotsRemaining == 0) return false;
            DoExplode();
            if (Reloadable != null) Reloadable.ShotsRemaining--;
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
            if (Reloadable == null) return false;
            return Reloadable.ShotsRemaining > 0 && base.Available();
        }
    }
}