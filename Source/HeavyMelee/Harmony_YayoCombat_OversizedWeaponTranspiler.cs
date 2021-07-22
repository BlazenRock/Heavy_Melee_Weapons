using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace HeavyMelee
{
    public static class Harmony_YayoCombat_OversizedWeaponTranspiler
    {
        public static bool YayoCombatLoaded;

        public static Dictionary<XYFlipMeshKey, Mesh> SA_KeyedMesh = new Dictionary<XYFlipMeshKey, Mesh>();

        public static MethodInfo SA_Transpiler_meshSwap =
            AccessTools.DeclaredMethod(typeof(Harmony_YayoCombat_OversizedWeaponTranspiler), "Transpiler_meshSwap");

        public static void tryPatch_YC_OWT(Harmony hInstance)
        {
            var internalBypassType = typeFromString("yayoCombat.patch_DrawEquipmentAiming");
            //Type_CompDeflector = typeFromString("CompDeflector.CompDeflector");
            if (internalBypassType != null)
            {
                YayoCombatLoaded = true;
                //Log.Warning("Detected Yayo Combat, Patch starting");
                hInstance.Patch(
                    AccessTools.Method(internalBypassType, "Prefix"),
                    null,
                    null,
                    new HarmonyMethod(typeof(Harmony_YayoCombat_OversizedWeaponTranspiler), nameof(YC_Transpiler))
                );
            }
        }

        public static Mesh meshOfSize(XYFlipMeshKey xyf)
        {
            Mesh outMesh;
            if (SA_KeyedMesh.TryGetValue(xyf, out outMesh)) return outMesh;
            outMesh = xyf.generateMesh();
            SA_KeyedMesh.Add(xyf, outMesh);
            return outMesh;
        }

        public static Mesh Transpiler_meshSwap(Mesh mesh, Thing holdingThing)
        {
            //ThingComp thingComp = (holdingThing as ThingWithComps).AllComps.FirstOrDefault((ThingComp y) => y.GetType() == Type_CompDeflector || (Type_CompDeflector != null && Type_CompDeflector.IsAssignableFrom(y.GetType())));
            var compOversizedWeapon = holdingThing.TryGetComp<CompOversizedWeapon.CompOversizedWeapon>();
            if (compOversizedWeapon != null)
            {
                var meshSwap = meshOfSize(new XYFlipMeshKey(holdingThing.def.graphicData.drawSize.x,
                    holdingThing.def.graphicData.drawSize.y, mesh == MeshPool.plane10Flip));
                return meshSwap;
            }

            return mesh;
        }

        public static IEnumerable<CodeInstruction> YC_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            //Log.Warning("YCT Patch start");
            var CIPrev = instructions.ToList();
            for (var i = 0; i < CIPrev.Count; i++)
            {
                var ci = CIPrev[i];
                //Log.Warning(ci.ToString());
                if (i > 7 &&
                    CIPrev[i - 0].opcode == OpCodes.Ldloc_2 &&
                    CIPrev[i - 1].opcode == OpCodes.Nop &&
                    CIPrev[i - 2].opcode == OpCodes.Stloc_S &&
                    CIPrev[i - 3].opcode == OpCodes.Callvirt &&
                    CIPrev[i - 4].opcode == OpCodes.Callvirt &&
                    CIPrev[i - 5].opcode == OpCodes.Ldarg_1 &&
                    CIPrev[i - 6].opcode == OpCodes.Nop &&
                    CIPrev[i - 7].opcode == OpCodes.Br_S
                )
                {
                    //Log.Warning("YCT Mesh Patching " + i);
                    yield return ci;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Call, SA_Transpiler_meshSwap);
                }
                else
                {
                    yield return ci;
                }
            }
        }

        public static Type typeFromString(string typeString)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var type = assembly.GetType(typeString, false, true);
                if (type != null) return type;
            }

            var type2 = Type.GetType(typeString, false, true);
            if (type2 != null) return type2;
            //Log.Warning("No type of " + typeString + " found in any assemblies! Returning null");
            return null;
        }

        public static Vector3 AdjustRenderOffsetFromDir(Pawn pawn,
            CompOversizedWeapon.CompOversizedWeapon compOversizedWeapon)
        {
            if (compOversizedWeapon.Props != null)
            {
                var rotation = pawn.Rotation;
                switch (rotation.AsInt)
                {
                    case 0:
                        return compOversizedWeapon.Props.northOffset;
                    case 1:
                        return compOversizedWeapon.Props.eastOffset;
                    case 2:
                        return compOversizedWeapon.Props.southOffset;
                    case 3:
                        return compOversizedWeapon.Props.westOffset;
                }
            }

            return default;
        }

        public struct XYFlipMeshKey
        {
            public float X;
            public float Y;
            public bool flip;
            public int cachedHash;

            public XYFlipMeshKey(float _X, float _Y, bool _f)
            {
                X = _X;
                Y = _Y;
                flip = _f;
                cachedHash = _X.GetHashCode() ^ _Y.GetHashCode() ^ (flip ? 0x8000000 : 0x0);
            }

            public override int GetHashCode()
            {
                return cachedHash;
            }

            public override bool Equals(object obj)
            {
                return obj is XYFlipMeshKey xyf && xyf.X == X && xyf.Y == Y && xyf.flip == flip;
            }

            public Mesh generateMesh()
            {
                return MeshMakerPlanes.NewPlaneMesh(new Vector2(X, Y), flip, false, false);
            }
        }
    }
}