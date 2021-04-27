using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using Reloading;

namespace HeavyMelee {

	public class Verb_PoweredMelee : Verb_MeleeAttackDamage, IReloadingVerb{
		
		protected override bool TryCastShot(){
			if(base.TryCastShot()){
				Reloadable.ShotsRemaining -= shotConsumption;
				return true;
			}
			return false;
		}
		public IReloadable Reloadable
		{
			get
			{
				IReloadable r = null;
				bool flag;
				if (base.EquipmentSource != null)
				{
					r = (base.EquipmentSource.AllComps.FirstOrFallback((ThingComp comp) => comp is IReloadable, null) as IReloadable);
					flag = (r != null);
				}
				else
				{
					flag = false;
				}
				bool flag2 = flag;
				IReloadable result;
				if (flag2)
				{
					result = r;
				}
				else
				{
					HediffComp_VerbGiver hediffCompSource = base.HediffCompSource;
					IReloadable r2 = null;
					bool flag3;
					if (((hediffCompSource != null) ? hediffCompSource.parent : null) != null)
					{
						r2 = (base.HediffCompSource.parent.comps.FirstOrFallback((HediffComp comp) => comp is IReloadable, null) as IReloadable);
						flag3 = (r2 != null);
					}
					else
					{
						flag3 = false;
					}
					bool flag4 = flag3;
					if (flag4)
					{
						result = r2;
					}
					else
					{
						result = null;
					}
				}
				return result;
			}
		}

		public override bool Available(){
			IReloadable reloadable = Reloadable;
			return reloadable != null && reloadable.ShotsRemaining >= shotConsumption && base.Available();
		}

		public const int shotConsumption = 6;
	}
}
