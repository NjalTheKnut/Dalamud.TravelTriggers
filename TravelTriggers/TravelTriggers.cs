using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
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
            ClientState.ClassJobChanged += this.ClientState_ClassJobChanged;
        }

        /// <summary>
        ///     Disposes of the plugin's resources.
        /// </summary>
        public void Dispose()
        {
            ClientState.TerritoryChanged -= this.OnTerritoryChanged;
            ClientState.ClassJobChanged -= this.ClientState_ClassJobChanged;
            CommandManager.Dispose();
            WindowManager.Dispose();
        }

        private void ClientState_ClassJobChanged(uint classJobId)
        {
            if (!ClientState.IsLoggedIn)
            {
                return;
            }

            if (PluginConfiguration.CharacterConfigurations.TryGetValue(ClientState.LocalContentId, out var characterConfig) &&
                characterConfig.PluginEnabled &&
                (!characterConfig.RoleplayOnly || ClientState.LocalPlayer?.OnlineStatus.RowId == ROLEPLAY_ONLINE_STATUS_ID) &&
                characterConfig.EnableGearsetSwap && ClientState.LocalPlayer?.ClassJob.Value.ClassJobCategory.IsValid == true &&
                !characterConfig.DefaultCommand.Content.IsNullOrEmpty())
            {
                switch (ClientState.LocalPlayer?.ClassJob.Value.Abbreviation.ToString())
                {
                    case "MIN":
                        return;
                    case "BTN":
                        return;
                    case "FSH":
                        return;
                    case "CRP":
                        return;
                    case "BSM":
                        return;
                    case "ARM":
                        return;
                    case "GSM":
                        return;
                    case "LTW":
                        return;
                    case "WVR":
                        return;
                    case "ALC":
                        return;
                    case "CUL":
                        return;
                    default:
                        PluginLog.Information("ClientState_ClassJobChanged trigger");
                        new Task(() =>
                        {
                            if (((characterConfig.EnableRNG && Random.Shared.Next(100) <= 25) || !characterConfig.EnableRNG) && !(Condition[ConditionFlag.Mounted] || Condition[ConditionFlag.WaitingForDuty]))
                            {
                                try
                                {
                                    Commands.ProcessCommand("/porch play Damnation");
                                    Commands.ProcessCommand("/popup -n -s You have an unsettled feeling of vulnerability...");
                                    while (Condition[ConditionFlag.BetweenAreas]
                                        || Condition[ConditionFlag.BetweenAreas51]
                                        || Condition[ConditionFlag.Occupied]
                                        || Condition[ConditionFlag.OccupiedInCutSceneEvent]
                                        || Condition[ConditionFlag.Unconscious])
                                    {
                                        PluginLog.Debug("Unable to execute yet, waiting for conditions to clear.");

                                        Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                                    }
                                    var cmd = characterConfig.DefaultCommand.Content;
                                    if (!cmd.IsNullOrEmpty())
                                    {
                                        Commands.ProcessCommand(cmd);
                                    }
                                }
                                catch (Exception e) { PluginLog.Error(e, "An error occured whilst attempting to execute custom commands."); }
                            }
                        }).Start();
                        break;
                }
            }

        }

        //private void OnCastTeleport(object sender, EventArgs e)
        //{
        //    if (!ClientState.IsLoggedIn)
        //    {
        //        return;
        //    }

        //    if (PluginConfiguration.CharacterConfigurations.TryGetValue(ClientState.LocalContentId, out var characterConfig) &&
        //        characterConfig.PluginEnabled &&
        //        (!characterConfig.RoleplayOnly || ClientState.LocalPlayer?.OnlineStatus.RowId == ROLEPLAY_ONLINE_STATUS_ID) &&
        //        sender.Equals(ClientState.LocalPlayer?.GameObjectId))
        //    {
        //        if (Condition[ConditionFlag.Casting] && (ClientState.LocalPlayer?.CastActionId == 5 || ClientState.LocalPlayer?.CastActionId == 6))
        //        {

        //            if (characterConfig.EnableRNG && Random.Shared.Next(100) <= 25 && !(Condition[ConditionFlag.Mounted] || Condition[ConditionFlag.WaitingForDuty]))
        //            {
        //                try
        //                {
        //                    Commands.ProcessCommand("/porch play Damnation");
        //                    Commands.ProcessCommand("/popup -n -s You have an unsettled feeling of vulnerability...");
        //                }
        //                catch (Exception err) { PluginLog.Error(err, "An error occured whilst attempting to execute custom commands."); }
        //            }
        //        }
        //    }
        //}

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
                ((!characterConfig.DefaultCommand.Content.IsNullOrEmpty()) || (characterConfig.ZoneCommands.TryGetValue(territory, out var customCommand) && customCommand.Enabled)))
            {
                PluginLog.Information("OnTerritoryChanged trigger");

                if (!AllowedTerritories.Any(t => t.RowId == territory))
                {
                    PluginLog.Warning($"Territory {territory} is not an allowed territoryID, skipping custom executions.");
                    return;
                }
                new Task(() =>
                {
                    if (((characterConfig.EnableRNG && Random.Shared.Next(100) <= 25) || !characterConfig.EnableRNG) && !(Condition[ConditionFlag.Mounted] || Condition[ConditionFlag.WaitingForDuty]))
                    {
                        try
                    {
                        Commands.ProcessCommand("/porch play Damnation");
                        Commands.ProcessCommand("/popup -n -s You have an unsettled feeling of vulnerability...");
                        while (Condition[ConditionFlag.BetweenAreas]
                            || Condition[ConditionFlag.BetweenAreas51]
                            || Condition[ConditionFlag.Occupied]
                            || Condition[ConditionFlag.OccupiedInCutSceneEvent]
                            || Condition[ConditionFlag.Unconscious])
                        {
                            PluginLog.Debug("Unable to execute yet, waiting for conditions to clear.");

                            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                        }
                        var cmd = characterConfig.DefaultCommand.Content.IsNullOrEmpty() ? characterConfig.ZoneCommands[territory].Content : characterConfig.DefaultCommand.Content;
                        if (!cmd.IsNullOrEmpty())
                        {
                            Commands.ProcessCommand(cmd);
                        }
                    }
                    catch (Exception e) { PluginLog.Error(e, "An error occured whilst attempting to execute custom commands."); }
                    }
                }).Start();
            }
        }

    }
}
