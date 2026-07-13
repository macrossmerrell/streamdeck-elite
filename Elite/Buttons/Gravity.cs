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
    [PluginActionId("com.mhwlng.elite.gravity")]
    public class Gravity : EliteKeypadBase
    {
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    PrimaryImageFilename = string.Empty,
                    DefaultImageFilename = string.Empty,
                    HighGravityImageFilename = string.Empty,
                    PrimaryColor = "#ffffff",
                    HighGravityColor = "#ffffff",
                    TextVerticalPosition = "28",
                    TextBold = "true",
                    HighGravityThreshold = "1.0"
                };

                return instance;
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "primaryImage")]
            public string PrimaryImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "defaultImage")]
            public string DefaultImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "highGravityImage")]
            public string HighGravityImageFilename { get; set; }

            [JsonProperty(PropertyName = "primaryColor")]
            public string PrimaryColor { get; set; }

            [JsonProperty(PropertyName = "highGravityColor")]
            public string HighGravityColor { get; set; }

            [JsonProperty(PropertyName = "textVerticalPosition")]
            public string TextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "textBold")]
            public string TextBold { get; set; }

            [JsonProperty(PropertyName = "highGravityThreshold")]
            public string HighGravityThreshold { get; set; }
        }

        private PluginSettings settings;
        private Bitmap _primaryImage = null;
        private Bitmap _defaultImage = null;
        private Bitmap _highGravityImage = null;
        private string _primaryFile;
        private string _defaultFile;
        private string _highGravityFile;
        private SolidBrush _primaryBrush = new SolidBrush(Color.White);
        private SolidBrush _highGravityBrush = new SolidBrush(Color.White);

        private double CalculateGravityAtAltitude(double surfaceGravity, double planetRadius, double altitude)
        {
            if (altitude <= 0) return surfaceGravity;
            var ratio = planetRadius / (planetRadius + altitude);
            return surfaceGravity * ratio * ratio;
        }

        private async Task HandleDisplay()
        {
            var s = EliteData.StatusData;

            Bitmap myBitmap = null;
            string imgBase64 = null;
            string gravityText = null;
            SolidBrush activeBrush = _primaryBrush;

            var highGravityThreshold = double.TryParse(settings.HighGravityThreshold, out double parsedThreshold)
                ? parsedThreshold
                : 1.0;

            if (s.HasLatLong)
            {
                // In orbital cruise or on the surface — game provides BodyName and altitude.
                // Calculate real-time gravity adjusted for current altitude.
                if (!string.IsNullOrEmpty(s.BodyName) &&
                    EliteData.GravityCache.TryGetValue(s.BodyName, out var cached))
                {
                    var currentGravity = CalculateGravityAtAltitude(cached.SurfaceGravity, cached.PlanetRadius, s.Altitude);
                    gravityText = $"{currentGravity:F2}g";

                    if (cached.Landable && cached.SurfaceGravity > highGravityThreshold && _highGravityImage != null)
                    {
                        myBitmap = _highGravityImage;
                        imgBase64 = _highGravityFile;
                        activeBrush = _highGravityBrush;
                    }
                    else
                    {
                        myBitmap = _primaryImage;
                        imgBase64 = _primaryFile;
                        activeBrush = _primaryBrush;
                    }
                }
                else
                {
                    // In orbital cruise but no scan data cached yet
                    gravityText = "?g";
                    myBitmap = _primaryImage;
                    imgBase64 = _primaryFile;
                    activeBrush = _primaryBrush;
                }
            }
            else
            {
                // Not in orbital cruise / on surface.
                // Status.json only sets BodyName when HasLatLong is present, so we fall back to
                // DestinationName, which the game writes whenever a body is targeted in the
                // system map, FSS, or nav panel.
                var lookupName = !string.IsNullOrEmpty(s.BodyName)
                    ? s.BodyName
                    : s.DestinationName;

                if (!string.IsNullOrEmpty(lookupName) &&
                    EliteData.GravityCache.TryGetValue(lookupName, out var cachedBody))
                {
                    // Body is targeted and we have scan data — show cached surface gravity.
                    gravityText = $"{cachedBody.SurfaceGravity:F2}g";

                    if (cachedBody.Landable && cachedBody.SurfaceGravity > highGravityThreshold && _highGravityImage != null)
                    {
                        myBitmap = _highGravityImage;
                        imgBase64 = _highGravityFile;
                        activeBrush = _highGravityBrush;
                    }
                    else
                    {
                        myBitmap = _primaryImage;
                        imgBase64 = _primaryFile;
                        activeBrush = _primaryBrush;
                    }
                }
                else
                {
                    // No body targeted or no scan data — show default image only
                    if (!string.IsNullOrEmpty(_defaultFile))
                        await Connection.SetImageAsync(_defaultFile);
                    return;
                }
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
                        var fontContainerHeight = 100 * (width / 256.0);

                        for (int adjustedSize = 100; adjustedSize >= 10; adjustedSize -= 5)
                        {
                            var isBold = settings.TextBold == "true";
                            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;
                            var testFont = new Font("Arial", adjustedSize, fontStyle);
                            var adjustedSizeNew = graphics.MeasureString(gravityText, testFont);

                            if (fontContainerHeight >= adjustedSizeNew.Height)
                            {
                                var stringSize = graphics.MeasureString(gravityText, testFont);
                                var x = (width - stringSize.Width) / 2.0;
                                var verticalPosition = double.TryParse(settings.TextVerticalPosition, out double parsedPosition) ? parsedPosition : 28.0;
                                var y = verticalPosition * (width / 256.0);

                                graphics.DrawString(gravityText, testFont, activeBrush, (float)x, (float)y);
                                testFont.Dispose();
                                break;
                            }

                            testFont.Dispose();
                        }
                    }

                    imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "Gravity HandleDisplay " + ex);
            }

            await Connection.SetImageAsync(imgBase64);
        }

        public Gravity(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            if (string.IsNullOrEmpty(settings.PrimaryColor))
                settings.PrimaryColor = "#ffffff";

            if (string.IsNullOrEmpty(settings.HighGravityColor))
                settings.HighGravityColor = "#ffffff";

            if (string.IsNullOrEmpty(settings.TextVerticalPosition))
                settings.TextVerticalPosition = "28";

            if (string.IsNullOrEmpty(settings.TextBold))
                settings.TextBold = "true";

            if (string.IsNullOrEmpty(settings.HighGravityThreshold))
                settings.HighGravityThreshold = "1.0";

            try
            {
                var converter = new ColorConverter();
                _primaryBrush = new SolidBrush((Color)converter.ConvertFromString(settings.PrimaryColor));
                _highGravityBrush = new SolidBrush((Color)converter.ConvertFromString(settings.HighGravityColor));

                if (_primaryImage != null) { _primaryImage.Dispose(); _primaryImage = null; _primaryFile = null; }
                if (_defaultImage != null) { _defaultImage.Dispose(); _defaultImage = null; _defaultFile = null; }
                if (_highGravityImage != null) { _highGravityImage.Dispose(); _highGravityImage = null; _highGravityFile = null; }

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

                if (File.Exists(settings.HighGravityImageFilename))
                {
                    _highGravityImage = (Bitmap)Image.FromFile(settings.HighGravityImageFilename);
                    _highGravityFile = Tools.FileToBase64(settings.HighGravityImageFilename, true);
                }
                // Note: _highGravityImage intentionally left null if no file is set;
                // the display logic falls back to the primary/default image in that case.
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "Gravity InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}
