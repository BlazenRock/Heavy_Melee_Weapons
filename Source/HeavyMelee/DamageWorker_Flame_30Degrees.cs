using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace HeavyMelee
{
    public class DamageWorker_Flame_30Degrees : DamageWorker_Flame
    {
        public static Pawn ExplosionOriginator;

        public override IEnumerable<IntVec3> ExplosionCellsToHit(IntVec3 center, Map map, float radius,
            IntVec3? needLOSToCell1 = null,
            IntVec3? needLOSToCell2 = null)
        {
            if (ExplosionOriginator == null)
            {
                Log.Error("[HeavyMelee] Did 30 Degree explosion without pawn!");
                return base.ExplosionCellsToHit(center, map, radius, needLOSToCell1, needLOSToCell2);
            }

            Log.Message("ExplosionCellsToHit!");
            return base.ExplosionCellsToHit(center, map, radius, needLOSToCell1, needLOSToCell2).Where(cell =>
            {
                var angle1 = cell.ToVector3().AngleToFlat(ExplosionOriginator.DrawPos) - 90f;
                while (angle1 < 0) angle1 += 360f;
                var angle2 = ExplosionOriginator.Rotation.AsAngle;
                return Mathf.Abs(angle1 - angle2) < 30f || Mathf.Abs(angle1 - (angle2 + 360f)) < 30f;
            });
        }
    }
}