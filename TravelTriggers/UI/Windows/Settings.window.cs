using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;

namespace TravelTriggers.UI.Windows
{
    public sealed class SettingsWindow : Window
    {
        public SettingsWindow() : base(TravelTriggers.PluginInterface.Manifest.Name)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(500, 300),
                MaximumSize = new Vector2(500, 300)
            };
            this.Size = new Vector2(500, 300);
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.TitleBarButtons = [
                 new() {
                    Icon = FontAwesomeIcon.Code,
                    Click = (mouseButton) => Util.OpenLink("https://github.com/NjalTheKnut/Dalamud.TravelTriggers"),
                    ShowTooltip = () => ImGui.SetTooltip("Repository"),
                },
            ];
        }

        public override bool DrawConditions() => TravelTriggers.ClientState.IsLoggedIn;

        public override void Draw()
        {
            if (!TravelTriggers.PluginConfiguration.CharacterConfigurations.TryGetValue(TravelTriggers.PlayerState.ContentId, out var config))
            {
                config = new();
                TravelTriggers.PluginConfiguration.CharacterConfigurations[TravelTriggers.PlayerState.ContentId] = config;
            }

            // Top-level config options.
            if (ImGui.Checkbox($"Enable {TravelTriggers.PluginInterface.Manifest.Name}", ref config.PluginEnabled))
            {
                TravelTriggers.PluginConfiguration.Save();
                if (ImGui.Checkbox("Enable Territory feature", ref config.ShowInDtr))
                {
                    WindowManager.UpdateDtrEntry(config);
                }
            }
#pragma warning disable CS8601 // Possible null reference assignment.
            ImGui.SameLine();
            ImGui.BeginDisabled(!config.PluginEnabled);
            if (ImGui.Checkbox("Only enable when roleplaying", ref config.RoleplayOnly))
            {
                TravelTriggers.PluginConfiguration.Save();
            }
            if (ImGui.Checkbox("Enable Command Override feature", ref config.EnableOverride))
            {
                TravelTriggers.PluginConfiguration.Save();
                ImGui.BeginDisabled(!config.EnableOverride);
                var defaultCmd = config.DefaultCommand;
                var dcmdslot = config.DefaultCommand.Content;
                if (ImGui.InputTextWithHint("Override Command", "/command here...", ref dcmdslot, 100))
                {
                    unsafe
                    {
                        defaultCmd.Content = dcmdslot;
                        config.DefaultCommand = defaultCmd;
                        TravelTriggers.PluginConfiguration.Save();
                    }
                }
                ImGui.EndDisabled();
            }

            if (ImGui.Checkbox("Enable RNG feature", ref config.EnableRNG))
            {
                TravelTriggers.PluginConfiguration.Save();
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
                        TravelTriggers.PluginConfiguration.Save();
                    }
                    ImGui.TableSetColumnIndex(1);
                    if (ImGui.InputInt("Max", ref config.OddsMax, 1, 25))
                    {
                        TravelTriggers.PluginConfiguration.Save();
                    }
                    ImGui.EndTable();
                    ImGui.EndDisabled();
                }
                ImGui.EndDisabled();
            }


            if (ImGui.Checkbox("Enable Gearset Swap feature", ref config.EnableGearsetSwap))
            {
                TravelTriggers.PluginConfiguration.Save();
                ImGui.BeginDisabled(!config.EnableGearsetSwap);
                var gearsetCmd = config.GearsetCommand;
                var gscmdslot = config.GearsetCommand.Content;
                if (ImGui.InputTextWithHint("Gearset Command", "/command here...", ref gscmdslot, 100))
                {
                    unsafe
                    {
                        gearsetCmd.Content = gscmdslot;
                        config.GearsetCommand = gearsetCmd;
                        TravelTriggers.PluginConfiguration.Save();
                    }
                }
                ImGui.EndDisabled();
            }

            if (ImGui.Checkbox("Enable Territory feature", ref config.EnableTerritoryMode))
            {
                TravelTriggers.PluginConfiguration.Save();
                ImGui.BeginDisabled(!config.EnableTerritoryMode);
                var territoryCmd = config.TerritoryCommand;
                var tcmdslot = config.TerritoryCommand.Content;
                if (ImGui.InputTextWithHint("Territory Command", "/command here...", ref tcmdslot, 100))
                {
                    unsafe
                    {
                        territoryCmd.Content = tcmdslot;
                        config.TerritoryCommand = territoryCmd;
                        TravelTriggers.PluginConfiguration.Save();
                    }
                }
                ImGui.EndDisabled();
            }
        }
    }
}
