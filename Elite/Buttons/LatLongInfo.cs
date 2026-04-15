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
        private Bitmap _primaryImage = null;
        private Bitmap _defaultImage = null;
        private string _primaryFile;
        private string _defaultFile;
        private SolidBrush _latBrush = new SolidBrush(Color.Lime);
        private SolidBrush _longBrush = new SolidBrush(Color.FromArgb(0, 170, 255));

        private void DrawLabelAndValue(Graphics graphics, string label, string value, SolidBrush brush, double verticalPosition, int width)
        {
            if (string.IsNullOrEmpty(value)) return;

            var isBold = settings.TextBold == "true";
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;

            // Try to fit both label and value lines starting from the largest font
            //for (int adjustedSize = 25; adjustedSize >= 8; adjustedSize -= 1)
            for (int adjustedSize = 20; adjustedSize >= 8; adjustedSize -= 1)
            {
                var testFont = new Font("Arial", adjustedSize, fontStyle);
                var sf = new StringFormat(StringFormat.GenericTypographic);

                // Measure label
                sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, label.Length) });
                var labelRegions = graphics.MeasureCharacterRanges(label, testFont, new RectangleF(0, 0, 1000, 1000), sf);
                var labelBounds = labelRegions[0].GetBounds(graphics);

                // Measure value
                sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, value.Length) });
                var valueRegions = graphics.MeasureCharacterRanges(value, testFont, new RectangleF(0, 0, 1000, 1000), sf);
                var valueBounds = valueRegions[0].GetBounds(graphics);

                bool fits = labelBounds.Width <= width * 0.95f && valueBounds.Width <= width * 0.95f;

                if (fits)
                {
                    var drawFmt = new StringFormat(StringFormat.GenericTypographic);
                    float currentY = (float)(verticalPosition * (width / 256.0));

                    // Draw label
                    var lsf = new StringFormat(StringFormat.GenericTypographic);
                    lsf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, label.Length) });
                    var lr = graphics.MeasureCharacterRanges(label, testFont, new RectangleF(0, 0, 1000, 1000), lsf);
                    var lb = lr[0].GetBounds(graphics);
                    float labelX = (width - lb.Width) / 2.0f;
                    graphics.DrawString(label, testFont, brush, labelX, currentY - lb.Y, drawFmt);
                    currentY += lb.Height * 1.1f;

                    // Draw value
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

            var latText = $"{s.Latitude:F4}°";
            var longText = $"{s.Longitude:F4}°";

            try
            {
                using (var bitmap = new Bitmap(myBitmap))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var width = bitmap.Width;
                        var latPos = double.TryParse(settings.LatVerticalPosition, out double lp) ? lp : 28.0;
                        var longPos = double.TryParse(settings.LongVerticalPosition, out double lop) ? lop : 128.0;

                        DrawLabelAndValue(graphics, "LAT", latText, _latBrush, latPos, width);
                        DrawLabelAndValue(graphics, "LONG", longText, _longBrush, longPos, width);
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

            Program.JournalWatcher.AllEventHandler += HandleEliteEvents;
        }

        public void HandleEliteEvents(object sender, JournalEventArgs e)
        {
            AsyncHelper.RunSync(HandleDisplay);
        }

        public override void KeyPressed(KeyPayload payload) { }
        public override void KeyReleased(KeyPayload payload) { }

        public override void Dispose()
        {
            base.Dispose();
            Program.JournalWatcher.AllEventHandler -= HandleEliteEvents;
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
                Logger.Instance.LogMessage(TracingLevel.FATAL, "LatLongInfo InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}