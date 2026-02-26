using Dalamud.Game.Command;
using ECommons.Logging;

namespace TravelTriggers.Command
{
    /// <summary>
    ///     Initializes and manages all commands and command-events for the plugin.
    /// </summary>
    public sealed class CommandManager : IDisposable
    {
        /// <summary>
        ///     Defines the command prefix for all other plugin commands.
        /// </summary>
        private const string SettingsCommand = "/tConfig";
        private const string RpOnlyCmd = "/tRpOnly";
        private const string RngCmd = "/tRng";
        private const string GsetCmd = "/tGset";
        private const string ZoneCmd = "/tZone";
        private const string OverrideCmd = "/tOverride";
        private const string OnLoginCmd = "/tOnLogin";

        /// <summary>
        ///     Initializes the CommandManager and its resources.
        /// </summary>
        public CommandManager()
        {
            TravelTriggers.Commands.AddHandler(SettingsCommand, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "Opens the TravelTriggers configuration window. ",
                ShowInHelp = true
            });

            TravelTriggers.Commands.AddHandler(RpOnlyCmd, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "toggles the Roleplay Only trigger behavior.",
                ShowInHelp = true
            });

            TravelTriggers.Commands.AddHandler(OverrideCmd, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "toggles the Command Override feature.",
                ShowInHelp = true
            });

            TravelTriggers.Commands.AddHandler(RngCmd, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "toggles the RNG trigger behavior.",
                ShowInHelp = true
            });

            TravelTriggers.Commands.AddHandler(GsetCmd, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "toggles the trigger.",
                ShowInHelp = true
            });

            TravelTriggers.Commands.AddHandler(ZoneCmd, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "toggles the Zone change trigger.",
                ShowInHelp = true
            });

            TravelTriggers.Commands.AddHandler(OnLoginCmd, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "toggles the Login trigger.",
                ShowInHelp = true
            });

            /*TravelTriggers.Commands.AddHandler(RngCmd, new CommandInfo(this.OnCommand)
            {
                HelpMessage = "Toggles the feature, and sets, increments, or reduces the bounds of your custom RNG range." +
                "\n'/rng t' toggles the RNG feature on and off" +
                "\n'/rng min <number>' sets Min to a fixed value. (default is 25)" +
                "\n'/rng max <number' sets Max to a fixed value. (default is 100)" +
                "\n'/rng minAdd <number>' Adds <number> to the existing Min value. " +
                "\n'/rng minSub <number>' Subtracts <number> from the existing Min value. " +
                "\n'/rng maxAdd <number>' Adds <number> to the existing Max value. " +
                "\n'/rng maxSub <number>' Subtracts <number> from the existing Max value. " +
                "\n'/rng ' ",
                ShowInHelp = true

            });*/
        }

        /// <summary>
        ///     Dispose of the PluginCommandManager and its resources.
        /// </summary>
        public void Dispose()
        {
            TravelTriggers.Commands.RemoveHandler(SettingsCommand);
            TravelTriggers.Commands.RemoveHandler(RpOnlyCmd);
            TravelTriggers.Commands.RemoveHandler(RngCmd);
            TravelTriggers.Commands.RemoveHandler(OverrideCmd);
            TravelTriggers.Commands.RemoveHandler(GsetCmd);
            TravelTriggers.Commands.RemoveHandler(ZoneCmd);
        }

        /// <summary>
        ///     Event handler for when a command is issued by the user.
        /// </summary>
        /// <param name="command">The command that was issued.</param>
        /// <param name="args">The arguments that were passed with the command.</param>
        ///
        private void OnCommand(string command, string args)
        {
            var config = Helpers.Utils.GetCharacterConfig();
            if (config is null)
            {
                return;
            }
            switch (command)
            {
                case SettingsCommand when args?.Length == 0:
                    TravelTriggers.WindowManager.ToggleConfigWindow();
                    break;
                case RpOnlyCmd when args?.Length == 0:
                    if (config != null)
                    {
                        config.EnableRpOnly = !config.EnableRpOnly;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Roleplay Only Module {(config.EnableRpOnly ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case RpOnlyCmd when args == "on":
                    if (config != null)
                    {
                        config.EnableRpOnly = true;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Roleplay Only Module {(config.EnableRpOnly ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case RpOnlyCmd when args == "off":
                    if (config != null)
                    {
                        config.EnableRpOnly = false;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Roleplay Only Module {(config.EnableRpOnly ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case RngCmd when args?.Length == 0:
                    if (config != null)
                    {
                        config.EnableRNG = !config.EnableRNG;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers RNG Module {(config.EnableRNG ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case RngCmd when args == "on":
                    if (config != null)
                    {
                        config.EnableRNG = true;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers RNG Module {(config.EnableRNG ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case RngCmd when args == "off":
                    if (config != null)
                    {
                        config.EnableRNG = false;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers  Module {(config.EnableRNG ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case GsetCmd when args?.Length == 0:
                    if (config != null)
                    {
                        config.EnableGset = !config.EnableGset;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Gearset Module {(config.EnableGset ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case GsetCmd when args == "on":
                    if (config != null)
                    {
                        config.EnableGset = true;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Gearset Module {(config.EnableGset ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case GsetCmd when args == "off":
                    if (config != null)
                    {
                        config.EnableGset = false;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Gearset Module {(config.EnableGset ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case ZoneCmd when args?.Length == 0:
                    if (config != null)
                    {
                        config.EnableZones = !config.EnableZones;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Zone Module {(config.EnableZones ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case ZoneCmd when args == "on":
                    if (config != null)
                    {
                        config.EnableZones = true;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Zone Module {(config.EnableZones ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case ZoneCmd when args == "off":
                    if (config != null)
                    {
                        config.EnableZones = false;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Zone Module {(config.EnableZones ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case OverrideCmd when args?.Length == 0:
                    if (config != null)
                    {
                        config.EnableOcmd = !config.EnableOcmd;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Override Module {(config.EnableOcmd ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case OverrideCmd when args == "on":
                    if (config != null)
                    {
                        config.EnableOcmd = true;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Override Module {(config.EnableOcmd ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case OverrideCmd when args == "off":
                    if (config != null)
                    {
                        config.EnableOcmd = false;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Override Module {(config.EnableOcmd ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case OnLoginCmd when args?.Length == 0:
                    if (config != null)
                    {
                        config.EnableOnLogin = !config.EnableOnLogin;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Login Module {(config.EnableOnLogin ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case OnLoginCmd when args == "on":
                    if (config != null)
                    {
                        config.EnableOnLogin = true;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Login Module {(config.EnableOnLogin ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
                case OnLoginCmd when args == "off":
                    if (config != null)
                    {
                        config.EnableOnLogin = false;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Login Module {(config.EnableOnLogin ? "Enabled" : "Disabled")}");
                        TravelTriggers.WindowManager.UpdateDtrEntry();
                    }
                    break;
            }
        }
    }
}
