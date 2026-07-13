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
        private Bitmap _primaryImage = null;
        private Bitmap _defaultImage = null;
        private string _primaryFile;
        private string _defaultFile;
        private SolidBrush _headingBrush = new SolidBrush(Color.Lime);
        private SolidBrush _altitudeBrush = new SolidBrush(Color.FromArgb(0, 170, 255));

        private void DrawLabelAndValue(Graphics graphics, string label, string value, SolidBrush brush, double verticalPosition, int width)
        {
            if (string.IsNullOrEmpty(value)) return;

            var isBold = settings.TextBold == "true";
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;

            for (int adjustedSize = 20; adjustedSize >= 8; adjustedSize -= 1)
            {
                var testFont = new Font("Arial", adjustedSize, fontStyle);
                var sf = new StringFormat(StringFormat.GenericTypographic);

                sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, label.Length) });
                var labelRegions = graphics.MeasureCharacterRanges(label, testFont, new RectangleF(0, 0, 1000, 1000), sf);
                var labelBounds = labelRegions[0].GetBounds(graphics);

                sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, value.Length) });
                var valueRegions = graphics.MeasureCharacterRanges(value, testFont, new RectangleF(0, 0, 1000, 1000), sf);
                var valueBounds = valueRegions[0].GetBounds(graphics);

                bool fits = labelBounds.Width <= width * 0.95f && valueBounds.Width <= width * 0.95f;

                if (fits)
                {
                    var drawFmt = new StringFormat(StringFormat.GenericTypographic);
                    float currentY = (float)(verticalPosition * (width / 256.0));

                    var lsf = new StringFormat(StringFormat.GenericTypographic);
                    lsf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, label.Length) });
                    var lr = graphics.MeasureCharacterRanges(label, testFont, new RectangleF(0, 0, 1000, 1000), lsf);
                    var lb = lr[0].GetBounds(graphics);
                    float labelX = (width - lb.Width) / 2.0f;
                    graphics.DrawString(label, testFont, brush, labelX, currentY - lb.Y, drawFmt);
                    currentY += lb.Height * 1.1f;

                    var vsf = new StringFormat(StringFormat.GenericTypographic);
                    vsf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, value.Length) });
                    var vr = graphics.MeasureCharacterRanges(value, testFont, new RectangleF(0, 0, 1000, 1000), vsf);
                    var vb = vr[0].GetBounds(graphics);
                    float valueX = (width - vb.Width) / 2.0f;
                    graphics.DrawString(value, testFont, brush, valueX, currentY - vb.Y, drawFmt);

                    testFont.Dispose();
                    return;
                }

                testFont.Dispose();
            }
        }

        private async Task HandleDisplay()
        {
            var s = EliteData.StatusData;

            if (!s.HasLatLong)
            {
                if (!string.IsNullOrEmpty(_defaultFile))
                    await Connection.SetImageAsync(_defaultFile);
                return;
            }

            var myBitmap = _primaryImage ?? _defaultImage;
            var imgBase64 = _primaryFile ?? _defaultFile;

            if (myBitmap == null)
            {
                if (!string.IsNullOrEmpty(imgBase64))
                    await Connection.SetImageAsync(imgBase64);
                return;
            }

            var headingText = $"{s.Heading:F0}°";
            var altitudeText = s.Altitude >= 3000
                ? $"{s.Altitude / 1000.0:F1}km"
                : $"{s.Altitude:F0}m";
            try
            {
                using (var bitmap = new Bitmap(myBitmap))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var width = bitmap.Width;
                        var headingPos = double.TryParse(settings.HeadingVerticalPosition, out double hp) ? hp : 28.0;
                        var altitudePos = double.TryParse(settings.AltitudeVerticalPosition, out double ap) ? ap : 128.0;

                        DrawLabelAndValue(graphics, "HDG", headingText, _headingBrush, headingPos, width);
                        DrawLabelAndValue(graphics, "ALT", altitudeText, _altitudeBrush, altitudePos, width);
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
            AsyncHelper.RunSync(HandleDisplay);
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
                    _primaryImage = (Bitmap)Image.FromFile(settings.PrimaryImageFilename);
                    _primaryFile = Tools.FileToBase64(settings.PrimaryImageFilename, true);
                }

                if (File.Exists(settings.DefaultImageFilename))
                {
                    _defaultImage = (Bitmap)Image.FromFile(settings.DefaultImageFilename);
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