using System.Xml.Serialization;
using TieredWorld.Core;
using TieredWorld.Core.Patches;
using Torch;
using Torch.Views;

namespace TieredWorld
{
    public sealed class TieredWorldConfig : ViewModel, TierWorldMessenger.IConfig, MySpaceRespawnComponentPatch.IConfig
    {
        const string MessageGroupName = "Messages";

        bool _enableSafeSpawn = true;
        float _safeZoneRadius = 5000000; //meters
        string _notificationName = "Tiered World";
        string _hostileTierMessage = "WARNING! You've entered a hostile part of the galaxy. Get closer to the Earth planet to retreat!";
        string _peacefulTierMessage = "You're now safe from all the hostile stuff (or most of it, at least).";

        float TierWorldMessenger.IConfig.HostileTierDistance => _safeZoneRadius;
        float MySpaceRespawnComponentPatch.IConfig.SafeSpawnRadius => _safeZoneRadius;

        [XmlElement]
        [Display(Name = "Safe world radius")]
        public float SafeWorldRadius
        {
            get => _safeZoneRadius;
            set => SetValue(ref _safeZoneRadius, value);
        }

        [XmlElement]
        [Display(Name = "Enable safe spawn")]
        public bool EnableSafeSpawn
        {
            get => _enableSafeSpawn;
            set => SetValue(ref _enableSafeSpawn, value);
        }

        [XmlElement]
        [Display(Name = "Message name", GroupName = MessageGroupName, Order = 1)]
        public string MessageName
        {
            get => _notificationName;
            set => SetValue(ref _notificationName, value);
        }

        [XmlElement]
        [Display(Name = "Hostile tier message", GroupName = MessageGroupName, Order = 2)]
        public string HostileTierMessage
        {
            get => _hostileTierMessage;
            set => SetValue(ref _hostileTierMessage, value);
        }

        [XmlElement]
        [Display(Name = "Peaceful tier message", GroupName = MessageGroupName, Order = 3)]
        public string PeacefulTierMessage
        {
            get => _peacefulTierMessage;
            set => SetValue(ref _peacefulTierMessage, value);
        }
    }
}