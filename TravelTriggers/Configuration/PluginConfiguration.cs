using System.Collections.Generic;
using Dalamud.Configuration;

namespace TravelTriggers.Configuration
{
    internal sealed class PluginConfiguration : IPluginConfiguration
    {
        public int Version { get; set; }
        public Dictionary<ulong, CharacterConfiguration> CharacterConfigurations = [];

        public void Save() => TravelTriggers.PluginInterface.SavePluginConfig(this);
        public static PluginConfiguration Load() => TravelTriggers.PluginInterface.GetPluginConfig() as PluginConfiguration ?? new();
    }

    internal sealed class CharacterConfiguration
    {
        public int Version { get; set; }

        public bool PluginEnabled = true;
        public bool RoleplayOnly;
        //public bool SelectAll;
        //public Dictionary<uint, CustomCommand> ZoneCommands = [];
        public CustomCommand? MasterCommand;
    }

    internal sealed class CustomCommand
    {
        public string Content = "";
        public bool Enabled;
    }
}
