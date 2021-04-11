using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using Verse.Sound;
using UnityEngine;

namespace HeavyMelee {

    [DefOf]
    public static class GravityLanceDefOf{
        public static ThingDef PlantedGravityLance;
    }

    public class CompProperties_GravityLanceExplode : CompProperties {
        public CompProperties_GravityLanceExplode():base(){
            this.compClass = typeof(Comp_GravityLanceExplode);
        }
        public int explosionAfterTicks = 0;
        public SoundDef countDownSoundDef;
        public SoundDef countDownSoundDef4Sec;
        public SoundDef impactSoundDef;
        public float explosionRadius;
        public DamageDef damageDef;
        public int damageAmount;
        public float armourPenetration;
    }

    public class Comp_GravityLanceExplode : ThingComp{
        public CompProperties_GravityLanceExplode Props{
            get{
                return (CompProperties_GravityLanceExplode)props;
            }
        }

        public override void PostSpawnSetup(bool respawningAfterLoad) {
            base.PostSpawnSetup(respawningAfterLoad);
            if(!respawningAfterLoad){
                ticksTillDetonation = Props.explosionAfterTicks;
            }
        }

        public override void CompTick() {
            base.CompTick();
            ticksTillDetonation -= 1;
            if(ticksTillDetonation == 0){
			    GenExplosion.DoExplosion(
                    parent.Position, 
                    parent.Map, 
                    Props.explosionRadius, 
                    Props.damageDef, 
                    parent,
                    Props.damageAmount, 
                    Props.armourPenetration, 
                    null, null, null, null, null, 0f, 1, false, null, 0f, 1, 0f, false, null, null);
                Props.impactSoundDef.PlayOneShot(
                    SoundInfo.InMap(new TargetInfo(parent.Position, parent.Map, false), MaintenanceType.None)
                );
                parent.Destroy();
            }else{
                if(ticksTillDetonation <= 240){
                    if(ticksTillDetonation == 240){
                        Props.countDownSoundDef4Sec.PlayOneShot(
                            SoundInfo.InMap(new TargetInfo(parent.Position, parent.Map, false), MaintenanceType.None)
                        );
                    }
                }else if((ticksTillDetonation - 240) % 60 == 0){
                    Props.countDownSoundDef.PlayOneShot(
                        SoundInfo.InMap(new TargetInfo(parent.Position, parent.Map, false), MaintenanceType.None)
                    );
                }
            }
        }
        public override void PostExposeData(){
            base.PostExposeData();
            Scribe_Values.Look<int>(ref ticksTillDetonation, "ticksTillDetonation");
        }

        public int ticksTillDetonation = -1;
    }

}
