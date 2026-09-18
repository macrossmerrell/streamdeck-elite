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
    [PluginActionId("com.mhwlng.elite.headingaltitude")]
    public class HeadingAltitude : EliteKeypadBase
    {
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                return new PluginSettings
                {
                    PrimaryImageFilename = string.Empty,
                    DefaultImageFilename = string.Empty,
                    HeadingColor = "#00ff00",
                    AltitudeColor = "#00aaff",
                    HeadingVerticalPosition = "28",
                    AltitudeVerticalPosition = "128",
                    TextBold = "true"
                };
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "primaryImage")]
            public string PrimaryImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "defaultImage")]
            public string DefaultImageFilename { get; set; }

            [JsonProperty(PropertyName = "headingColor")]
            public string HeadingColor { get; set; }

            [JsonProperty(PropertyName = "altitudeColor")]
            public string AltitudeColor { get; set; }

            [JsonProperty(PropertyName = "headingVerticalPosition")]
            public string HeadingVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "altitudeVerticalPosition")]
            public string AltitudeVerticalPosition { get; set; }

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
        private SolidBrush _headingBrush = new SolidBrush(Color.Lime);
        private SolidBrush _altitudeBrush = new SolidBrush(Color.FromArgb(0, 170, 255));

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

            var headingText = $"{s.Heading:F0}°";
            var altitudeText = s.Altitude >= 3000
                ? $"{s.Altitude / 1000.0:F1}km"
                : $"{s.Altitude:F0}m";
            try
            {
                using (var bitmap = myBitmap != null ? new Bitmap(myBitmap) : new Bitmap(256, 256))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        if (myBitmap == null)
                            graphics.Clear(Color.Black);

                        var width = bitmap.Width;
                        var headingPos = double.TryParse(settings.HeadingVerticalPosition, out double hp) ? hp : 28.0;
                        var altitudePos = double.TryParse(settings.AltitudeVerticalPosition, out double ap) ? ap : 128.0;

                        // Budget each block's height to the actual gap between the two configured
                        // positions (scaled to this bitmap), so the font can grow as large as the
                        // user's own layout allows instead of a fixed guess.
                        var headingBudget = (float)((altitudePos > headingPos ? altitudePos - headingPos : 100.0) * (width / 256.0));
                        var altitudeBudget = (float)((256.0 - altitudePos > 0 ? 256.0 - altitudePos : 100.0) * (width / 256.0));

                        TextFit.DrawLabelAndValue(graphics, "HDG", headingText, _headingBrush, headingPos, width, headingBudget, settings.TextBold == "true");
                        TextFit.DrawLabelAndValue(graphics, "ALT", altitudeText, _altitudeBrush, altitudePos, width, altitudeBudget, settings.TextBold == "true");
                    }

                    imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "HeadingAltitude HandleDisplay " + ex);
            }

            await Connection.SetImageAsync(imgBase64);
        }

        public HeadingAltitude(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            if (string.IsNullOrEmpty(settings.HeadingColor)) settings.HeadingColor = "#00ff00";
            if (string.IsNullOrEmpty(settings.AltitudeColor)) settings.AltitudeColor = "#00aaff";
            if (string.IsNullOrEmpty(settings.HeadingVerticalPosition)) settings.HeadingVerticalPosition = "28";
            if (string.IsNullOrEmpty(settings.AltitudeVerticalPosition)) settings.AltitudeVerticalPosition = "128";
            if (string.IsNullOrEmpty(settings.TextBold)) settings.TextBold = "true";

            try
            {
                var converter = new ColorConverter();
                _headingBrush = new SolidBrush((Color)converter.ConvertFromString(settings.HeadingColor));
                _altitudeBrush = new SolidBrush((Color)converter.ConvertFromString(settings.AltitudeColor));

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
                Logger.Instance.LogMessage(TracingLevel.FATAL, "HeadingAltitude InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}