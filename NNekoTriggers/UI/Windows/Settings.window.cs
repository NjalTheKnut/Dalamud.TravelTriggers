using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using ECommons.Logging;
using NNekoTriggers.Helpers;

namespace NNekoTriggers.UI.Windows
{
    public sealed class SettingsWindow : Window
    {
        /// <summary>
        ///     Constructor for the Settings Window (In-Game Config GUI).
        /// </summary>
        public SettingsWindow() : base(NNekoTriggers.PluginInterface.Manifest.Name)
        {
            this.Flags = ImGuiWindowFlags.NoResize;
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(525, 430),
                MaximumSize = new Vector2(525, 430),
            };
            this.AllowPinning = true;
            this.Size = new Vector2(525, 430);
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.TitleBarButtons = [
                 new() {
                    Icon = FontAwesomeIcon.Code,
                    Click = (mouseButton) => Util.OpenLink("https://github.com/NjalTheKnut/Dalamud.NNekoTriggers"),
                    ShowTooltip = () => ImGui.SetTooltip("Repository"),
                },
                new() {
                    Icon = FontAwesomeIcon.Comment,
                    Click = (mouseButton) => Util.OpenLink("https://github.com/NjalTheKnut/Dalamud.NNekoTriggers/issues"),
                    ShowTooltip = () => ImGui.SetTooltip("Feedback"),
                },
            ];
        }
        /// <summary>
        ///     The conditions under which the GUI can be opened.
        /// </summary>
        /// <returns>boolean</returns>
        public override bool DrawConditions() => NNekoTriggers.ClientState.IsLoggedIn;

        /// <summary>
        ///     The definition of elements in the Settings/Config Window GUI.
        /// </summary>
        public override void Draw()
        {
            var config = Utils.GetCharacterConfig();
            var mgr = NNekoTriggers.WindowManager;
            // Top-level config options.
            if (ImGui.Checkbox($"Enable {NNekoTriggers.PluginInterface.Manifest.Name}", ref config.PluginEnabled))
            {
                NNekoTriggers.PluginConfiguration.Save();
            }
            //ImGui.SameLine();
            ImGui.BeginDisabled(!config.PluginEnabled);
            if (ImGui.Checkbox("Show in Server Info Bar", ref config.ShowInDtr))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            ImGui.BeginDisabled(!config.ShowInDtr);
            if (ImGui.Checkbox("RP Only", ref config.RpOnlyInDtr))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("RNG", ref config.RngInDtr))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Zone Change", ref config.ZoneInDtr))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Job Swap", ref config.GsetInDtr))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Login", ref config.OnLoginInDtr))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Override", ref config.OcmdInDtr))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            /*if (ImGui.Checkbox("", ref config.))
            {
                NNekoTriggers.PluginConfiguration.Save();
                WindowManager.UpdateDtrEntry(config);
            }*/
            ImGui.EndDisabled();

            if (ImGui.Checkbox("Only enable when roleplaying", ref config.EnableRpOnly))
            {
                NNekoTriggers.PluginConfiguration.Save();
                PluginLog.Information($"NNekoTriggers RP Only Module {(config.EnableRpOnly ? "Enabled" : "Disabled")}");
                //PluginLog.Information($"NNekoTriggers  Module {(config. ? "Enabled" : "Disabled")}");
                mgr.UpdateDtrEntry();
            }

            if (ImGui.Checkbox("Enable RNG feature", ref config.EnableRNG))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }

            ImGui.BeginDisabled(!config.EnableRNG);
            if (ImGui.BeginTable("##OddsTable", 2))
            {
                ImGui.TableSetupColumn("Min", ImGuiTableColumnFlags.WidthFixed, 250);
                ImGui.TableSetupColumn("Max", ImGuiTableColumnFlags.WidthFixed, 250);
                ImGui.TableHeadersRow();
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                if (ImGui.InputInt("Min", ref config.OddsMin, 1, 25))
                {
                    NNekoTriggers.PluginConfiguration.Save();
                    mgr.UpdateDtrEntry();
                }
                ImGui.TableSetColumnIndex(1);
                if (ImGui.InputInt("Max", ref config.OddsMax, 1, 25))
                {
                    NNekoTriggers.PluginConfiguration.Save();
                    mgr.UpdateDtrEntry();
                }
                ImGui.EndTable();
                ImGui.EndDisabled();
            }
            ImGui.EndDisabled();

            if (ImGui.Checkbox("Enable Gearset Swap feature", ref config.EnableGset))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            ImGui.BeginDisabled(!config.EnableGset);
            var gearsetCmd = config.GearsetCommand;
            var gscmdslot = config.GearsetCommand.Content;
