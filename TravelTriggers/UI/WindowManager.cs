using System;
using System.Linq;
using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using TravelTriggers.Configuration;
using TravelTriggers.UI.Windows;

namespace TravelTriggers.UI
{
    internal sealed class WindowManager : IDisposable
    {
        private bool disposedValue;

        /// <summary>
        ///     All windows to add to the windowing system, holds all references.
        /// </summary>
        private readonly Window[] windows = [new SettingsWindow()];

        /// <summary>
        ///     The windowing system.
        /// </summary>
        private readonly WindowSystem windowingSystem;

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowManager" /> class.
        /// </summary>
        public WindowManager()
        {
            this.windowingSystem = new WindowSystem(TravelTriggers.PluginInterface.Manifest.InternalName);
            foreach (var window in this.windows)
            {
                this.windowingSystem.AddWindow(window);
            }
            TravelTriggers.PluginInterface.UiBuilder.Draw += this.windowingSystem.Draw;
            TravelTriggers.ClientState.Login += this.OnLogin;
            TravelTriggers.ClientState.Logout += this.OnLogout;
            TravelTriggers.PluginInterface.UiBuilder.OpenConfigUi += this.ToggleConfigWindow;
            TravelTriggers.PluginInterface.UiBuilder.OpenMainUi += this.ToggleConfigWindow;
            if (!TravelTriggers.PluginConfiguration.CharacterConfigurations.TryGetValue(TravelTriggers.PlayerState.ContentId, out var config))
            {
                config = new();
                TravelTriggers.PluginConfiguration.CharacterConfigurations[TravelTriggers.PlayerState.ContentId] = config;
            }
            if (config.PluginEnabled && config.ShowInDtr)
            {
                TravelTriggers.DtrEntry ??= Svc.DtrBar.Get("TravelTriggers");
                TravelTriggers.DtrEntry.OnClick += this.OnDtrInteractionEvent;
                UpdateDtrEntry(config);
            }
        }

        private void OnDtrInteractionEvent(DtrInteractionEvent @event)
        {
            if (@event == null)
            {
                return;
            }

            this.ToggleConfigWindow();
        }

        /// <summary>
        ///     Disposes of the window manager.
        /// </summary>
        public void Dispose()
        {
            if (this.disposedValue)
            {
                ObjectDisposedException.ThrowIf(this.disposedValue, nameof(this.windowingSystem));
                return;
            }
            TravelTriggers.ClientState.Login -= this.OnLogin;
            TravelTriggers.ClientState.Logout -= this.OnLogout;
            TravelTriggers.PluginInterface.UiBuilder.OpenConfigUi -= this.ToggleConfigWindow;
            TravelTriggers.PluginInterface.UiBuilder.OpenMainUi -= this.ToggleConfigWindow;
            TravelTriggers.PluginInterface.UiBuilder.Draw -= this.windowingSystem.Draw;
            this.windowingSystem.RemoveAllWindows();
            foreach (var disposable in this.windows.OfType<IDisposable>())
            {
                disposable.Dispose();
            }
            this.disposedValue = true;
        }

        /// <summary>
        ///     Toggles the open state of the configuration window.
        /// </summary>
        public void ToggleConfigWindow()
        {
            ObjectDisposedException.ThrowIf(this.disposedValue, nameof(this.windowingSystem));
            this.windows.FirstOrDefault(window => window is SettingsWindow)?.Toggle();
        }

        private void OnLogin()
        {
            TravelTriggers.PluginInterface.UiBuilder.OpenConfigUi += this.ToggleConfigWindow;
            TravelTriggers.PluginInterface.UiBuilder.OpenMainUi += this.ToggleConfigWindow;
        }
        private void OnLogout(int type, int code)
        {
            TravelTriggers.PluginInterface.UiBuilder.OpenConfigUi -= this.ToggleConfigWindow;
            TravelTriggers.PluginInterface.UiBuilder.OpenMainUi += this.ToggleConfigWindow;
        }

        /// <summary>
        /// Updates the plugin DTR element (Server Info Text)
        /// </summary>
        /// <param name="config"></param>
        public static void UpdateDtrEntry(CharacterConfiguration? config)
        {
            if (config is null)
            {
                return;
            }

            if (config.PluginEnabled && config.ShowInDtr)
            {
                TravelTriggers.DtrEntry ??= Svc.DtrBar.Get("TravelTriggers");
                TravelTriggers.DtrEntry.Text = new SeString(
                        new IconPayload(BitmapFontIcon.RolePlaying),
                        new IconPayload(config.RoleplayOnly ? BitmapFontIcon.GreenDot : BitmapFontIcon.NoCircle),
                        //new TextPayload(config.RoleplayOnly ? "On" : "Off"),
                        new IconPayload(BitmapFontIcon.Dice),
                        new IconPayload(config.EnableRNG ? BitmapFontIcon.GreenDot : BitmapFontIcon.NoCircle),
                        //new TextPayload(config.EnableRNG ? "On" : "Off"),
                        new IconPayload(BitmapFontIcon.Aetheryte),
                        new IconPayload(config.EnableTerritoryMode ? BitmapFontIcon.GreenDot : BitmapFontIcon.NoCircle),
                        //new TextPayload(config.EnableTerritoryMode ? "On" : "Off"),
                        new IconPayload(BitmapFontIcon.SwordSheathed),
                        new IconPayload(config.EnableGearsetSwap ? BitmapFontIcon.GreenDot : BitmapFontIcon.NoCircle),
                        //new TextPayload(config.EnableGearsetSwap ? "On" : "Off"),
                        new IconPayload(BitmapFontIcon.Mentor),
                        new IconPayload(config.EnableOverride ? BitmapFontIcon.GreenDot : BitmapFontIcon.NoCircle)
                        //new TextPayload(config.EnableOverride ? "On" : "Off")
                        );
                TravelTriggers.DtrEntry.Shown = true;
            }
        }
    }
}
