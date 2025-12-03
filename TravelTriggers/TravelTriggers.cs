using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using ECommons;
using ECommons.GameHelpers;
using TravelTriggers.Command;
using TravelTriggers.Configuration;
using TravelTriggers.UI;
using TerritoryType = Lumina.Excel.Sheets.TerritoryType;

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
        [PluginService] public static IPlayerState PlayerState { get; private set; }
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
        private static readonly uint[] TeleportActionIds = [
            5, //Teleport
            6, //Return (Patch 4.1)
            10061, //Return (Patch 4.15)
            21069, //Storm Ticket
            21070, //Adder Ticket
            21071, //Flame Ticket
            30362, //Vesper Bay Ticket
            41708, //Gold Saucer Ticket
            28064, //Firmament Ticket
            49121  //Cosmic Exploration Ticket
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
            Framework.Update += this.OnFrameworkUpdate;
            //ClientState.TerritoryChanged += this.OnTerritoryChanged;
            ClientState.ClassJobChanged += this.ClientState_ClassJobChanged;
            ECommonsMain.Init(PluginInterface, this);
        }


        /// <summary>
        ///     Disposes of the plugin's resources.
        /// </summary>
        public void Dispose()
        {
            ECommonsMain.Dispose();
            ClientState.ClassJobChanged -= this.ClientState_ClassJobChanged;
            //ClientState.TerritoryChanged -= this.OnTerritoryChanged;
            Framework.Update -= this.OnFrameworkUpdate;
            CommandManager.Dispose();
            WindowManager.Dispose();
        }
        private void OnFrameworkUpdate(IFramework framework)
        {
            if (!ClientState.IsLoggedIn)
            {
                return;
            }

            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled &&
                (!characterConfig.RoleplayOnly || Player.OnlineStatus == ROLEPLAY_ONLINE_STATUS_ID) &&
                characterConfig.EnableTeleportMode)
            {
                new Task(() =>
                {
                    if (IsPlayerTeleporting() && ShouldDoENF())
                    {
                        try
                        {
                            while (Condition[ConditionFlag.BetweenAreas]
                                    || Condition[ConditionFlag.BetweenAreas51]
                                    || Condition[ConditionFlag.Occupied]
                                    || Condition[ConditionFlag.OccupiedInCutSceneEvent]
                                    || Condition[ConditionFlag.Unconscious])
                            {
                                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                            }
                            var cmd = characterConfig.DefaultCommand.Content;
                            if (!cmd.IsNullOrEmpty())
                            {
                                PluginLog.Information("OnFrameworkUpdate: Command Triggered");
                                Commands.ProcessCommand(cmd);
                            }
                        }
                        catch (Exception e) { PluginLog.Error(e, "OnFrameworkUpdate: An error occured processing Framework Update."); }
                    }
                }).Start();
            }
        }

        private void ClientState_ClassJobChanged(uint classJobId)
        {
            if (!ClientState.IsLoggedIn)
            {
                return;
            }

            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled &&
                (!characterConfig.RoleplayOnly || Player.OnlineStatus == ROLEPLAY_ONLINE_STATUS_ID) &&
                characterConfig.EnableGearsetSwap && PlayerState.ClassJob.Value.ClassJobCategory.IsValid)
            {

                new Task(() =>
                {
                    if (ShouldDoENF())
                    {
                        try
                        {
                            while (Condition[ConditionFlag.BetweenAreas]
                                || Condition[ConditionFlag.BetweenAreas51]
                                || Condition[ConditionFlag.Occupied]
                                || Condition[ConditionFlag.OccupiedInCutSceneEvent]
                                || Condition[ConditionFlag.Unconscious])
                            {
                                PluginLog.Debug("ClientState_ClassJobChanged: Unable to execute yet, waiting for conditions to clear.");

                                Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                            }
                            var cmd = characterConfig.DefaultCommand.Content;
                            if (!cmd.IsNullOrEmpty())
                            {
                                PluginLog.Information("ClientState_ClassJobChanged: Command Triggered");
                                Commands.ProcessCommand(cmd);
                            }
                        }
                        catch (Exception e) { PluginLog.Error(e, "ClientState_ClassJobChanged: An error occured processing ClientState_ClassJobChanged."); }
                    }
                }).Start();
            }
        }

        private static bool IsPlayerTeleporting()
        {
            var result = false;
            result = Player.IsCasting && Player.Object.CastActionId.NotNull(out var spellId) && spellId.EqualsAny(TeleportActionIds);
            PluginLog.Information($"IsPlayerTeleporting: " +
                $"\nSpellId = {ECommons.ExcelServices.ExcelActionHelper.GetActionName(Player.Object.CastActionId, true)}" +
                $"\nResult = {(result ? "True" : "False")}");
            return result;
        }

        private static bool ShouldDoENF()
        {
            var result = false;
            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled)
            {
                result = ((characterConfig.EnableRNG && (Random.Shared.Next(characterConfig.OddsMax) <= characterConfig.OddsMin)) || !characterConfig.EnableRNG) && !(Condition[ConditionFlag.Mounted] || Condition[ConditionFlag.WaitingForDuty]);
                PluginLog.Information($"ShouldDoENF: " +
                    $"\nEnableRNG = {(characterConfig.EnableRNG ? "Enabled" : "Disabled")} " +
                    $"\nOddsMin = {characterConfig.OddsMin}" +
                    $"\nOddsMax = {characterConfig.OddsMax}" +
                    $"\nMounted = {(Condition[ConditionFlag.Mounted] ? "True" : "False")}" +
                    $"\nWaitingForDuty = {(Condition[ConditionFlag.WaitingForDuty] ? "True" : "False")}" +
                    $"\nResult = {(result ? "True" : "False")}");
            }
            return result;
        }

        /*/// <summary>
        ///     Handles territory changes and custom command execution.
        /// </summary>
        private void OnTerritoryChanged(ushort territory)
        {

            if (!ClientState.IsLoggedIn)
            {
                return;
            }

            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled &&
                (!characterConfig.RoleplayOnly || Player.OnlineStatus == ROLEPLAY_ONLINE_STATUS_ID) &&
                ((!GenericHelpers.IsNullOrEmpty(characterConfig.DefaultCommand.Content)) || (characterConfig.ZoneCommands.TryGetValue(territory, out var customCommand) && customCommand.Enabled)))
            {
                PluginLog.Information("OnTerritoryChanged trigger");

                if (!AllowedTerritories.Any(t => t.RowId == territory))
                {
                    PluginLog.Warning($"Territory {territory} is not an allowed territoryID, skipping custom executions.");
                    return;
                }
                new Task(() =>
                {
                    if (ShouldDoENF())
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
                            var cmd = GenericHelpers.IsNullOrEmpty(characterConfig.DefaultCommand.Content) ? characterConfig.ZoneCommands[territory].Content : characterConfig.DefaultCommand.Content;
                            if (!GenericHelpers.IsNullOrEmpty(cmd))
                            {
                                Commands.ProcessCommand(cmd);
                            }
                        }
                        catch (Exception e) { PluginLog.Error(e, "An error occured whilst attempting to execute custom commands."); }
                    }
                }).Start();
            }
        }*/

    }
}
