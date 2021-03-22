using System.Linq;
using System.Threading;
using System.Windows.Controls;
using NLog;
using Sandbox.Game.Entities;
using Sandbox.Game.World;
using TieredWorld.Core;
using TieredWorld.Core.Patches;
using Torch;
using Torch.API;
using Torch.API.Managers;
using Torch.API.Plugins;
using Utils.General;
using Utils.Torch;
using VRage.ModAPI;

namespace TieredWorld
{
    public class TieredWorldPlugin :
        TorchPluginBase,
        IWpfPlugin,
        MySafeZoneComponentPatch.IPlugin
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        Persistent<TieredWorldConfig> _config;
        UserControl _userControl;

        CancellationTokenSource _cancellationTokenSource;
        IChatManagerServer _chatManager;
        TierNotificationCenter _tierNotificationCenter;

        public UserControl GetControl() => _config.GetOrCreateUserControl(ref _userControl);
        public TieredWorldConfig Config => _config.Data;

        public override void Init(ITorchBase torch)
        {
            base.Init(torch);

            this.ListenOnGameLoaded(OnGameLoaded);
            this.ListenOnGameUnloading(OnGameUnloaded);

            GameLoopObserverManager.Add(Torch);

            var configPath = this.MakeConfigFilePath();
            _config = Persistent<TieredWorldConfig>.Load(configPath);

            _cancellationTokenSource = new CancellationTokenSource();

            MySafeZoneComponentPatch.Plugin = this;
        }

        void OnGameLoaded()
        {
            _chatManager = Torch.CurrentSession.Managers.GetManager<IChatManagerServer>();
            _chatManager.OrThrow("Chat manager not found");

            _tierNotificationCenter = new TierNotificationCenter(Config, _chatManager);
            TaskUtils.RunUntilCancelledAsync(_tierNotificationCenter.Run, _cancellationTokenSource.Token).Forget(Log);
        }

        void OnGameUnloaded()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        bool MySafeZoneComponentPatch.IPlugin.PermitsPlayerSafeZone(IMyEntity entity, bool turnOnSafeZone)
        {
            Log.Info($"{nameof(TieredWorldConfig)}.{nameof(MySafeZoneComponentPatch.IPlugin.PermitsPlayerSafeZone)}()");

            if (!turnOnSafeZone) return true; // disabling safezone is permitted anywhere

            var entityPosition = entity.PositionComp.GetPosition();
            var distance = entityPosition.Length();
            var permitted = distance < Config.PlayerSafeZonePermittedDistanceFromWorldCenter;
            Log.Info($"Player SafeZone: {entityPosition}, {distance}");

            if (permitted) return true;

            Log.Info("Player SafeZone denied");

            // send a chat message to whoever affected (as to why their safe zone isn't activated)
            var grid = entity.GetParentEntityOfType<MyCubeGrid>();
            var ownerIds = grid.BigOwners.Concat(grid.SmallOwners).ToSet();
            foreach (var ownerId in ownerIds)
            {
                var steamId = MySession.Static.Players.TryGetSteamId(ownerId);
                if (steamId == 0) continue;

                _chatManager.SendMessage("Server", steamId, $"Can't activate safe zone here: {distance:0}m away from the world center");
            }

            return false;
        }
    }
}