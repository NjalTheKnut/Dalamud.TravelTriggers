using System;
using System.Linq;
using Dalamud.Interface.Windowing;
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
            //if (TravelTriggers.ClientState.IsLoggedIn)
            //{
            TravelTriggers.PluginInterface.UiBuilder.OpenConfigUi += this.ToggleConfigWindow;
            TravelTriggers.PluginInterface.UiBuilder.OpenMainUi += this.ToggleConfigWindow;
            //}
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
    }
}
