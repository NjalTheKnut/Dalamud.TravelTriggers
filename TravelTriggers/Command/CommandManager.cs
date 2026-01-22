using System;
using Dalamud.Game.Command;
using ECommons.Logging;

namespace TravelTriggers.Command
{
    /// <summary>
    ///     Initializes and manages all commands and command-events for the plugin.
    /// </summary>
    public sealed class CommandManager : IDisposable
    {
        private const string SettingsCommand = "/ttrig";

        /// <summary>
        ///     Initializes the CommandManager and its resources.
        /// </summary>
        public CommandManager() => TravelTriggers.Commands.AddHandler(SettingsCommand, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Opens the TravelTriggers configuration window when no arguments are specified. " +
            "\n'/ttrig toggle' to toggle the plugin. " +
            "\n'/ttrig rp' to toggle roleplay mode. " +
            "\n'/ttrig ocmd' to toggle Command Override mode. " +
            "\n'/ttrig rng' to toggle RNG mode. " +
            "\n'/ttrig gs' to toggle Gearset mode." +
            "\n'/ttrig tp' to toggle Territory mode. ",
            ShowInHelp = true
        });

        /// <summary>
        ///     Dispose of the PluginCommandManager and its resources.
        /// </summary>
        public void Dispose() => TravelTriggers.Commands.RemoveHandler(SettingsCommand);

        /// <summary>
        ///     Event handler for when a command is issued by the user.
        /// </summary>
        /// <param name="command">The command that was issued.</param>
        /// <param name="args">The arguments that were passed with the command.</param>
        ///
        private void OnCommand(string command, string args)
        {
            var hasConfig = TravelTriggers.PluginConfiguration.CharacterConfigurations.TryGetValue(TravelTriggers.PlayerState.ContentId, out var config);
            if (!hasConfig || config is null)
            {
                return;
            }

            switch (command)
            {
                case SettingsCommand when args == "toggle":
                    if (config != null)
                    {
                        config.PluginEnabled = !config.PluginEnabled;
                        TravelTriggers.PluginConfiguration.Save();
                        //TravelTriggers.Toast.ShowNormal($"TravelTriggers Plugin {(config.PluginEnabled ? "Enabled" : "Disabled")}", TravelTriggers.ToastOptions);
                        PluginLog.Debug($"TravelTriggers Plugin {(config.PluginEnabled ? "Enabled" : "Disabled")}");
                        UI.WindowManager.UpdateDtrEntry(config);
                    }
                    break;
                case SettingsCommand when args == "rp":
                    if (config != null)
                    {
                        config.RoleplayOnly = !config.RoleplayOnly;
                        TravelTriggers.PluginConfiguration.Save();
                        //TravelTriggers.Toast.ShowNormal($"TravelTriggers Roleplay Only Module {(config.RoleplayOnly ? "Enabled" : "Disabled")}", TravelTriggers.ToastOptions);
                        PluginLog.Debug($"TravelTriggers Roleplay Only Module {(config.RoleplayOnly ? "Enabled" : "Disabled")}");
                        UI.WindowManager.UpdateDtrEntry(config);
                    }
                    break;
                case SettingsCommand when args == "ocmd":
                    if (config != null)
                    {
                        config.EnableOverride = !config.EnableOverride;
                        TravelTriggers.PluginConfiguration.Save();
                        //TravelTriggers.Toast.ShowNormal($"TravelTriggers Command Override Module {(config.EnableOverride ? "Enabled" : "Disabled")}", TravelTriggers.ToastOptions);
                        PluginLog.Debug($"TravelTriggers Command Override Module {(config.EnableOverride ? "Enabled" : "Disabled")}");
                        UI.WindowManager.UpdateDtrEntry(config);
                    }
                    break;
                case SettingsCommand when args == "rng":
                    if (config != null)
                    {
                        config.EnableRNG = !config.EnableRNG;
                        TravelTriggers.PluginConfiguration.Save();
                        //TravelTriggers.Toast.ShowNormal($"TravelTriggers RNG Module {(config.EnableRNG ? "Enabled" : "Disabled")}", TravelTriggers.ToastOptions);
                        PluginLog.Debug($"TravelTriggers RNG Module {(config.EnableRNG ? "Enabled" : "Disabled")}");
                        UI.WindowManager.UpdateDtrEntry(config);
                    }
                    break;
                case SettingsCommand when args == "gs":
                    if (config != null)
                    {
                        config.EnableGearsetSwap = !config.EnableGearsetSwap;
                        TravelTriggers.PluginConfiguration.Save();
                        //TravelTriggers.Toast.ShowNormal($"TravelTriggers Gearset Module {(config.EnableGearsetSwap ? "Enabled" : "Disabled")}", TravelTriggers.ToastOptions);
                        PluginLog.Debug($"TravelTriggers Gearset Module {(config.EnableGearsetSwap ? "Enabled" : "Disabled")}");
                        UI.WindowManager.UpdateDtrEntry(config);
                    }
                    break;
                case SettingsCommand when args == "tp":
                    if (config != null)
                    {
                        config.EnableTerritoryMode = !config.EnableTerritoryMode;
                        TravelTriggers.PluginConfiguration.Save();
                        //TravelTriggers.Toast.ShowNormal($"TravelTriggers Territory Module {(config.EnableTerritoryMode ? "Enabled" : "Disabled")}", TravelTriggers.ToastOptions);
                        PluginLog.Debug($"TravelTriggers Territory Module {(config.EnableTerritoryMode ? "Enabled" : "Disabled")}");
                        UI.WindowManager.UpdateDtrEntry(config);
                    }
                    break;
                case SettingsCommand when args?.Length == 0:
                    TravelTriggers.WindowManager.ToggleConfigWindow();
                    break;
            }
        }
    }
}
