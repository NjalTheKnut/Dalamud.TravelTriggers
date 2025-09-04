using System.Collections.Generic;
using Dalamud.Configuration;
using FFXIVClientStructs;
using InteropGenerator.Runtime;

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
        public CustomCommand MasterCommand;
    }

    internal sealed class CustomCommand
    {
        public string Content { get; set; }

        public CustomCommand()
        {
            if (this.Content == null || this.Content.Length == 0)
            {
                this.Content = "/echo [TravelTriggers] Command Not Set.  Use /TravelTriggers to configure.";
            }
        }
    }
}
