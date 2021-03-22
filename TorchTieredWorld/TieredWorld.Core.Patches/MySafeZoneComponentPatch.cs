using System.Reflection;
using NLog;
using Sandbox.Game.Entities;
using SpaceEngineers.Game.Entities.Blocks.SafeZone;
using Torch.Managers.PatchManager;
using Torch.Utils;
using VRage.ModAPI;

namespace TieredWorld.Core.Patches
{
    [PatchShim]
    public sealed class MySafeZoneComponentPatch
    {
        public interface IPlugin
        {
            bool PermitsPlayerSafeZone(IMyEntity entity, bool turnOnSafeZone);
        }

        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

#pragma warning disable 649
        [ReflectedMethodInfo(typeof(MySafeZoneComponent), "OnSafezoneCreateRemove")]
        static readonly MethodInfo _OnSafezoneCreateRemove;

        [ReflectedMethodInfo(typeof(MySafeZoneComponentPatch), nameof(OnSafezoneCreateRemovePatch))]
        static readonly MethodInfo _OnSafezoneCreateRemovePatch;

        [ReflectedMethodInfo(typeof(MySafeZoneComponent), "UpdateSafeZoneEnabled")]
        static readonly MethodInfo _UpdateSafeZoneEnabled;

        [ReflectedMethodInfo(typeof(MySafeZoneComponentPatch), nameof(UpdateSafeZoneEnabledPatch))]
        static readonly MethodInfo _UpdateSafeZoneEnabledPatch;
#pragma warning restore 649

        public static IPlugin Plugin { private get; set; }

        public static void Patch(PatchContext ptx)
        {
            ptx.GetPattern(_OnSafezoneCreateRemove).Prefixes.Add(_OnSafezoneCreateRemovePatch);
            ptx.GetPattern(_UpdateSafeZoneEnabled).Prefixes.Add(_UpdateSafeZoneEnabledPatch);
            Log.Info($"Patching {nameof(MySafeZoneComponentPatch)} done");
        }

        static bool OnSafezoneCreateRemovePatch(MySafeZoneComponent __instance, bool turnOnSafeZone)
        {
            Log.Info($"{nameof(MySafeZoneComponentPatch)}.{nameof(OnSafezoneCreateRemovePatch)}({turnOnSafeZone})");
            return Plugin.PermitsPlayerSafeZone(__instance.Entity, turnOnSafeZone);
        }

        public static bool UpdateSafeZoneEnabledPatch(MySafeZoneComponent __instance, MySafeZone safeZone, bool activate)
        {
            Log.Info($"{nameof(MySafeZoneComponentPatch)}.{nameof(UpdateSafeZoneEnabledPatch)}({activate})");
            return Plugin.PermitsPlayerSafeZone(__instance.Entity, activate);
        }
    }
}