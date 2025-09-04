using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using TravelTriggers.Configuration;

namespace TravelTriggers.UI.Windows
{
    public sealed class SettingsWindow : Window
    {
        public SettingsWindow() : base(TravelTriggers.PluginInterface.Manifest.Name)
        {
            this.SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(850, 150),
                MaximumSize = new Vector2(1200, 1000)
            };
            this.Size = new Vector2(850, 150);
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

            var slot = config.MasterCommand.Content;
            if (ImGui.InputTextWithHint($"MasterCommandSlot", "/command here...", ref slot, 100, 0))
            {
                unsafe
                {
                    config.MasterCommand.Content = slot;
                    TravelTriggers.PluginConfiguration.Save();
                }
            }

        }
    }
}
