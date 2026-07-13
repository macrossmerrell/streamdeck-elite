using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using BarRaider.SdTools;
using EliteJournalReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elite.Buttons
{
    [PluginActionId("com.mhwlng.elite.galaxysearch")]
    public class GalaxySearch : EliteKeypadBase
    {
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                return new PluginSettings
                {
                    BackgroundImageFilename = string.Empty,
                    Service = "inara",
                    ServiceColor = "#00aaff",
                    ServiceVerticalPosition = "14",
                    LocationColor = "#ffffff",
                    LocationVerticalPosition = "80",
                    TextBold = "true"
                };
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "backgroundImage")]
            public string BackgroundImageFilename { get; set; }

            [JsonProperty(PropertyName = "service")]
            public string Service { get; set; }

            [JsonProperty(PropertyName = "serviceColor")]
            public string ServiceColor { get; set; }

            [JsonProperty(PropertyName = "serviceVerticalPosition")]
            public string ServiceVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "locationColor")]
            public string LocationColor { get; set; }

            [JsonProperty(PropertyName = "locationVerticalPosition")]
            public string LocationVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "textBold")]
            public string TextBold { get; set; }
        }

        private PluginSettings settings;
        private Bitmap _backgroundImage = null;
        private string _backgroundFile  = null;
        private SolidBrush _serviceBrush  = new SolidBrush(Color.FromArgb(0, 170, 255));
        private SolidBrush _locationBrush = new SolidBrush(Color.White);

        private string _currentStation = null;
        private string _currentBody    = null;

        // ── URL builder ──────────────────────────────────────────────────────────

        private string BuildUrl()
        {
            var system  = EliteData.StarSystem ?? "";
            var station = _currentStation ?? "";

            switch (settings.Service?.ToLower())
            {
                case "edsm":
                    if (!string.IsNullOrWhiteSpace(_currentStation))
                        return $"https://www.edsm.net/en/system?systemName={Uri.EscapeDataString(system)}#stations";
                    return $"https://www.edsm.net/en/system?systemName={Uri.EscapeDataString(system)}";

                case "spansh":
                    // Spansh search term goes in the path, not as a query parameter
                    if (!string.IsNullOrWhiteSpace(_currentStation))
                        return $"https://spansh.co.uk/search/{Uri.EscapeDataString(station)}";
                    return $"https://spansh.co.uk/search/{Uri.EscapeDataString(system)}";

                default: // inara
                    if (!string.IsNullOrWhiteSpace(_currentStation))
                    {
                        var q = Uri.EscapeDataString($"{station} [{system}]");
                        return $"https://inara.cz/elite/station/?search={q}";
                    }
                    return $"https://inara.cz/elite/starsystem/?search={Uri.EscapeDataString(system)}";
            }
        }

        // ── Label helpers ─────────────────────────────────────────────────────────

        private string ServiceLabel()
        {
            switch (settings.Service?.ToLower())
            {
                case "edsm":   return "EDSM";
                case "spansh": return "SPANSH";
                default:       return "INARA";
            }
        }

        private string LocationLabel()
        {
            if (!string.IsNullOrWhiteSpace(_currentStation)) return "STATION";
            if (!string.IsNullOrWhiteSpace(_currentBody))    return "BODY";
            return "SYSTEM";
        }

        // ── Text rendering ────────────────────────────────────────────────────────
        // Matches the Gravity.cs scaling pattern: font shrinks until it fits width,
        // position uses verticalPosition * (width / 256.0) to scale to any bitmap size.

        private void DrawScaledText(Graphics graphics, string text, SolidBrush brush,
                                    double verticalPosition, int width)
        {
            if (string.IsNullOrEmpty(text)) return;

            var isBold    = settings.TextBold == "true";
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;

            for (int size = 24; size >= 8; size--)
            {
                using (var font = new Font("Arial", size, fontStyle))
                {
                    var measuredSize = graphics.MeasureString(text, font);

                    if (measuredSize.Width <= width * 0.92f)
                    {
                        var x = (width - measuredSize.Width) / 2.0f;
                        var y = (float)(verticalPosition * (width / 256.0));
                        graphics.DrawString(text, font, brush, x, y);
                        return;
                    }
                }
            }
        }

        // ── Display ───────────────────────────────────────────────────────────────

        private async Task HandleDisplay()
        {
            string imgBase64 = null;
            try
            {
                // Start from background image if set, otherwise solid black
                Bitmap baseBitmap;
                if (_backgroundImage != null)
                    baseBitmap = new Bitmap(_backgroundImage);  // copy so we don't draw on the cached original
                else
                    baseBitmap = new Bitmap(256, 256);

                using (baseBitmap)
                using (var graphics = Graphics.FromImage(baseBitmap))
                {
                    if (_backgroundImage == null)
                        graphics.Clear(Color.Black);

                    var width  = baseBitmap.Width;
                    var svcPos = double.TryParse(settings.ServiceVerticalPosition,  out double sp) ? sp : 14.0;
                    var locPos = double.TryParse(settings.LocationVerticalPosition, out double lp) ? lp : 80.0;

                    DrawScaledText(graphics, ServiceLabel(),  _serviceBrush,  svcPos, width);
                    DrawScaledText(graphics, LocationLabel(), _locationBrush, locPos, width);

                    imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(baseBitmap, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "GalaxySearch HandleDisplay " + ex);
            }

            if (imgBase64 != null)
                await Connection.SetImageAsync(imgBase64);
        }

        // ── Constructor ───────────────────────────────────────────────────────────

        public GalaxySearch(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
            }
            else
            {
                settings = payload.Settings.ToObject<PluginSettings>();
                InitializeSettings();
            }

            // Clear any SD-native title so our bitmap is always in control
            Connection.SetTitleAsync(null).Wait();

            Program.JournalWatcher.MessageReceived += HandleEliteEvents;
            AsyncHelper.RunSync(HandleDisplay);
        }

        // ── Journal events ────────────────────────────────────────────────────────

        public void HandleEliteEvents(object sender, MessageReceivedEventArgs args)
        {
            var e = args.EventArgs;
                        var evt = ((JournalEventArgs)e).OriginalEvent.Value<string>("event");
            if (string.IsNullOrWhiteSpace(evt)) return;

            switch (evt)
            {
                case "Location":
                    var locRaw = ((JournalEventArgs)e).OriginalEvent;
                    _currentStation = (locRaw.Value<bool?>("Docked") ?? false)
                        ? locRaw.Value<string>("StationName") : null;
                    _currentBody = null;
                    break;

                case "Docked":
                    _currentStation = ((JournalEventArgs)e).OriginalEvent.Value<string>("StationName");
                    _currentBody    = null;
                    break;

                case "Undocked":
                case "SupercruiseEntry":
                    _currentStation = null;
                    _currentBody    = null;
                    break;

                case "ApproachBody":
                    _currentBody = ((JournalEventArgs)e).OriginalEvent.Value<string>("Body");
                    break;

                case "LeaveBody":
                case "FSDJump":
                case "CarrierJump":
                    _currentStation = null;
                    _currentBody    = null;
                    break;
            }

            AsyncHelper.RunSync(HandleDisplay);
        }

        // ── Key press ─────────────────────────────────────────────────────────────

        public override void KeyPressed(KeyPayload payload)
        {
            try
            {
                Process.Start(new ProcessStartInfo(BuildUrl()) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "GalaxySearch KeyPressed " + ex);
            }
        }

        public override void KeyReleased(KeyPayload payload) { }

        // ── Lifecycle ─────────────────────────────────────────────────────────────

        public override void Dispose()
        {
            base.Dispose();
            Program.JournalWatcher.MessageReceived -= HandleEliteEvents;
        }

        public override async void OnTick()
        {
            base.OnTick();
            await HandleDisplay();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            BarRaider.SdTools.Tools.AutoPopulateSettings(settings, payload.Settings);
            InitializeSettings();
            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
            AsyncHelper.RunSync(HandleDisplay);
        }

        // ── Settings init ─────────────────────────────────────────────────────────

        private void InitializeSettings()
        {
            if (string.IsNullOrEmpty(settings.Service))
                settings.Service = "inara";
            if (string.IsNullOrEmpty(settings.ServiceColor))
                settings.ServiceColor = "#00aaff";
            if (string.IsNullOrEmpty(settings.ServiceVerticalPosition))
                settings.ServiceVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.LocationColor))
                settings.LocationColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.LocationVerticalPosition))
                settings.LocationVerticalPosition = "80";
            if (string.IsNullOrEmpty(settings.TextBold))
                settings.TextBold = "true";

            try
            {
                var converter = new ColorConverter();
                _serviceBrush?.Dispose();
                _locationBrush?.Dispose();
                _serviceBrush  = new SolidBrush((Color)converter.ConvertFromString(settings.ServiceColor));
                _locationBrush = new SolidBrush((Color)converter.ConvertFromString(settings.LocationColor));

                // Reload background image
                if (_backgroundImage != null) { _backgroundImage.Dispose(); _backgroundImage = null; _backgroundFile = null; }

                if (File.Exists(settings.BackgroundImageFilename))
                {
                    _backgroundImage = (Bitmap)Image.FromFile(settings.BackgroundImageFilename);
                    _backgroundFile  = Tools.FileToBase64(settings.BackgroundImageFilename, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "GalaxySearch InitializeSettings " + ex);
            }
        }
    }
}
