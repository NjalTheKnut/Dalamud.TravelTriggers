using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.ServerSentEvents;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Gui.Toast;
using Dalamud.Game.Inventory;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Textures;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using ECommons;
using ECommons.DalamudServices;
using ECommons.GameHelpers;
using ECommons.Logging;
using Lumina;
using Lumina.Text.ReadOnly;
using TravelTriggers.Command;
using TravelTriggers.Configuration;
using TravelTriggers.UI;
using Task = System.Threading.Tasks.Task;
using TerritoryType = Lumina.Excel.Sheets.TerritoryType;

namespace TravelTriggers
{
    internal sealed class TravelTriggers : IDalamudPlugin, IDisposable
    {
#pragma warning disable CS8618
        [PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; }
        [PluginService] public static ICommandManager Commands { get; private set; }
        [PluginService] public static IClientState ClientState { get; private set; }
        [PluginService] public static IDataManager DataManager { get; private set; }
        [PluginService] public static ICondition Condition { get; private set; }
        [PluginService] public static IFramework Framework { get; private set; }
        [PluginService] public static IDtrBar DtrBar { get; private set; }
        [PluginService] public static IAgentLifecycle AgentLifecycle { get; private set; }
        [PluginService] public static IPluginLog PluginLog { get; private set; }
        [PluginService] public static IPlayerState PlayerState { get; private set; }
        [PluginService] internal static IToastGui Toast { get; private set; } = null!;
        public static ToastOptions ToastOptions = new()
        {
            Speed = ToastSpeed.Fast,
            Position = ToastPosition.Top
        };
        public static CommandManager CommandManager { get; private set; }
        public static WindowManager WindowManager { get; private set; }
        public static PluginConfiguration PluginConfiguration { get; private set; }
        internal static IDtrBarEntry DtrEntry;
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
            ECommonsMain.Init(PluginInterface, this);
            PluginConfiguration = PluginConfiguration.Load();
            AllowedTerritories = DataManager.Excel.GetSheet<TerritoryType>().Where(x => AllowedTerritoryUse.Contains(x.TerritoryIntendedUse.RowId) && !x.IsPvpZone);
            WindowManager = new();
            CommandManager = new();
            DtrEntry ??= Svc.DtrBar.Get("TravelTriggers");
            DtrEntry.Text = new SeString(
                        new IconPayload(BitmapFontIcon.None),
                        new TextPayload($"TTrig Enabled"));
            DtrEntry.Shown = true;
            //+= this.OnLootCoffer;
            ClientState.TerritoryChanged += this.OnTerritoryChanged;
            ClientState.ClassJobChanged += this.ClientState_ClassJobChanged;
        }

        /// <summary>
        ///     Disposes of the plugin's resources.
        /// </summary>
        public void Dispose()
        {
            ClientState.ClassJobChanged -= this.ClientState_ClassJobChanged;
            ClientState.TerritoryChanged -= this.OnTerritoryChanged;
            DtrEntry.Remove();
            CommandManager.Dispose();
            WindowManager.Dispose();
            ECommonsMain.Dispose();
        }

        //private void OnFrameworkUpdate()
        //{
        //    if (!ClientState.IsLoggedIn)
        //    {
        //        return;
        //    }

        //    if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
        //        characterConfig.PluginEnabled)
        //    {


        //        DtrEntry.Text = new SeString(
        //                new IconPayload(BitmapFontIcon.Mentor),
        //                new TextPayload($"TTrig Enabled"));

        //    }
        //    else
        //    {
        //        DtrEntry.Text = new SeString(
        //        new IconPayload(BitmapFontIcon.NoCircle),
        //        new TextPayload("TTrig Disabled"));
        //    }

        //    DtrEntry.Shown = true;
        //}

        /*private void OnLootCoffer(AgentGameEventArgs agentGameEventArgs)
        {
            if (!ClientState.IsLoggedIn)
            {
                return;
            }

            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled &&
                (!characterConfig.RoleplayOnly || Player.OnlineStatus.Equals(ROLEPLAY_ONLINE_STATUS_ID)) &&
                characterConfig.EnableCursedLootMode)
            {
                PluginLog.Information("OnLootCoffer: 'Cursed Loot' Command Triggered");
                if (agentGameEventArgs == null)
                {
                    return;
                }
                else if (agentGameEventArgs.AgentId == Dalamud.Game.Agent.AgentId.Loot)
                {
                    var cmd = "/echo TravelTriggers: 'Cursed Loot' Command is Unset.";
                    if (!GenericHelpers.IsNullOrEmpty(cmd))
                    {
                        PluginLog.Information("OnLootCoffer: Trigger Successful. Processing 'Cursed Loot' Command.");
                        Commands.ProcessCommand(cmd);
                    }
                }
            }
        }*/


