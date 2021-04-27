using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Verse;

namespace HeavyMelee
{
    public class HeavyMeleeMod : Mod
    {
        private static readonly Dictionary<CompEquippable, CompPsychicShock> compCache =
            new Dictionary<CompEquippable, CompPsychicShock>();

        public HeavyMeleeMod(ModContentPack content) : base(content)
        {
            var harm = new Harmony("PitchStone.HeavyMeleeWeapons");
            harm.Patch(AccessTools.Method(typeof(CompEquippable), "GetVerbsCommands"),
                postfix: new HarmonyMethod(typeof(HeavyMeleeMod), "AddShockCommand"));
            harm.Patch(AccessTools.Method(typeof(Verb), "VerbTick"),
                postfix: new HarmonyMethod(GetType(), "VerbPostTick"));
            harm.Patch(AccessTools.Method(typeof(VerbTracker), "CreateVerbTargetCommand"),
                new HarmonyMethod(AccessTools.Method(GetType(), "UseCustomCommand1")));
            harm.Patch(
                AccessTools.Method(Type.GetType("MVCF.Utilities.PawnVerbGizmoUtility, MVCF"),
                    "GetGizmosForVerb"), postfix: new HarmonyMethod(GetType(), "UseCustomCommand2"));
        }

        public static bool UseCustomCommand1(
            Thing ownerThing,
            Verb verb,
            ref Command_VerbTarget __result)
        {
            if (!(verb is IVerbCustomCommand custom)) return true;
            __result = custom.GetTargetCommand(__result);
            return false;
        }

        public static IEnumerable<Gizmo> UseCustomCommand2(
            IEnumerable<Gizmo> __result)
        {
            return __result.Select(gizmo =>
            {
                if (gizmo is Command_VerbTarget cvt && cvt.verb is IVerbCustomCommand custom)
                    return custom.GetTargetCommand(cvt);

                return gizmo;
            });
        }

        public static void AddShockCommand(ref IEnumerable<Command> __result, CompEquippable __instance)
        {
            if (!compCache.TryGetValue(__instance, out var comp))
            {
                comp = __instance.parent.TryGetComp<CompPsychicShock>();
                compCache.Add(__instance, comp);
            }

            if (comp != null)
                __result = __result.AddItem(new Command_Action
                {
                    defaultLabel = "HeavyMelee.PsychicShock".Translate(),
                    icon = __instance.parent.def.uiIcon,
                    iconAngle = __instance.parent.def.uiIconAngle,
                    iconDrawScale = __instance.parent.def.uiIconScale,
                    iconOffset = __instance.parent.def.uiIconOffset,
                    disabled = Disabled(comp, out var disabledReason),
                    disabledReason = disabledReason,
                    action = () =>
                    {
                        comp.ShockOnNext = true;
                        (comp.parent.holdingOwner.Owner as Pawn_EquipmentTracker)?.pawn?.psychicEntropy
                            ?.OffsetPsyfocusDirectly(-comp.Props.PsyfocusCost);
                    }
                });
        }

        private static bool Disabled(CompPsychicShock comp, out string reason)
        {
            if (comp.ShockOnNext)
            {
                reason = "HeavyMelee.AlreadyShock".Translate();
                return true;
            }

            var pawn = (comp.parent.holdingOwner.Owner as Pawn_EquipmentTracker)?.pawn;
            if (pawn == null)
            {
                reason = "Cannot find pawn";
                return true;
            }

            var psyfocus = pawn.psychicEntropy.CurrentPsyfocus;
            if (psyfocus < comp.Props.PsyfocusCost)
            {
                reason = "HeavyMelee.NotEnoughPsyfocus".Translate();
                return true;
            }

            reason = "";
            return false;
        }

        public static void VerbPostTick(Verb __instance)
        {
            if (__instance is IVerbTick ticker) ticker.Tick();
        }
    }

    public interface IVerbTick
    {
        void Tick();
    }

    public interface IVerbCustomCommand
    {
        Command_VerbTarget GetTargetCommand(Command_VerbTarget old);
    }
}