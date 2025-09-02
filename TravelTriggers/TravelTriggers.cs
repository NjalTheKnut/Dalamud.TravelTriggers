using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using TravelTriggers.Command;
using TravelTriggers.Configuration;
using TravelTriggers.UI;

namespace TravelTriggers
{
    internal sealed class TravelTriggers : IDalamudPlugin
    {
#pragma warning disable CS8618
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static ICommandManager Commands { get; private set; }
        [PluginService] public static IClientState ClientState { get; private set; }
        [PluginService] public static IDataManager DataManager { get; private set; }
        [PluginService] public static ICondition Condition { get; private set; }
        [PluginService] public static IFramework Framework { get; private set; }
        [PluginService] public static IPluginLog PluginLog { get; private set; }
        public static CommandManager CommandManager { get; private set; }
        public static WindowManager WindowManager { get; private set; }
        public static PluginConfiguration PluginConfiguration { get; private set; }
        public static IEnumerable<TerritoryType> AllowedTerritories;
#pragma warning restore CS8618

        private const uint ROLEPLAY_ONLINE_STATUS_ID = 22;
        private static readonly uint[] AllowedTerritoryUse = [
              0,  // Town
              1,  // Open World
              2,  // Inn
             13, // Housing Area
             19, // Chocobo Square
             23, // Gold Saucer
             30, // Free Company Garrison
             41, // Eureka
             45, // Masked Carnival
             46, // Ocean Fishing
             47, // Island Sanctuary
             48, // Bozja
             60, // Cosmic Exploration
        ];

        /// <summary>
        ///     The plugin's main entry point.
        /// </summary>
        public TravelTriggers()
        {
            AllowedTerritories = DataManager.Excel.GetSheet<TerritoryType>().Where(x => AllowedTerritoryUse.Contains(x.TerritoryIntendedUse.RowId) && !x.IsPvpZone);
            PluginConfiguration = PluginConfiguration.Load();
            WindowManager = new();
            CommandManager = new();
            ClientState.TerritoryChanged += this.OnTerritoryChanged;
        }
        /// <summary>
        ///     Disposes of the plugin's resources.
        /// </summary>
        public void Dispose()
        {
            ClientState.TerritoryChanged -= this.OnTerritoryChanged;
            CommandManager.Dispose();
            WindowManager.Dispose();
        }

        /// <summary>
        ///     Handles territory changes and custom command execution.
        /// </summary>
        private void OnTerritoryChanged(ushort territory)
        {

            if (!ClientState.IsLoggedIn)
            {
                return;
            }

            if (PluginConfiguration.CharacterConfigurations.TryGetValue(ClientState.LocalContentId, out var characterConfig) &&
                characterConfig.PluginEnabled &&
                (!characterConfig.RoleplayOnly || ClientState.LocalPlayer?.OnlineStatus.RowId == ROLEPLAY_ONLINE_STATUS_ID) &&
                characterConfig.MasterCommand.Enabled)
                //characterConfig.ZoneCommands.TryGetValue(territory, out var customCommand) && customCommand.Enabled)
            {
                PluginLog.Information("trigger");

                if (!AllowedTerritories.Any(t => t.RowId == territory))
                {
                    PluginLog.Warning($"Territory {territory} is not an allowed territoryID, skipping custom executions.");
                    return;
                }
                new Task(() =>
                {
                    try
                    {
                        while (Condition[ConditionFlag.BetweenAreas]
                            || Condition[ConditionFlag.BetweenAreas51]
                            || Condition[ConditionFlag.Occupied]
                            || Condition[ConditionFlag.OccupiedInCutSceneEvent]
                            || Condition[ConditionFlag.Unconscious])
                        {
                            PluginLog.Debug("Unable to execute yet, waiting for conditions to clear.");
                            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                        }
                        //var cmd = characterConfig.SelectAll ? characterConfig.MasterCommand.Content : characterConfig.ZoneCommands[territory].Content;
                        //if (!cmd.IsNullOrEmpty())
                        //{
                        //    Commands.ProcessCommand(cmd);
                        //}

                        var cmd = characterConfig.MasterCommand.Content;
                        if (!cmd.IsNullOrEmpty())
                        {

                        }
                    }
                    catch (Exception e) { PluginLog.Error(e, "An error occured whilst attempting to execute custom commands."); }
                }).Start();
            }
        }

    }
}