#pragma warning disable CS8601 // Possible null reference assignment.
            if (ImGui.InputTextWithHint("Gearset Command", "/command here...", ref gscmdslot, 100))
            {
                unsafe
                {
                    gearsetCmd.Content = gscmdslot;
                    config.GearsetCommand = gearsetCmd;
                    NNekoTriggers.PluginConfiguration.Save();
                    mgr.UpdateDtrEntry();
                }
            }
#pragma warning restore CS8601 // Possible null reference assignment.

            ImGui.EndDisabled();
            if (ImGui.Checkbox("Enable Zone feature", ref config.EnableZones))
            {
                NNekoTriggers.PluginConfiguration.Save();
                mgr.UpdateDtrEntry();
            }
            ImGui.BeginDisabled(!config.EnableZones);
            var territoryCmd = config.ZoneCommand;
            var tcmdslot = config.ZoneCommand.Content;
#pragma warning disable CS8601 // Possible null reference assignment.
            if (ImGui.InputTextWithHint("Zone Command", "/command here...", ref tcmdslot, 100))
            {
                unsafe
                {
                    territoryCmd.Content = tcmdslot;
                    config.ZoneCommand = territoryCmd;
                    NNekoTriggers.PluginConfiguration.Save();
                    mgr.UpdateDtrEntry();
                }
            }
#pragma warning restore CS8601 // Possible null reference assignment.
            ImGui.EndDisabled();

            if (ImGui.Checkbox("Enable Login feature", ref config.EnableOnLogin))
            {
                NNekoTriggers.PluginConfiguration.Save();
                PluginLog.Information($"NNekoTriggers Login Module {(config.EnableOnLogin ? "Enabled" : "Disabled")}");
                mgr.UpdateDtrEntry();
            }
            ImGui.BeginDisabled(!config.EnableOnLogin);
            var onLoginCmd = config.OnLoginCommand;
            var logincmdslot = config.OnLoginCommand.Content;
#pragma warning disable CS8601 // Possible null reference assignment.
            if (ImGui.InputTextWithHint("Login Command", "/command here...", ref logincmdslot, 100))
            {
                unsafe
                {
                    onLoginCmd.Content = logincmdslot;
                    config.OnLoginCommand = onLoginCmd;
                    NNekoTriggers.PluginConfiguration.Save();
                }
            }
#pragma warning restore CS8601 // Possible null reference assignment.
            ImGui.EndDisabled();

            if (ImGui.Checkbox("Enable Command Override feature", ref config.EnableOcmd))
            {
                NNekoTriggers.PluginConfiguration.Save();
                PluginLog.Information($"NNekoTriggers Override Module {(config.EnableOcmd ? "Enabled" : "Disabled")}");
                mgr.UpdateDtrEntry();
            }
            ImGui.BeginDisabled(!config.EnableOcmd);
            var defaultCmd = config.OverrideCommand;
            var dcmdslot = config.OverrideCommand.Content;
#pragma warning disable CS8601 // Possible null reference assignment.
            if (ImGui.InputTextWithHint("Override Command", "/command here...", ref dcmdslot, 100))
            {
                unsafe
                {
                    defaultCmd.Content = dcmdslot;
                    config.OverrideCommand = defaultCmd;
                    NNekoTriggers.PluginConfiguration.Save();
                }
            }
#pragma warning restore CS8601 // Possible null reference assignment.
            ImGui.EndDisabled();

        }
    }
}
