using Reloading;
using RimWorld;
using Verse;

namespace HeavyMelee
{
    public static class MeleeVerbs
    {
        public const int SHOT_CONSUMPTION = 6;
    }

    public class Verb_PoweredMelee : Verb_MeleeAttackDamage
    {
        public IReloadable Reloadable
        {
            get
            {
                if (EquipmentSource?.AllComps.FirstOrFallback(comp => comp is IReloadable) is IReloadable r) return r;

                if (HediffCompSource?.parent?.comps.FirstOrFallback(comp => comp is IReloadable) is IReloadable r2) return r2;

                return null;
            }
        }

        protected override bool TryCastShot()
        {
            if (Reloadable == null || Reloadable.ShotsRemaining <= 0) return false;
            if (!base.TryCastShot()) return false;
            Reloadable.ShotsRemaining -= MeleeVerbs.SHOT_CONSUMPTION;
            return true;
        }

        public override bool Available()
        {
            return Reloadable != null && Reloadable.ShotsRemaining >= MeleeVerbs.SHOT_CONSUMPTION && base.Available();
        }
    }

    public class Verb_UnpoweredMelee : Verb_MeleeAttackDamage
    {
        public IReloadable Reloadable
        {
            get
            {
                if (EquipmentSource?.AllComps.FirstOrFallback(comp => comp is IReloadable) is IReloadable r) return r;

                if (HediffCompSource?.parent?.comps.FirstOrFallback(comp => comp is IReloadable) is IReloadable r2) return r2;

                return null;
            }
        }

        public override bool Available()
        {
            var reloadable = Reloadable;
            return reloadable != null && reloadable.ShotsRemaining < MeleeVerbs.SHOT_CONSUMPTION && base.Available();
        }
    }
}