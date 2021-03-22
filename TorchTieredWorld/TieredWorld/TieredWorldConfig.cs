using System.Xml.Serialization;
using TieredWorld.Core;
using Torch;
using Torch.Views;

namespace TieredWorld
{
    public sealed class TieredWorldConfig : ViewModel, TierNotificationCenter.IConfig
    {
        double _playerSafeZonePermittedDistance = 5000000;
        string _notificationAuthorName = "Tiered World";
        string _hostileTierMessage = "WARNING! You've entered a hostile part of the galaxy. Get closer to the Earth planet to retreat!";
        string _peacefulTierMessage = "You're now safe from all the hostile stuff (or most of it, at least).";

        [XmlElement]
        [Display(Name = "Player SafeZone permitted radius")]
        public double PlayerSafeZonePermittedDistanceFromWorldCenter
        {
            get => _playerSafeZonePermittedDistance;
            set => SetValue(ref _playerSafeZonePermittedDistance, value);
        }

        double TierNotificationCenter.IConfig.HostileTierDistanceFromWorldCenter => _playerSafeZonePermittedDistance;

        [XmlElement]
        public string NotificationAuthorName
        {
            get => _notificationAuthorName;
            set => SetValue(ref _notificationAuthorName, value);
        }

        [XmlElement]
        public string HostileTierMessage
        {
            get => _hostileTierMessage;
            set => SetValue(ref _hostileTierMessage, value);
        }

        [XmlElement]
        public string PeacefulTierMessage
        {
            get => _peacefulTierMessage;
            set => SetValue(ref _peacefulTierMessage, value);
        }
    }
}