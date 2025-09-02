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
                MinimumSize = new Vector2(850, 300),
                MaximumSize = new Vector2(1200, 1000)
            };
            this.Size = new Vector2(850, 450);
            this.SizeCondition = ImGuiCond.FirstUseEver;
            this.TitleBarButtons = [
                 new() {
                    Icon = FontAwesomeIcon.Comment,
                    Click = (mouseButton) => Util.OpenLink("https://github.com/NjalTheKnut/Dalamud.TravelTriggers"),
                    ShowTooltip = () => ImGui.SetTooltip("Repository"),
                },
            ];
        }

        public override bool DrawConditions() => TravelTriggers.ClientState.IsLoggedIn;

        public override void Draw()
        {
            if (!TravelTriggers.PluginConfiguration.CharacterConfigurations.TryGetValue(TravelTriggers.ClientState.LocalContentId, out var config))
            {
                config = new();
                TravelTriggers.PluginConfiguration.CharacterConfigurations[TravelTriggers.ClientState.LocalContentId] = config;
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
            ImGui.EndDisabled();
            ImGui.SameLine();
            ImGui.BeginDisabled(!config.PluginEnabled);
            if (ImGui.Checkbox("Enable for all zones", ref config.SelectAll))
            {
                TravelTriggers.PluginConfiguration.Save();
            }
            ImGui.EndDisabled();

            ImGui.SetNextItemWidth(-1);
            ImGui.BeginDisabled(!config.PluginEnabled && !config.SelectAll);
            var mcmd = config.MasterCommand.Content.IsNullOrEmpty() ? "" : config.MasterCommand.Content;
            if (ImGui.InputTextWithHint($"##MasterCommand", "/<command>", ref mcmd, 1000))
            {
                config.MasterCommand.Content = mcmd;
            }
            ImGui.EndDisabled();

            // Zone list.
            ImGui.SetNextItemWidth(-1);
            ImGui.InputTextWithHint("##Search", "Search...", ref this.searchQuery, 100);
            var filteredTerritories = TerritoryList.Where(x => x.Value.Contains(this.searchQuery, StringComparison.InvariantCultureIgnoreCase));
            if (filteredTerritories.Any())
            {
                ImGui.BeginDisabled(!config.PluginEnabled && !config.SelectAll);
                if (ImGui.BeginTable("##SettingsTable", 4, ImGuiTableFlags.ScrollY))
                {
                    ImGui.TableSetupScrollFreeze(0, 1);
                    ImGui.TableSetupColumn("Zone");
                    ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed, 100);
                    ImGui.TableSetupColumn("Command");
                    ImGui.TableHeadersRow();
                    foreach (var t in filteredTerritories)
                    {
                        var customCommand = config.ZoneCommands.GetValueOrDefault(t.Key, new());

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

                        var cmd = customCommand.Content;
                        var enabled = customCommand.Enabled;
                        if (ImGui.InputTextWithHint($"##{t.Key}CommandCmd", "/<command>", ref cmd, 1000))
                        {
                            unsafe
                            {
                                customCommand.Content = cmd;
                                customCommand.Enabled = config.SelectAll || enabled;
                                config.ZoneCommands[t.Key] = customCommand;
                                TravelTriggers.PluginConfiguration.Save();
                            }
                        }

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
