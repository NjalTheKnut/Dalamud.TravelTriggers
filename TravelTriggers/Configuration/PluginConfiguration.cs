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
        public bool ShowInDtr;
        public bool RoleplayOnly;
        public bool EnableRNG;
        public bool EnableOverride;
        public bool EnableGearsetSwap = true;
        public bool EnableTerritoryMode = true;
        public int OddsMax = 100;
        public int OddsMin = 25;
        public Dictionary<uint, CustomCommand> ZoneCommands = [];
        public CustomCommand DefaultCommand = new();
        public CustomCommand GearsetCommand = new();
        public CustomCommand TerritoryCommand = new();
        public CustomCommand CursedLootCommand = new();
    }

    internal sealed class CustomCommand
    {
        public string? Content;
        public bool Enabled = true;
    }
}
