using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;

namespace TravelTriggers.UI.Windows
{
    public sealed class SettingsWindow : Window
    {
        private static readonly Dictionary<uint, string> TerritoryList = TravelTriggers.AllowedTerritories
            .Where(t => !string.IsNullOrEmpty(t.PlaceName.Value.Name.ExtractText())
                )
            .OrderBy(x => x.PlaceName.Value.Name.ExtractText())
            .ToDictionary(
                t => t.RowId,
                t => t.PlaceName.Value.Name.ExtractText()
            );
        private string searchQuery = string.Empty;

        public SettingsWindow() : base(TravelTriggers.PluginInterface.Manifest.Name)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(600, 200),
                MaximumSize = new Vector2(1200, 1000)
            };
            this.Size = new Vector2(600, 200);
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.TitleBarButtons = [
                 new() {
                    Icon = FontAwesomeIcon.Comment,
                    Click = (mouseButton) => Util.OpenLink("https://github.com/NjalTheKnut/TravelTriggers"),
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
            }
            ImGui.SameLine();
            ImGui.BeginDisabled(!config.PluginEnabled);
            if (ImGui.Checkbox("Only enable when roleplaying", ref config.RoleplayOnly))
            {
                TravelTriggers.PluginConfiguration.Save();
            }
            if (ImGui.Checkbox("Enable RNG feature", ref config.EnableRNG))
            {
                TravelTriggers.PluginConfiguration.Save();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Enable Gearset Swap feature", ref config.EnableGearsetSwap))
            {
                TravelTriggers.PluginConfiguration.Save();
            }
            ImGui.SameLine();
            if (ImGui.Checkbox("Enable Teleport feature", ref config.EnableTeleportMode))
            {
                TravelTriggers.PluginConfiguration.Save();
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

            var defaultCmd = config.DefaultCommand;
            var cmdslot = config.DefaultCommand.Content;
#pragma warning disable CS8601 // Possible null reference assignment.
            if (ImGui.InputTextWithHint("Default/Override Command", "/command here...", ref cmdslot, 100))
            {
                unsafe
                {
                    defaultCmd.Content = cmdslot;
                    config.DefaultCommand = defaultCmd;
                    TravelTriggers.PluginConfiguration.Save();
                }
            }

#pragma warning restore CS8601 // Possible null reference assignment.
            // Zone list.
            if (ImGui.CollapsingHeader("Zone List"))
            {
                ImGui.SetNextItemWidth(-1);
                ImGui.InputTextWithHint("##Search", "Search...", ref this.searchQuery, 100);
                var filteredTerritories = TerritoryList.Where(x => x.Value.Contains(this.searchQuery, StringComparison.InvariantCultureIgnoreCase));
                if (filteredTerritories.Any())
                {
                    ImGui.BeginDisabled(!config.PluginEnabled);
                    if (ImGui.BeginTable("##SettingsTable", 4, ImGuiTableFlags.ScrollY))
                    {
                        ImGui.TableSetupScrollFreeze(0, 1);
                        ImGui.TableSetupColumn("Zone", ImGuiTableColumnFlags.WidthFixed, 250);
                        ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 50);
                        ImGui.TableSetupColumn("Command", ImGuiTableColumnFlags.WidthFixed, 250);
                        ImGui.TableHeadersRow();
                        foreach (var t in filteredTerritories)
                        {
                            var customCommand = config.DefaultCommand.Content.IsNullOrEmpty() ? config.ZoneCommands.GetValueOrDefault(t.Key, new()) : config.DefaultCommand;

                            ImGui.TableNextRow();
                            ImGui.TableNextColumn();
                            ImGui.Text(t.Value);
                            ImGui.TableNextColumn();
                            if (ImGui.Checkbox($"##{t.Key}", ref customCommand.Enabled))
                            {
                                config.ZoneCommands[t.Key] = customCommand;
                                TravelTriggers.PluginConfiguration.Save();
                            }
                            ImGui.TableSetColumnIndex(2);
                            ImGui.BeginDisabled(!customCommand.Enabled);

                            var slot = customCommand.Content;
#pragma warning disable CS8601 // Possible null reference assignment.
                            if (ImGui.InputTextWithHint($"##{t.Key} Command", "/command here...", ref slot, 100))
                            {
                                unsafe
                                {
                                    customCommand.Content = slot;
                                    config.ZoneCommands[t.Key] = customCommand;
                                    TravelTriggers.PluginConfiguration.Save();
                                }
                            }
#pragma warning restore CS8601 // Possible null reference assignment.

                            ImGui.EndDisabled();
                        }
                        ImGui.EndTable();
                        ImGui.EndDisabled();
                    }
                }
                else
                {
                    ImGui.TextDisabled("No zones matching your search query");
                }
            }
        }
    }
}
