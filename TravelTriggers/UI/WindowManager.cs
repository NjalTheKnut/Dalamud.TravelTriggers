using Dalamud.Game.Gui.Dtr;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.Windowing;
using ECommons.DalamudServices;
using ECommons.Logging;
using TravelTriggers.Helpers;
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

        public IDtrBarEntry RpEntry { get; } = Svc.DtrBar.Get("TTrig-RP");

        public IDtrBarEntry RngEntry { get; } = Svc.DtrBar.Get("TTrig-RNG");

        public IDtrBarEntry TpEntry { get; } = Svc.DtrBar.Get("TTrig-TP");

        public IDtrBarEntry GsEntry { get; } = Svc.DtrBar.Get("TTrig-GS");

        public IDtrBarEntry OcmdEntry { get; } = Svc.DtrBar.Get("TTrig-OCMD");

        //private readonly IDtrBarEntry _Entry = Svc.DtrBar.Get("TTrig-");

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
            var config = Utils.GetCharacterConfig();
            if (config.PluginEnabled && config.ShowInDtr)
            {
                this.UpdateDtrEntry();
                this.RpEntry.OnClick = ev =>
                {
                    if (ev.ClickType == MouseClickType.Right)
                    {
                        this.ToggleConfigWindow();
                    }
                    else
                    {
                        config.EnableRpOnly = !config.EnableRpOnly;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Roleplay Only Module {(config.EnableRpOnly ? "Enabled" : "Disabled")}");
                        this.UpdateDtrEntry();
                    }
                };
                this.RngEntry.OnClick = ev =>
                {
                    if (ev.ClickType == MouseClickType.Right)
                    {
                        this.ToggleConfigWindow();
                    }
                    else
                    {
                        config.EnableRNG = !config.EnableRNG;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers RNG Module {(config.EnableRNG ? "Enabled" : "Disabled")}");
                        this.UpdateDtrEntry();
                    }
                };
                this.TpEntry.OnClick = ev =>
                {
                    if (ev.ClickType == MouseClickType.Right)
                    {
                        this.ToggleConfigWindow();
                    }
                    else
                    {
                        config.EnableZones = !config.EnableZones;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Territory Module {(config.EnableZones ? "Enabled" : "Disabled")}");
                        this.UpdateDtrEntry();
                    }
                };
                this.GsEntry.OnClick = ev =>
                {
                    if (ev.ClickType == MouseClickType.Right)
                    {
                        this.ToggleConfigWindow();
                    }
                    else
                    {
                        config.EnableGset = !config.EnableGset;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Gearset Module {(config.EnableGset ? "Enabled" : "Disabled")}");
                        this.UpdateDtrEntry();
                    }
                };
                this.OcmdEntry.OnClick = ev =>
                {
                    if (ev.ClickType == MouseClickType.Right)
                    {
                        this.ToggleConfigWindow();
                    }
                    else
                    {
                        config.EnableOcmd = !config.EnableOcmd;
                        TravelTriggers.PluginConfiguration.Save();
                        PluginLog.Information($"TravelTriggers Command Override Module {(config.EnableOcmd ? "Enabled" : "Disabled")}");
                        this.UpdateDtrEntry();
                    }
                };
                this.UpdateDtrEntry();
            }
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
            //var mgr = TravelTriggers.WindowManager;
            this.RpEntry.Remove();
            this.RngEntry.Remove();
            this.TpEntry.Remove();
            this.GsEntry.Remove();
            this.OcmdEntry.Remove();
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
            if (TravelTriggers.ClientState.IsLoggedIn)
            {
                ObjectDisposedException.ThrowIf(this.disposedValue, nameof(this.windowingSystem));
                this.windows.FirstOrDefault(window => window is SettingsWindow)?.Toggle();
            }
        }

        /// <summary>
        ///     Handles the login event
        /// </summary>
        private void OnLogin()
        {
            var config = Utils.GetCharacterConfig();
            TravelTriggers.PluginInterface.UiBuilder.OpenConfigUi += this.ToggleConfigWindow;
            TravelTriggers.PluginInterface.UiBuilder.OpenMainUi += this.ToggleConfigWindow;
            this.UpdateDtrEntry();
        }

        /// <summary>
        ///     Handles the logout event
        /// </summary>
        /// <param name="type"></param>
        /// <param name="code"></param>
        private void OnLogout(int type, int code)
        {
            TravelTriggers.PluginInterface.UiBuilder.OpenConfigUi -= this.ToggleConfigWindow;
            TravelTriggers.PluginInterface.UiBuilder.OpenMainUi += this.ToggleConfigWindow;
        }

        /*/// <summary>
        ///     Enables a click event for the DTR (server info bar) menu entry.
        /// </summary>
        /// <param name="event"></param>
        private void OnDtrInteractionEvent(DtrInteractionEvent @event)
        {
            if (@event == null)
            {
                return;
            }
            this.ToggleConfigWindow();
        }*/

        /// <summary>
        /// Updates the plugin DTR element (Server Info Text)
        /// </summary>
        public void UpdateDtrEntry()
        {
            var config = Utils.GetCharacterConfig();
            if (config is null)
            {
                return;
            }
            /*this.RpEntry.Shown = false;
            this.RngEntry.Shown = false;
            this.TpEntry.Shown = false;
            this.GsEntry.Shown = false;
            this.OcmdEntry.Shown = false;*/
            if (config.PluginEnabled && config.ShowInDtr)
            {
                if (config.RpOnlyInDtr)
                {
                    this.RpEntry.Text = new SeString(new IconPayload(BitmapFontIcon.RolePlaying), config.EnableRpOnly ? new IconPayload(BitmapFontIcon.GreenDot) : new IconPayload(BitmapFontIcon.NoCircle));
                    this.RpEntry.Tooltip = new SeString(new TextPayload("Left Click to toggle Roleplay Only Mode."), new NewLinePayload(), new TextPayload("(Right Click opens main config)"));
                    this.RpEntry.Shown = true;
                }
                else
                {
                    this.RpEntry.Shown = false;
                }
                if (config.RngInDtr)
                {
                    this.RngEntry.Text = new SeString(new IconPayload(BitmapFontIcon.Dice), config.EnableRNG ? new IconPayload(BitmapFontIcon.GreenDot) : new IconPayload(BitmapFontIcon.NoCircle), config.EnableRNG ? new TextPayload($"{config.OddsMin}/{config.OddsMax}") : new TextPayload("??/??"));
                    this.RngEntry.Tooltip = new SeString(new TextPayload("Left Click to toggle RNG Mode."), new NewLinePayload(), new TextPayload("(Right Click opens main config)"));
                    this.RngEntry.Shown = true;
                }
                else
                {
                    this.RngEntry.Shown = false;
                }
                if (config.ZoneInDtr)
                {
                    this.TpEntry.Text = new SeString(new IconPayload(BitmapFontIcon.Aetheryte), config.EnableZones ? new IconPayload(BitmapFontIcon.GreenDot) : new IconPayload(BitmapFontIcon.NoCircle));
                    this.TpEntry.Tooltip = new SeString(new TextPayload("Left Click to toggle Zone Change Mode."), new NewLinePayload(), new TextPayload("(Right Click opens main config)"));
                    this.TpEntry.Shown = true;
                }
                else
                {
                    this.TpEntry.Shown = false;
                }
                if (config.GsetInDtr)
                {
                    this.GsEntry.Text = new SeString(new IconPayload(BitmapFontIcon.SwordSheathed), config.EnableGset ? new IconPayload(BitmapFontIcon.GreenDot) : new IconPayload(BitmapFontIcon.NoCircle));
                    this.GsEntry.Tooltip = new SeString(new TextPayload("Left Click to toggle Job Swap Mode."), new NewLinePayload(), new TextPayload("(Right Click opens main config)"));
                    this.GsEntry.Shown = true;
                }
                else
                {
                    this.GsEntry.Shown = false;
                }
                if (config.OcmdInDtr)
                {
                    this.OcmdEntry.Text = new SeString(new IconPayload(BitmapFontIcon.Mentor), config.EnableOcmd ? new IconPayload(BitmapFontIcon.GreenDot) : new IconPayload(BitmapFontIcon.NoCircle));
                    this.OcmdEntry.Tooltip = new SeString(new TextPayload("Left Click to toggle Command Override Mode."), new NewLinePayload(), new TextPayload("(Right Click opens main config)"));
                    this.OcmdEntry.Shown = true;
                }
                else
                {
                    this.OcmdEntry.Shown = false;
                }
                //TravelTriggers.WindowManager._ocmdEntry.Text = new SeString(new SeHyphenPayload(), );
                /*TravelTriggers.DtrEntry.Text = new SeString(
                        new SeHyphenPayload(),
                        config.RpOnlyInDtr ? config.EnableRpOnly ? new IconPayload(BitmapFontIcon.RolePlaying) : new IconPayload(BitmapFontIcon.NoCircle) : new SeHyphenPayload(),
                        new SeHyphenPayload(),
                        config.RngInDtr ? config.EnableRNG ? new IconPayload(BitmapFontIcon.Dice) : new IconPayload(BitmapFontIcon.NoCircle) : new SeHyphenPayload(),
                        new SeHyphenPayload(),
                        config.ZoneInDtr ? config.EnableZones ? new IconPayload(BitmapFontIcon.Aetheryte) : new IconPayload(BitmapFontIcon.NoCircle) : new SeHyphenPayload(),
                        new SeHyphenPayload(),
                        config.GsetInDtr ? config.EnableGset ? new IconPayload(BitmapFontIcon.SwordSheathed) : new IconPayload(BitmapFontIcon.NoCircle) : new SeHyphenPayload(),
                        new SeHyphenPayload(),
                        config.OcmdInDtr ? config.EnableOcmd ? new IconPayload(BitmapFontIcon.Mentor) : new IconPayload(BitmapFontIcon.NoCircle) : new SeHyphenPayload(),
                        new SeHyphenPayload()
                        );
                TravelTriggers.DtrEntry.Shown = true;*/
                //TravelTriggers.DtrEntry.OnClick += TravelTriggers.WindowManager.OnDtrInteractionEvent;
            }
            //else
            //{
            /*this.RpEntry.Shown = false;
            this.RngEntry.Shown = false;
            this.TpEntry.Shown = false;
            this.GsEntry.Shown = false;
            this.OcmdEntry.Shown = false;*/
            /*this.RpEntry.Text = new SeString(new IconPayload(BitmapFontIcon.Disconnecting));
            this.RngEntry.Text = new SeString(new IconPayload(BitmapFontIcon.Disconnecting));
            this.TpEntry.Text = new SeString(new IconPayload(BitmapFontIcon.Disconnecting));
            this.GsEntry.Text = new SeString(new IconPayload(BitmapFontIcon.Disconnecting));
            this.OcmdEntry.Text = new SeString(new IconPayload(BitmapFontIcon.Disconnecting));*/
            //TravelTriggers.DtrEntry.Text = new SeString(
            //        new SeHyphenPayload(),
            //        new IconPayload(BitmapFontIcon.Disconnecting),
            //        new SeHyphenPayload()
            //        );
            //TravelTriggers.DtrEntry.Shown = false;

            //}
        }
    }
}
