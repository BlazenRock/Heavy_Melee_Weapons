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
				if(EquipmentSource.HitPoints <= 0){
                    Thing bb = ThingMaker.MakeThing(GravityLanceDefOf.PlantedGravityLance, null);
                    GenSpawn.Spawn(bb, CasterPawn.Position, CasterPawn.Map);
                    EquipmentSource.Destroy();
				}
				return true;
			}
			return false;
		}

	}
}
