using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using BarRaider.SdTools;
using EliteJournalReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Elite.Buttons
{
    [PluginActionId("com.mhwlng.elite.latlonginfo")]
    public class LatLongInfo : EliteKeypadBase
    {
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                return new PluginSettings
                {
                    PrimaryImageFilename = string.Empty,
                    DefaultImageFilename = string.Empty,
                    LatColor = "#00ff00",
                    LongColor = "#00aaff",
                    LatVerticalPosition = "28",
                    LongVerticalPosition = "128",
                    TextBold = "true"
                };
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "primaryImage")]
            public string PrimaryImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "defaultImage")]
            public string DefaultImageFilename { get; set; }

            [JsonProperty(PropertyName = "latColor")]
            public string LatColor { get; set; }

            [JsonProperty(PropertyName = "longColor")]
            public string LongColor { get; set; }

            [JsonProperty(PropertyName = "latVerticalPosition")]
            public string LatVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "longVerticalPosition")]
            public string LongVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "textBold")]
            public string TextBold { get; set; }
        }

        private PluginSettings settings;
        private long _lastDrawnVersion = -1;
        private int _ticksSinceDraw;
        private Bitmap _primaryImage = null;
        private Bitmap _defaultImage = null;
        private string _primaryFile;
        private string _defaultFile;
        private SolidBrush _latBrush = new SolidBrush(Color.Lime);
        private SolidBrush _longBrush = new SolidBrush(Color.FromArgb(0, 170, 255));

        private async Task HandleDisplay()
        {
            _lastDrawnVersion = EliteData.DataVersion;
            _ticksSinceDraw = 0;

            var s = EliteData.StatusData;

            if (!s.HasLatLong)
            {
                if (!string.IsNullOrEmpty(_defaultFile))
                    await Connection.SetImageAsync(_defaultFile);
                return;
            }

            var myBitmap = _primaryImage ?? _defaultImage;
            var imgBase64 = _primaryFile ?? _defaultFile;

            var latText = $"{s.Latitude:F4}°";
            var longText = $"{s.Longitude:F4}°";

            try
            {
                using (var bitmap = myBitmap != null ? new Bitmap(myBitmap) : new Bitmap(256, 256))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        if (myBitmap == null)
                            graphics.Clear(Color.Black);

                        var width = bitmap.Width;
                        var latPos = double.TryParse(settings.LatVerticalPosition, out double lp) ? lp : 28.0;
                        var longPos = double.TryParse(settings.LongVerticalPosition, out double lop) ? lop : 128.0;

                        // Budget each block's height to the actual gap between the two configured
                        // positions (scaled to this bitmap), so the font can grow as large as the
                        // user's own layout allows instead of a fixed guess.
                        var latBudget = (float)((longPos > latPos ? longPos - latPos : 100.0) * (width / 256.0));
                        var longBudget = (float)((256.0 - longPos > 0 ? 256.0 - longPos : 100.0) * (width / 256.0));

                        TextFit.DrawLabelAndValue(graphics, "LAT", latText, _latBrush, latPos, width, latBudget, settings.TextBold == "true");
                        TextFit.DrawLabelAndValue(graphics, "LONG", longText, _longBrush, longPos, width, longBudget, settings.TextBold == "true");
                    }

                    imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "LatLongInfo HandleDisplay " + ex);
            }

            await Connection.SetImageAsync(imgBase64);
        }

        public LatLongInfo(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
                AsyncHelper.RunSync(HandleDisplay);
            }

            Program.JournalWatcher.MessageReceived += HandleEliteEvents;
        }

        public void HandleEliteEvents(object sender, MessageReceivedEventArgs args)
        {
            AsyncHelper.RunCoalesced(this, HandleDisplay);
        }

        public override void KeyPressed(KeyPayload payload) { }
        public override void KeyReleased(KeyPayload payload) { }

        public override void Dispose()
        {
            base.Dispose();
            Program.JournalWatcher.MessageReceived -= HandleEliteEvents;
        }

        public override async void OnTick()
        {
            base.OnTick();

            // Nothing this button shows can have changed since the last draw; redraw at least every 30 ticks as a safety net.
            if (_lastDrawnVersion == EliteData.DataVersion && ++_ticksSinceDraw < 30) return;

            await HandleDisplay();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            BarRaider.SdTools.Tools.AutoPopulateSettings(settings, payload.Settings);
            InitializeSettings();
            AsyncHelper.RunSync(HandleDisplay);
        }

        private void InitializeSettings()
        {
            if (string.IsNullOrEmpty(settings.LatColor)) settings.LatColor = "#00ff00";
            if (string.IsNullOrEmpty(settings.LongColor)) settings.LongColor = "#00aaff";
            if (string.IsNullOrEmpty(settings.LatVerticalPosition)) settings.LatVerticalPosition = "28";
            if (string.IsNullOrEmpty(settings.LongVerticalPosition)) settings.LongVerticalPosition = "128";
            if (string.IsNullOrEmpty(settings.TextBold)) settings.TextBold = "true";

            try
            {
                var converter = new ColorConverter();
                _latBrush = new SolidBrush((Color)converter.ConvertFromString(settings.LatColor));
                _longBrush = new SolidBrush((Color)converter.ConvertFromString(settings.LongColor));

                if (_primaryImage != null) { _primaryImage.Dispose(); _primaryImage = null; _primaryFile = null; }
                if (_defaultImage != null) { _defaultImage.Dispose(); _defaultImage = null; _defaultFile = null; }

                if (File.Exists(settings.PrimaryImageFilename))
                {
                    _primaryImage = StreamDeckCommon.LoadBitmap(settings.PrimaryImageFilename);
                    _primaryFile = Tools.FileToBase64(settings.PrimaryImageFilename, true);
                }

                if (File.Exists(settings.DefaultImageFilename))
                {
                    _defaultImage = StreamDeckCommon.LoadBitmap(settings.DefaultImageFilename);
                    _defaultFile = Tools.FileToBase64(settings.DefaultImageFilename, true);
                }
                else
                {
                    _defaultImage = _primaryImage;
                    _defaultFile = _primaryFile;
                }

                if (_primaryImage == null)
                {
                    _primaryImage = _defaultImage;
                    _primaryFile = _defaultFile;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "LatLongInfo InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}