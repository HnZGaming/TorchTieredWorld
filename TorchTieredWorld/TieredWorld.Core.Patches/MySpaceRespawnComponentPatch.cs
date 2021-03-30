using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
            var insl = ins.ToList();
            for (var i = 0; i < insl.Count; i++)
            {
                var k = insl[i];

                // FROM: call bool [Sandbox.Game]Sandbox.Game.Entities.MyEntities::IsWorldLimited()
                if (k.OpCode == OpCodes.Call && k.Operand is MsilOperandInline<MethodBase> m && m.Value.Name == "IsWorldLimited")
                {
                    var patcher = typeof(MySpaceRespawnComponentPatch).Method(nameof(IsWorldLimitedPatch), BindingFlags.NonPublic | BindingFlags.Static);
                    yield return new MsilInstruction(OpCodes.Call).InlineValue((MethodBase) patcher);
                    continue;
                }

                // FROM: call float32 [Sandbox.Game]Sandbox.Game.Entities.MyEntities::WorldSafeHalfExtent()
                if (k.OpCode == OpCodes.Call && k.Operand is MsilOperandInline<MethodBase> n && n.Value.Name == "WorldSafeHalfExtent")
                {
                    var patcher = typeof(MySpaceRespawnComponentPatch).Method(nameof(WorldSafeHalfExtentPatch), BindingFlags.NonPublic | BindingFlags.Static);
                    yield return new MsilInstruction(OpCodes.Call).InlineValue((MethodBase) patcher);
                    continue;
                }

                yield return k;
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