using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Sandbox.Game.World;
using Torch.API.Managers;
using Utils.General;
using Utils.Torch;
using VRageMath;

namespace TieredWorld.Core
{
    public sealed class TierWorldMessenger
    {
        public interface IConfig
        {
            float HostileTierDistance { get; }
            string MessageName { get; }
            string HostileTierMessage { get; }
            string PeacefulTierMessage { get; }
        }

        readonly IConfig _config;
        readonly IChatManagerServer _chatManager;
        readonly Dictionary<ulong, Vector3D> _lastPositions;

        public TierWorldMessenger(IConfig config, IChatManagerServer chatManager)
        {
            _config = config;
            _chatManager = chatManager;
            _lastPositions = new Dictionary<ulong, Vector3D>();
        }

        public async Task Run(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await GameLoopObserver.MoveToGameLoop(cancellationToken);

                foreach (var player in MySession.Static.Players.GetOnlinePlayers())
                {
                    if (player == null) continue;

                    var steamId = player.SteamId();
                    if (steamId == 0) continue;

                    var position = player.GetPosition();

                    if (_lastPositions.TryGetValue(steamId, out var lastPosition))
                    {
                        NotifyIfTierChanged(steamId, position, lastPosition);
                    }

                    _lastPositions[steamId] = position;
                }

                await Task.Delay(1.Seconds(), cancellationToken);
            }
        }

        void NotifyIfTierChanged(ulong steamId, Vector3D position, Vector3D lastPosition)
        {
            var distance = position.Length();
            var lastDistance = lastPosition.Length();

            if (distance > _config.HostileTierDistance &&
                lastDistance < _config.HostileTierDistance)
            {
                Notify(steamId, Color.Red, _config.HostileTierMessage);
            }

            if (distance < _config.HostileTierDistance &&
                lastDistance > _config.HostileTierDistance)
            {
                Notify(steamId, Color.White, _config.PeacefulTierMessage);
            }
        }

        void Notify(ulong steamId, Color color, string message)
        {
            _chatManager.SendMessageAsOther(_config.MessageName, message, color, steamId);
        }
    }
}