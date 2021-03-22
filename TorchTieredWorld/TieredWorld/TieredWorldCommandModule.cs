using NLog;
using Torch.Commands;
using Torch.Commands.Permissions;
using Utils.Torch;
using VRage.Game.ModAPI;

namespace TieredWorld
{
    [Category("tw")]
    public sealed class TieredWorldCommandModule : CommandModule
    {
        static readonly ILogger Log = LogManager.GetCurrentClassLogger();

        TieredWorldPlugin Plugin => (TieredWorldPlugin) Context.Plugin;

        [Command("configs")]
        [Permission(MyPromoteLevel.Admin)]
        public void Configs()
        {
            this.GetOrSetProperty(Plugin.Config);
        }

        [Command("commands")]
        [Permission(MyPromoteLevel.Admin)]
        public void Commands()
        {
            this.ShowCommands();
        }
    }
}