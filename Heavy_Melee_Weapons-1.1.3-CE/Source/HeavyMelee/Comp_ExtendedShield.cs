using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;
using VFECore;

namespace HeavyMelee
{
    public class CompProperties_ExtendedShield : CompProperties
    {
        public GraphicData shieldActiveGraphic;
        public string shieldIcon = "";
        public string shieldToggleDesc;

        public string shieldToggleLabel;

        public CompProperties_ExtendedShield()
        {
            compClass = typeof(Comp_ExtendedShield);
        }
    }

    public class Comp_ExtendedShield : ThingComp
    {
        public static List<IntVec3>[] DirectionalCheckVector3 = new List<IntVec3>[4];
        public static FieldInfo LandedAccess = AccessTools.DeclaredField(typeof(Projectile), "landed");
        public static MethodInfo ImpactAccess = AccessTools.DeclaredMethod(typeof(Projectile), "Impact");
        public static FieldInfo ShieldGraphic = AccessTools.DeclaredField(typeof(Apparel_Shield), "shieldGraphic");

        public bool recacheGraphic = true;
        public bool shieldActive;

        static Comp_ExtendedShield()
        {
            DirectionalCheckVector3[0] = new List<IntVec3>();
            DirectionalCheckVector3[1] = new List<IntVec3>();
            DirectionalCheckVector3[2] = new List<IntVec3>();
            DirectionalCheckVector3[3] = new List<IntVec3>();

            DirectionalCheckVector3[0].Add(new IntVec3(0, 0, 0));
            DirectionalCheckVector3[0].Add(new IntVec3(1, 0, 0));
            DirectionalCheckVector3[0].Add(new IntVec3(-1, 0, 0));
            DirectionalCheckVector3[0].Add(new IntVec3(0, 0, 1));
            DirectionalCheckVector3[0].Add(new IntVec3(1, 0, 1));
            DirectionalCheckVector3[0].Add(new IntVec3(-1, 0, 1));

            DirectionalCheckVector3[1].Add(new IntVec3(0, 0, 1));
            DirectionalCheckVector3[1].Add(new IntVec3(0, 0, 0));
            DirectionalCheckVector3[1].Add(new IntVec3(0, 0, -1));
            DirectionalCheckVector3[1].Add(new IntVec3(1, 0, 1));
            DirectionalCheckVector3[1].Add(new IntVec3(1, 0, 0));
            DirectionalCheckVector3[1].Add(new IntVec3(1, 0, -1));

            DirectionalCheckVector3[2].Add(new IntVec3(0, 0, 0));
            DirectionalCheckVector3[2].Add(new IntVec3(1, 0, 0));
            DirectionalCheckVector3[2].Add(new IntVec3(-1, 0, 0));
            DirectionalCheckVector3[2].Add(new IntVec3(0, 0, -1));
            DirectionalCheckVector3[2].Add(new IntVec3(1, 0, -1));
            DirectionalCheckVector3[2].Add(new IntVec3(-1, 0, -1));

            DirectionalCheckVector3[3].Add(new IntVec3(0, 0, 1));
            DirectionalCheckVector3[3].Add(new IntVec3(0, 0, 0));
            DirectionalCheckVector3[3].Add(new IntVec3(0, 0, -1));
            DirectionalCheckVector3[3].Add(new IntVec3(-1, 0, 1));
            DirectionalCheckVector3[3].Add(new IntVec3(-1, 0, 0));
            DirectionalCheckVector3[3].Add(new IntVec3(-1, 0, -1));
        }

        public CompProperties_ExtendedShield Props => props as CompProperties_ExtendedShield;

        public override void PostDraw()
        {
            base.PostDraw();
            if (recacheGraphic)
            {
                recacheGraphic = false;
                ShieldGraphic.SetValue(parent,
                    shieldActive ? Props.shieldActiveGraphic.GraphicColoredFor(parent) : null,
                    BindingFlags.NonPublic | BindingFlags.Instance, null, null);
            }
        }

        public override void CompTick()
        {
            base.CompTick();
            if (shieldActive)
            {
                var eq = getEquipper();
                if (eq != null && !eq.Downed && eq.Map != null)
                {
                    var map = eq.Map;
                    var cell = parent.Position;
                    var i = eq.Rotation.AsInt;
                    //Log.Warning("Valid Equiper");
                    foreach (var offset in DirectionalCheckVector3[i])
                    foreach (var t in map.thingGrid.ThingsAt(offset + cell)) //Log.Warning("t is " + t);
                        if (t is Projectile p && !(bool) LandedAccess.GetValue(p) && p.Faction != eq.Faction)
                            ImpactAccess.Invoke(p, new object[] {eq});
                }
            }
        }

        public Pawn getEquipper()
        {
            var holder = ParentHolder;
            if (holder != null)
            {
                if (holder is Pawn_EquipmentTracker) return ((Pawn_EquipmentTracker) holder).pawn;
                if (holder is Pawn_ApparelTracker) return ((Pawn_ApparelTracker) holder).pawn;
            }

            return null;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref shieldActive, "ShieldActive");
        }

        public override IEnumerable<Gizmo> CompGetWornGizmosExtra()
        {
            foreach (var giz in base.CompGetWornGizmosExtra()) yield return giz;
            yield return new Command_Toggle
            {
                defaultLabel = Props.shieldToggleLabel,
                defaultDesc = Props.shieldToggleDesc,
                icon = ContentFinder<Texture2D>.Get(Props.shieldIcon),
                isActive = () => shieldActive,
                toggleAction = delegate
                {
                    shieldActive = !shieldActive;
                    recacheGraphic = true;
                }
            };
        }
    }
}