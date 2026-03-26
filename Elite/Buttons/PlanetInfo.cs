using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using BarRaider.SdTools;
using EliteJournalReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable StringLiteralTypo

namespace Elite.Buttons
{
    [PluginActionId("com.mhwlng.elite.planetinfo")]
    public class PlanetInfo : EliteKeypadBase
    {
        // Atmosphere abbreviation lookup
        private static readonly Dictionary<string, string> AtmosphereAbbreviations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "silicate vapour",        "SILICATE\nVAPOUR" },
            { "silicate vapor",         "SILICATE\nVAPOUR" },
            { "oxygen",                 "OXYGEN" },
            { "ammonia",                "AMMONIA" },
            { "nitrogen",               "NITROGEN" },
            { "methane",                "METHANE" },
            { "argon",                  "ARGON" },
            { "water",                  "WATER" },
            { "sulphur dioxide",        "SULPHUR\nDIOXIDE" },
            { "sulfur dioxide",         "SULPHUR\nDIOXIDE" },
            { "neon rich",              "NEON" },
            { "argon rich",             "ARGON" },
            { "carbon dioxide rich",    "CARBON\nDIOXIDE" },
            { "carbon dioxide",         "CARBON\nDIOXIDE" },
            { "helium",                 "HELIUM" },
            { "neon",                   "NEON" },
            { "metallic vapour",        "METALLIC\nVAPOUR" },
            { "metallic vapor",         "METALLIC\nVAPOUR" },
            { "no atmosphere",          "NONE" },
            { "",                       "NONE" }
        };

        private static string AbbreviateAtmosphere(string atmosphere)
        {
            if (string.IsNullOrWhiteSpace(atmosphere)) return "NONE";

            // Strip "thin " and "atmosphere" variations
            var cleaned = atmosphere.Trim();
            if (cleaned.StartsWith("thin ", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned.Substring(5).Trim();
            if (cleaned.EndsWith(" atmosphere", StringComparison.OrdinalIgnoreCase))
                cleaned = cleaned.Substring(0, cleaned.Length - 11).Trim();
            if (cleaned.Equals("atmosphere", StringComparison.OrdinalIgnoreCase))
                return "NONE";

            if (AtmosphereAbbreviations.TryGetValue(cleaned, out var abbr))
                return abbr;

            // Fallback - uppercase, split at first space if multi-word
            var upper = cleaned.ToUpper();
            var spaceIdx = upper.IndexOf(' ');
            if (spaceIdx > 0)
                return upper.Substring(0, spaceIdx) + "\n" + upper.Substring(spaceIdx + 1);
            return upper;
        }

        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    PrimaryImageFilename = string.Empty,
                    DefaultImageFilename = string.Empty,
                    AtmosphereColor = "#ffffff",
                    TemperatureColor = "#ff8800",
                    AtmosphereVerticalPosition = "28",
                    TemperatureVerticalPosition = "128",
                    TextBold = "true"
                };
                return instance;
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "primaryImage")]
            public string PrimaryImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "defaultImage")]
            public string DefaultImageFilename { get; set; }

            [JsonProperty(PropertyName = "atmosphereColor")]
            public string AtmosphereColor { get; set; }

            [JsonProperty(PropertyName = "temperatureColor")]
            public string TemperatureColor { get; set; }

            [JsonProperty(PropertyName = "atmosphereVerticalPosition")]
            public string AtmosphereVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "temperatureVerticalPosition")]
            public string TemperatureVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "textBold")]
            public string TextBold { get; set; }
        }

        private PluginSettings settings;
        private Bitmap _primaryImage = null;
        private Bitmap _defaultImage = null;
        private string _primaryFile;
        private string _defaultFile;
        private SolidBrush _atmosphereBrush = new SolidBrush(Color.White);
        private SolidBrush _temperatureBrush = new SolidBrush(Color.FromArgb(255, 136, 0));

        private void DrawAtmosphereText(Graphics graphics, string text, SolidBrush brush, double verticalPosition, int width)
        {
            if (string.IsNullOrEmpty(text)) return;

            var isBold = settings.TextBold == "true";
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;
            var lines = text.Split('\n');

            for (int adjustedSize = 25; adjustedSize >= 10; adjustedSize -= 1)
            {
                var testFont = new Font("Arial", adjustedSize, fontStyle);
                bool fits = true;
                var lineWidths = new float[lines.Length];
                var lineHeights = new float[lines.Length];

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrEmpty(line)) { lineWidths[i] = 0; lineHeights[i] = testFont.Height; continue; }

                    var sf = new StringFormat(StringFormat.GenericTypographic);
                    sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, line.Length) });
                    var regions = graphics.MeasureCharacterRanges(line, testFont, new RectangleF(0, 0, 1000, 1000), sf);
                    var bounds = regions[0].GetBounds(graphics);
                    lineWidths[i] = bounds.Width;
                    lineHeights[i] = bounds.Height;

                    if (bounds.Width > width * 0.90f)
                    {
                        fits = false;
                        break;
                    }
                }

                if (fits)
                {
                    var drawFmt = new StringFormat(StringFormat.GenericTypographic);
                    float currentY = (float)(verticalPosition * (width / 256.0));

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var sf2 = new StringFormat(StringFormat.GenericTypographic);
                        sf2.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, lines[i].Length) });
                        var regions2 = graphics.MeasureCharacterRanges(lines[i], testFont, new RectangleF(0, 0, 1000, 1000), sf2);
                        var b = regions2[0].GetBounds(graphics);
                        var x = (width - b.Width) / 2.0f;
                        // b.Y is offset from draw origin to visual top — subtract to align visual top to currentY
                        graphics.DrawString(lines[i], testFont, brush, x, currentY - b.Y, drawFmt);
                        currentY += b.Height * 1.1f;
                    }
                    testFont.Dispose();
                    return;
                }

                testFont.Dispose();
            }
        }

        private void DrawTemperatureText(Graphics graphics, string text, SolidBrush brush, double verticalPosition, int width)
        {
            if (string.IsNullOrEmpty(text)) return;

            var fontContainerHeight = 100 * (width / 256.0);
            var isBold = settings.TextBold == "true";
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;

            for (int adjustedSize = 100; adjustedSize >= 10; adjustedSize -= 5)
            {
                var testFont = new Font("Arial", adjustedSize, fontStyle);
                var measuredSize = graphics.MeasureString(text, testFont);

                if (fontContainerHeight >= measuredSize.Height && measuredSize.Width <= width)
                {
                    var x = (width - measuredSize.Width) / 2.0;
                    var y = verticalPosition * (width / 256.0);
                    graphics.DrawString(text, testFont, brush, (float)x, (float)y);
                    testFont.Dispose();
                    return;
                }

                testFont.Dispose();
            }
        }

        private async Task HandleDisplay()
        {
            var s = EliteData.StatusData;

            Bitmap myBitmap = null;
            string imgBase64 = null;
            string atmosphereText = null;
            string temperatureText = null;

            if (s.HasLatLong)
            {
                // Near planet - use primary image
                myBitmap = _primaryImage;
                imgBase64 = _primaryFile;

                if (!string.IsNullOrEmpty(s.BodyName) &&
                    EliteData.GravityCache.TryGetValue(s.BodyName, out var cached))
                {
                    atmosphereText = AbbreviateAtmosphere(cached.Atmosphere);

                    // Use live temperature if on foot, otherwise use cached surface temperature
                    double temp = (s.OnFoot && s.Temperature > 0) ? s.Temperature : cached.SurfaceTemperature;
                    if (temp > 0)
                        temperatureText = $"{temp:F0}K";
                }
                else
                {
                    atmosphereText = "?";
                    temperatureText = "?K";
                }
            }
            else if (!string.IsNullOrEmpty(s.BodyName) &&
                     EliteData.GravityCache.TryGetValue(s.BodyName, out var cachedBody))
            {
                // Not near planet but have cached data - show on default image
                myBitmap = _defaultImage;
                imgBase64 = _defaultFile;
                atmosphereText = AbbreviateAtmosphere(cachedBody.Atmosphere);
                if (cachedBody.SurfaceTemperature > 0)
                    temperatureText = $"{cachedBody.SurfaceTemperature:F0}K";
            }
            else
            {
                // No data - show default image only
                if (!string.IsNullOrEmpty(_defaultFile))
                    await Connection.SetImageAsync(_defaultFile);
                return;
            }

            if (myBitmap == null)
            {
                if (!string.IsNullOrEmpty(imgBase64))
                    await Connection.SetImageAsync(imgBase64);
                return;
            }

            try
            {
                using (var bitmap = new Bitmap(myBitmap))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var width = bitmap.Width;
                        var atmPos = double.TryParse(settings.AtmosphereVerticalPosition, out double ap) ? ap : 28.0;
                        var tempPos = double.TryParse(settings.TemperatureVerticalPosition, out double tp) ? tp : 128.0;

                        DrawAtmosphereText(graphics, atmosphereText, _atmosphereBrush, atmPos, width);
                        DrawTemperatureText(graphics, temperatureText, _temperatureBrush, tempPos, width);
                    }

                    imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "PlanetInfo HandleDisplay " + ex);
            }

            await Connection.SetImageAsync(imgBase64);
        }

        public PlanetInfo(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            if (string.IsNullOrEmpty(settings.AtmosphereColor))
                settings.AtmosphereColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.TemperatureColor))
                settings.TemperatureColor = "#ff8800";
            if (string.IsNullOrEmpty(settings.AtmosphereVerticalPosition))
                settings.AtmosphereVerticalPosition = "28";
            if (string.IsNullOrEmpty(settings.TemperatureVerticalPosition))
                settings.TemperatureVerticalPosition = "128";
            if (string.IsNullOrEmpty(settings.TextBold))
                settings.TextBold = "true";

            try
            {
                var converter = new ColorConverter();
                _atmosphereBrush = new SolidBrush((Color)converter.ConvertFromString(settings.AtmosphereColor));
                _temperatureBrush = new SolidBrush((Color)converter.ConvertFromString(settings.TemperatureColor));

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
                Logger.Instance.LogMessage(TracingLevel.FATAL, "PlanetInfo InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}
