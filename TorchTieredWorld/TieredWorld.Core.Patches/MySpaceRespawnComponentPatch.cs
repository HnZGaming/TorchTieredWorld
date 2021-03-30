using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using NLog;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.World;
using Torch.Managers.PatchManager;
using Torch.Managers.PatchManager.MSIL;
using Torch.Utils;
using Utils.General;

namespace TieredWorld.Core.Patches
{
    [PatchShim]
    public static class MySpaceRespawnComponentPatch
    {
        public interface IConfig
        {
            bool EnableSafeSpawn { get; }
            float SafeSpawnRadius { get; }
        }

        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

#pragma warning disable 649
        [ReflectedMethodInfo(typeof(MySpaceRespawnComponent), "GetSpawnPositionInSpace")]
        static readonly MethodInfo _GetSpawnPositionInSpace;

        [ReflectedMethodInfo(typeof(MySpaceRespawnComponentPatch), nameof(GetSpawnPositionInSpaceTranspiler))]
        static readonly MethodInfo _GetSpawnPositionInSpaceTranspiler;
#pragma warning restore 649

        public static IConfig Config { private get; set; }

        static bool IsPatchEnabled => Config?.EnableSafeSpawn ?? false;

        public static void Patch(PatchContext ptx)
        {
            ptx.GetPattern(_GetSpawnPositionInSpace).Transpilers.Add(_GetSpawnPositionInSpaceTranspiler);
            Log.Info($"Patching {nameof(MySpaceRespawnComponentPatch)} done");
        }

        static IEnumerable<MsilInstruction> GetSpawnPositionInSpaceTranspiler(IEnumerable<MsilInstruction> ins)
        {
            var log = new StringBuilder();
            log.AppendLine();

            foreach (var insl in GetSpawnPositionInSpaceTranspilerImpl(ins))
            {
                log.AppendLine(insl.ToString());
                yield return insl;
            }

            Log.Debug(log);
        }

        static IEnumerable<MsilInstruction> GetSpawnPositionInSpaceTranspilerImpl(IEnumerable<MsilInstruction> ins)
        {
            var insl = ins.ToList();
            for (var i = 0; i < insl.Count; i++)
            {
                var k0 = insl[i];
                var k1 = insl.GetElementAtIndexOrElse(i + 1, null);
                var k2 = insl.GetElementAtIndexOrElse(i + 2, null);

                // remove the if block that could bypass the distance check
                // FROM (multi-lines):
                // ldarg.0
                // ldfld bool SpaceEngineers.Game.World.MySpaceRespawnComponent/SpawnInfo::SpawnNearPlayers
                // brfalse IL_012c
                if (k1 != null && k2 != null &&
                    k0.OpCode == OpCodes.Ldarg_0 &&
                    k1.OpCode == OpCodes.Ldfld && k1.Operand is MsilOperandInline<FieldInfo> l && l.Value.Name == "SpawnNearPlayers" &&
                    k2.OpCode == OpCodes.Brfalse)
                {
                    Log.Info($"found {l.Value.Name}");
                    yield return new MsilInstruction(OpCodes.Nop);
                    yield return new MsilInstruction(OpCodes.Nop);
                    yield return k2.CopyWith(OpCodes.Br);
                    i += 2;
                    continue;
                }

                // FROM: call bool [Sandbox.Game]Sandbox.Game.Entities.MyEntities::IsWorldLimited()
                if (k0.OpCode == OpCodes.Call && k0.Operand is MsilOperandInline<MethodBase> m && m.Value.Name == "IsWorldLimited")
                {
                    Log.Info($"found {m.Value.Name}");
                    var patcher = typeof(MySpaceRespawnComponentPatch).Method(nameof(IsWorldLimitedPatch), BindingFlags.NonPublic | BindingFlags.Static);
                    yield return new MsilInstruction(OpCodes.Call).InlineValue((MethodBase) patcher);
                    continue;
                }

                // FROM: call float32 [Sandbox.Game]Sandbox.Game.Entities.MyEntities::WorldSafeHalfExtent()
                if (k0.OpCode == OpCodes.Call && k0.Operand is MsilOperandInline<MethodBase> n && n.Value.Name == "WorldSafeHalfExtent")
                {
                    Log.Info($"found {n.Value.Name}");
                    var patcher = typeof(MySpaceRespawnComponentPatch).Method(nameof(WorldSafeHalfExtentPatch), BindingFlags.NonPublic | BindingFlags.Static);
                    yield return new MsilInstruction(OpCodes.Call).InlineValue((MethodBase) patcher);
                    continue;
                }

                yield return k0;
            }
        }

        static bool IsWorldLimitedPatch()
        {
            return IsPatchEnabled || MyEntities.IsWorldLimited();
        }

        static float WorldSafeHalfExtentPatch()
        {
            return IsPatchEnabled
                ? Config?.SafeSpawnRadius ?? float.PositiveInfinity
                : MyEntities.WorldHalfExtent();
        }
    }
}