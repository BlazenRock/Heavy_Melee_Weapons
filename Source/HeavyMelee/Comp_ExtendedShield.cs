using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using VFECore;

namespace HeavyMelee {
    public class CompProperties_ExtendedShield : CompProperties{
        public CompProperties_ExtendedShield(){
            compClass = typeof(Comp_ExtendedShield);
        }

        public string shieldToggleLabel;
        public string shieldToggleDesc;
        public string shieldIcon = "";
        public GraphicData shieldActiveGraphic;

    }
    public class Comp_ExtendedShield : ThingComp {
        public static List<IntVec3>[] DirectionalCheckVector3 = new List<IntVec3>[4];
        public static FieldInfo LandedAccess = AccessTools.DeclaredField(typeof(Projectile), "landed");
        public static MethodInfo ImpactAccess = AccessTools.DeclaredMethod(typeof(Projectile), "Impact");
        public static FieldInfo ShieldGraphic = AccessTools.DeclaredField(typeof(Apparel_Shield), "shieldGraphic");
        static Comp_ExtendedShield(){
            DirectionalCheckVector3[0] = new List<IntVec3>();
            DirectionalCheckVector3[1] = new List<IntVec3>();
            DirectionalCheckVector3[2] = new List<IntVec3>();
            DirectionalCheckVector3[3] = new List<IntVec3>();
            
            DirectionalCheckVector3[0].Add(new IntVec3(0,0,0));
            DirectionalCheckVector3[0].Add(new IntVec3(1,0,0));
            DirectionalCheckVector3[0].Add(new IntVec3(-1,0,0));
            DirectionalCheckVector3[0].Add(new IntVec3(0,0,1));
            DirectionalCheckVector3[0].Add(new IntVec3(1,0,1));
            DirectionalCheckVector3[0].Add(new IntVec3(-1,0,1));
            
            DirectionalCheckVector3[1].Add(new IntVec3(0,0,1));
            DirectionalCheckVector3[1].Add(new IntVec3(0,0,0));
            DirectionalCheckVector3[1].Add(new IntVec3(0,0,-1));
            DirectionalCheckVector3[1].Add(new IntVec3(1,0,1));
            DirectionalCheckVector3[1].Add(new IntVec3(1,0,0));
            DirectionalCheckVector3[1].Add(new IntVec3(1,0,-1));
            
            DirectionalCheckVector3[2].Add(new IntVec3(0,0,0));
            DirectionalCheckVector3[2].Add(new IntVec3(1,0,0));
            DirectionalCheckVector3[2].Add(new IntVec3(-1,0,0));
            DirectionalCheckVector3[2].Add(new IntVec3(0,0,-1));
            DirectionalCheckVector3[2].Add(new IntVec3(1,0,-1));
            DirectionalCheckVector3[2].Add(new IntVec3(-1,0,-1));

            DirectionalCheckVector3[3].Add(new IntVec3(0,0,1));
            DirectionalCheckVector3[3].Add(new IntVec3(0,0,0));
            DirectionalCheckVector3[3].Add(new IntVec3(0,0,-1));
            DirectionalCheckVector3[3].Add(new IntVec3(-1,0,1));
            DirectionalCheckVector3[3].Add(new IntVec3(-1,0,0));
            DirectionalCheckVector3[3].Add(new IntVec3(-1,0,-1));
            
        }

        public CompProperties_ExtendedShield Props{
            get{
                return props as CompProperties_ExtendedShield;
            }
        }

        public override void PostDraw() {
            base.PostDraw();
            if(recacheGraphic){
                recacheGraphic = false;    
                ShieldGraphic.SetValue(this.parent, shieldActive? Props.shieldActiveGraphic.GraphicColoredFor(this.parent) : null, BindingFlags.NonPublic | BindingFlags.Instance, null, null);
            }
        }

        public override void CompTick() {
            base.CompTick();
            if(shieldActive){
                Pawn eq = getEquipper();
                if(eq != null && !eq.Downed && eq.Map != null){
                    Map map = eq.Map;
                    IntVec3 cell = parent.Position;
                    int i = eq.Rotation.AsInt;
                    //Log.Warning("Valid Equiper");
                    foreach(IntVec3 offset in DirectionalCheckVector3[i]){
                        foreach(Thing t in map.thingGrid.ThingsAt(offset + cell)){
                            //Log.Warning("t is " + t);
                            if(t is Projectile p && !(bool)LandedAccess.GetValue(p) && p.Faction != eq.Faction){
                                ImpactAccess.Invoke(p, new object[] { eq });
                            }
                        }
                    }
                }
            }
        }
        
		public Pawn getEquipper(){
			IThingHolder holder = ParentHolder;
			if(holder != null){
				if(holder is Pawn_EquipmentTracker){
					return ((Pawn_EquipmentTracker)holder).pawn;
				}
				if(holder is Pawn_ApparelTracker){
					return ((Pawn_ApparelTracker)holder).pawn;
				}
			}
			return null;
		}

        public override void PostExposeData() {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref shieldActive, "ShieldActive");
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra() {
            foreach(Gizmo giz in base.CompGetWornGizmosExtra()){
                yield return giz;
            }
            yield return new Command_Toggle
			{
				defaultLabel = Props.shieldToggleLabel,
				defaultDesc = Props.shieldToggleDesc,
				icon = ContentFinder<Texture2D>.Get(Props.shieldIcon, true),
				isActive = (() => this.shieldActive),
				toggleAction = delegate(){
					this.shieldActive = !this.shieldActive;
                    recacheGraphic = true;
                }
			};
        }

        public bool recacheGraphic = true;
        public bool shieldActive;
    }
}
