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
        //public static bool DoENF = ShouldDoENF();
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
            5,
            6,
            10061,
            21071,
            21069,
            21070,
            30362,
            41708,
            28064,
            49121
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
            //ClientState.TerritoryChanged += this.OnTerritoryChanged;
            Framework.Update += this.OnFrameworkUpdate;
            ClientState.ClassJobChanged += this.ClientState_ClassJobChanged;
            ECommonsMain.Init(PluginInterface, this);
            //DoENF = false;
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
                (!characterConfig.RoleplayOnly || Player.OnlineStatus == ROLEPLAY_ONLINE_STATUS_ID))
            {
                if (IsPlayerTeleporting() && ShouldDoENF())
                {
                    try
                    {
                        while (IsPlayerTeleporting())
                        {
                            Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                        }
                        var cmd = characterConfig.DefaultCommand.Content;
                        if (!GenericHelpers.IsNullOrEmpty(cmd))
                        {
                            Commands.ProcessCommand(cmd);
                        }
                    }
                    catch (Exception e) { PluginLog.Error(e, "An error occured processing Framework Update."); }

                }
            }
        }

        private static bool IsPlayerTeleporting()
        {
            var result = false;
            result = Player.IsCasting && Player.Object.CastActionId.NotNull(out var spellId) && spellId.EqualsAny(TeleportActionIds) && (Player.Object.BaseCastTime <= Player.Object.CurrentCastTime);
            return result;
        }

        private static bool ShouldDoENF()
        {
            var result = false;
            if (PluginConfiguration.CharacterConfigurations.TryGetValue(PlayerState.ContentId, out var characterConfig) &&
                characterConfig.PluginEnabled)
            {
                result = ((characterConfig.EnableRNG && Random.Shared.Next(100) <= 25) || !characterConfig.EnableRNG) && !(Condition[ConditionFlag.Mounted] || Condition[ConditionFlag.WaitingForDuty]);
            }
            return result;
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
                characterConfig.EnableGearsetSwap && PlayerState.ClassJob.Value.ClassJobCategory.IsValid &&
                !GenericHelpers.IsNullOrEmpty(characterConfig.DefaultCommand.Content))
            {

                PluginLog.Information("ClientState_ClassJobChanged trigger");
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
                            var cmd = characterConfig.DefaultCommand.Content;
                            if (!GenericHelpers.IsNullOrEmpty(cmd))
                            {
                                Commands.ProcessCommand(cmd);
                            }
                        }
                        catch (Exception e) { PluginLog.Error(e, "An error occured whilst attempting to execute custom commands."); }
                    }
                }).Start();
            }
        }

        /// <summary>
        ///     Handles territory changes and custom command execution.
        /// </summary>
        private static void OnTerritoryChanged(ushort territory)
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
        }

    }
}
