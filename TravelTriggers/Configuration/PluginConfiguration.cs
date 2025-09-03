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
        public CustomCommand MasterCommand;
    }

    internal sealed class CustomCommand
    {
        private const string V = $"/echo [TravelTriggers] Command Not Set.  Use /TravelTriggers to configure.";

        public string? Content { get; set; }

        private CustomCommand() => this.Content = V;

        // ToDo: Add feature to support more than one command per config?
        //private CustomCommand(string? content) => this.Content = content;
    }
}
