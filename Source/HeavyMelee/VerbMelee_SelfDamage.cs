using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;

namespace HeavyMelee {
	public class Verb_SelfDamagingMelee : Verb_MeleeAttackDamage{
		protected override bool TryCastShot(){
			if(base.TryCastShot()){
				EquipmentSource.HitPoints -= EquipmentSource.def.GetModExtension<SelfDamageModExtension>().selfDamageAmountPerAttack;
				return true;
			}
			return false;
		}

	}
}
