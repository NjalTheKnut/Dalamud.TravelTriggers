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
        //private static readonly Dictionary<uint, string> TerritoryList = TravelTriggers.AllowedTerritories
        //    .Where(t => !string.IsNullOrEmpty(t.PlaceName.Value.Name.ExtractText())
        //        )
        //    .OrderBy(x => x.PlaceName.Value.Name.ExtractText())
        //    .ToDictionary(
        //        t => t.RowId,
        //        t => t.PlaceName.Value.Name.ExtractText()
        //    );
        //private string searchQuery = string.Empty;

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
            ImGui.SetNextItemWidth(-1);
            var mcmd = "";
            if (ImGui.InputTextWithHint($"##MasterCommand", "/<command>", ref mcmd, 1000))
            {
                config.MasterCommand.Content = mcmd;
                config.MasterCommand.Enabled = true;
                TravelTriggers.PluginConfiguration.Save();
            }
        }
    }
}