        /// <summary>
        ///     Handles class/job changes and custom command execution.
        /// </summary>
        /// <param name="classJobId"></param>
        private void ClientState_ClassJobChanged(uint classJobId)
        {
            if (!ClientState.IsLoggedIn)
            {
                return;
            }

            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled &&
                (!characterConfig.RoleplayOnly || Player.OnlineStatus.Equals(ROLEPLAY_ONLINE_STATUS_ID)) &&
                characterConfig.EnableGearsetSwap && PlayerState.ClassJob.Value.ClassJobCategory.IsValid)
            {
                PluginLog.Information("ClientState_ClassJobChanged: Job Swap Command Triggered");
                new Task(() =>
                {
                    if (ShouldDoENF())
                    {
                        try
                        {

                            var cmd = "/echo TravelTriggers: Job Swap Command is Unset.";
                            if (characterConfig.EnableOverride)
                            {
                                cmd = characterConfig.DefaultCommand.Content;
                            }
                            else if (!GenericHelpers.IsNullOrEmpty(characterConfig.GearsetCommand.Content))
                            {
                                cmd = characterConfig.GearsetCommand.Content;
                            }
                            else
                            {
                                PluginLog.Debug("Unable to execute, because no Override or Job Swap commands were found.");
                                return;
                            }
#pragma warning disable CS8604 // Possible null reference argument.
                            if (!GenericHelpers.IsNullOrEmpty(cmd))
                            {
                                PluginLog.Information("ClientState_ClassJobChanged: Trigger Successful. Processing Job Swap Command.");
                                Commands.ProcessCommand(cmd);
                            }
#pragma warning restore CS8604 // Possible null reference argument.
                        }
                        catch (Exception e) { PluginLog.Error(e, "ClientState_ClassJobChanged: An error occured processing ClientState_ClassJobChanged."); }
                    }
                }).Start();
            }
        }

        /// <summary>
        ///     Handles territory changes and custom command execution.
        /// </summary>
        /// <param name="territory"></param>
        private void OnTerritoryChanged(ushort territory)
        {

            if (!ClientState.IsLoggedIn)
            {
                return;
            }

            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled &&
                characterConfig.EnableTerritoryMode &&
                (!characterConfig.RoleplayOnly || Player.OnlineStatus.Equals(ROLEPLAY_ONLINE_STATUS_ID)) &&
                (!GenericHelpers.IsNullOrEmpty(characterConfig.TerritoryCommand.Content)))
            {
                PluginLog.Information("OnTerritoryChanged: Terriotry Command Triggered");

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
                            var cmd = "/echo TravelTriggers: Territory Command is Unset.";
                            if (characterConfig.EnableOverride)
                            {
                                cmd = characterConfig.DefaultCommand.Content;
                            }
                            else if (!GenericHelpers.IsNullOrEmpty(characterConfig.TerritoryCommand.Content))
                            {
                                cmd = characterConfig.TerritoryCommand.Content;
                            }
                            else
                            {
                                PluginLog.Debug("Unable to execute, because no Override or Territory commands were found.");
                                return;
                            }
#pragma warning disable CS8604 // Possible null reference argument.
                            if (!GenericHelpers.IsNullOrEmpty(cmd))
                            {
                                PluginLog.Information("OnTerritoryChanged: Trigger Successful. Processing Territory Command.");
                                Commands.ProcessCommand(cmd);
                            }
#pragma warning restore CS8604 // Possible null reference argument.
                        }
                        catch (Exception e) { PluginLog.Error(e, "An error occured whilst attempting to execute custom commands."); }
                    }
                }).Start();
            }
        }

        /// <summary>
        ///     Checks the current player status and the plugin configuration to determine whether to queue an attempted execution of custom commands.
        /// </summary>
        /// <returns>True if conditions for attempting the commands are met, and False otherwise.</returns>
        private static bool ShouldDoENF()
        {
            var result = false;
            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled)
            {
                result = ((characterConfig.EnableRNG && (Random.Shared.Next(characterConfig.OddsMax) <= characterConfig.OddsMin)) ||
                          !characterConfig.EnableRNG) && !(Condition[ConditionFlag.Mounted] || Condition[ConditionFlag.WaitingForDuty]);
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
    }
}
