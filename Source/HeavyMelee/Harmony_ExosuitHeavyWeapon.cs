using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Reflection.Emit;
using Verse.AI;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HeavyWeapons;

namespace HeavyMelee
{
	[StaticConstructorOnStartup]
    public static class Harmony_ExosuitHeavyWeapon
    {
		public static HashSet<string> SA_HeavyWeaponHediffString = new HashSet<string>();
		public static HashSet<HediffDef> SA_HeavyWeaponableHediffDefs = new HashSet<HediffDef>();
		public static HashSet<string> SA_HeavyWeaponThingString = new HashSet<string>();
        public static HashSet<ThingDef> SA_HeavyWeaponThingDefs = new HashSet<ThingDef>();
		public static HashSet<HeavyWeapon> SA_HeavyWeaponExtentionInstances = new HashSet<HeavyWeapon>();//each of the heavy weapon has a unique instance of this class, so this can be used to keep track of which weapon
        static Harmony_ExosuitHeavyWeapon()
        {
            SA_HeavyWeaponHediffString.Add("ExoskeletonSuit");
            SA_HeavyWeaponHediffString.Add("Trunken_hediff_ExoskeletonArmor");
            SA_HeavyWeaponHediffString.Add("FSFImplantTorsoWorker");
            SA_HeavyWeaponHediffString.Add("FSFImplantTorsoSpeed");
            SA_HeavyWeaponHediffString.Add("FSFImplantTorsoPsychic");
            foreach(string str in SA_HeavyWeaponHediffString){
                HediffDef hdd = DefDatabase<HediffDef>.GetNamed(str, false);
                SA_HeavyWeaponableHediffDefs.Add(hdd);
            }

            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_HeavyMonoSword");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_FlameLance");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_PsychicWarhammer");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_ZeusSword");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_RocketHammer");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_SagittariusMight");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_PersonaHeavyMonoSword");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_PersonaFlameLance");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_PersonaPsychicWarhammer");
            SA_HeavyWeaponThingString.Add("HMW_MeleeWeapon_PersonaZeusSword");
            //SA_HeavyWeaponThingString.Add("HMW_GuardianShield");
            
            foreach(string str in SA_HeavyWeaponThingString){
                ThingDef hdd = DefDatabase<ThingDef>.GetNamed(str, false);
                if(hdd == null){
                    //Log.Warning("Could not find the def of " + str);
                    continue;
                }
                SA_HeavyWeaponThingDefs.Add(hdd);
                HeavyWeapon HW = hdd.GetModExtension<HeavyWeapon>();
                if(HW == null){
                    //Log.Warning(str + " has no HeavyWeapon extention !");
                }else{
                    SA_HeavyWeaponExtentionInstances.Add(HW);
                }
            }

            HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("ExoHW.ExosuitHeavyWeapon");
            //harmony.PatchAll();
			
			harmony.Patch(
                AccessTools.Method(typeof(Patch_FloatMenuMakerMap.AddHumanlikeOrders_Fix), "CanEquip"),
				null,
                new HarmonyMethod(typeof(Harmony_ExosuitHeavyWeapon), nameof(CanEquipPostFix)),
                null);
            
			harmony.Patch(
                AccessTools.Method(typeof(Pawn_EquipmentTracker), "GetGizmos"),
				null,
                new HarmonyMethod(typeof(Harmony_ExosuitHeavyWeapon), nameof(GetExtraEquipmentGizmosPassThrough)),
                null);
        }
		public static IEnumerable<Gizmo> GetExtraEquipmentGizmosPassThrough(IEnumerable<Gizmo> values, Pawn_EquipmentTracker __instance)
		{
			foreach(Gizmo giz in values){
				yield return giz;
			}
			if(PawnAttackGizmoUtility.CanShowEquipmentGizmos() && __instance.pawn.IsColonistPlayerControlled){
				List<ThingWithComps> list = __instance.AllEquipmentListForReading;
				for (int i = 0; i < list.Count; i++){
					ThingWithComps eq = list[i];
                    if(eq.def.GetModExtension<SagittariusMightPlantModExtention>() is SagittariusMightPlantModExtention samip){
                        yield return new Command_Action {
                            defaultLabel = samip.label,
                            defaultDesc = samip.description,
                            icon = ContentFinder<Texture2D>.Get(samip.texPath, true),
                            action = delegate (){
                                Thing bb = ThingMaker.MakeThing(GravityLanceDefOf.PlantedGravityLance, null);
                                GenSpawn.Spawn(bb, __instance.pawn.Position, __instance.pawn.Map);
                                eq.Destroy();
                            }
                        };
                    }
                }
			}
			yield break;
		}

        public static void CanEquipPostFix(Pawn pawn, HeavyWeapon options, ref bool __result) {
            if (!__result && SA_HeavyWeaponExtentionInstances.Contains(options)) {
                if (pawn != null && pawn.health != null) {
                    foreach(Hediff hed in pawn.health.hediffSet.hediffs){
                        if(SA_HeavyWeaponableHediffDefs.Contains(hed.def)){
                            __result = true;
                            return;
                        }
                    }
                }
            }

        }
    }
}