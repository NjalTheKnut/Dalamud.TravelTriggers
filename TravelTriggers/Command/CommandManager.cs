using System;
using Dalamud.Game.Command;

namespace TravelTriggers.Command
{
    /// <summary>
    ///     Initializes and manages all commands and command-events for the plugin.
    /// </summary>
    public sealed class CommandManager : IDisposable
    {
        private const string SettingsCommand = "/TravelTriggers";

        /// <summary>
        ///     Initializes the CommandManager and its resources.
        /// </summary>
        public CommandManager() => TravelTriggers.Commands.AddHandler(SettingsCommand, new CommandInfo(this.OnCommand)
        {
            HelpMessage = "Opens the TravelTriggers configuration window when no arguments are specified. '/TravelTriggers toggle' to toggle the plugin, '/TravelTriggers rp' to toggle roleplay mode.",
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
            var hasConfig = TravelTriggers.PluginConfiguration.CharacterConfigurations.TryGetValue(TravelTriggers.ClientState.LocalContentId, out var config);
            if (!hasConfig || config is null)
            {
                return;
            }

            switch (command)
            {
                case SettingsCommand when args == "toggle":
                    if (config != null)
                    { config.PluginEnabled = !config.PluginEnabled; TravelTriggers.PluginConfiguration.Save(); }
                    break;
                case SettingsCommand when args?.Length == 0:
                    TravelTriggers.WindowManager.ToggleConfigWindow();
                    break;
            }
        }
    }
}
