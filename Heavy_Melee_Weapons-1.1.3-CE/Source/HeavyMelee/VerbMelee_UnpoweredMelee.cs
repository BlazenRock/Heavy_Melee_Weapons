using Reloading;
using RimWorld;
using Verse;

namespace HeavyMelee
{
    public class Verb_UnpoweredMelee : Verb_MeleeAttackDamage, IReloadingVerb
    {
        public const int shotConsumption = 6;

        public IReloadable Reloadable
        {
            get
            {
                IReloadable r = null;
                bool flag;
                if (EquipmentSource != null)
                {
                    r = EquipmentSource.AllComps.FirstOrFallback(comp => comp is IReloadable) as IReloadable;
                    flag = r != null;
                }
                else
                {
                    flag = false;
                }

                var flag2 = flag;
                IReloadable result;
                if (flag2)
                {
                    result = r;
                }
                else
                {
                    var hediffCompSource = HediffCompSource;
                    IReloadable r2 = null;
                    bool flag3;
                    if ((hediffCompSource != null ? hediffCompSource.parent : null) != null)
                    {
                        r2 = HediffCompSource.parent.comps.FirstOrFallback(comp => comp is IReloadable) as IReloadable;
                        flag3 = r2 != null;
                    }
                    else
                    {
                        flag3 = false;
                    }

                    var flag4 = flag3;
                    if (flag4)
                        result = r2;
                    else
                        result = null;
                }

                return result;
            }
        }

        public override bool Available()
        {
            var reloadable = Reloadable;
            return reloadable != null && reloadable.ShotsRemaining < shotConsumption && base.Available();
        }
    }
}