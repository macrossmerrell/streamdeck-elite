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
                    PrimaryColor = "#ffffff",
                    TextVerticalPosition = "28",
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

            [JsonProperty(PropertyName = "primaryColor")]
            public string PrimaryColor { get; set; }

            [JsonProperty(PropertyName = "textVerticalPosition")]
            public string TextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "textBold")]
            public string TextBold { get; set; }
        }

        private PluginSettings settings;
        private Bitmap _primaryImage = null;
        private Bitmap _defaultImage = null;
        private string _primaryFile;
        private string _defaultFile;
        private SolidBrush _primaryBrush = new SolidBrush(Color.White);

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

            if (s.HasLatLong)
            {
                // Near planet - use primary image and calculate real-time gravity
                myBitmap = _primaryImage;
                imgBase64 = _primaryFile;

                if (!string.IsNullOrEmpty(s.BodyName) &&
                    EliteData.GravityCache.TryGetValue(s.BodyName, out var cached))
                {
                    var currentGravity = CalculateGravityAtAltitude(cached.SurfaceGravity, cached.PlanetRadius, s.Altitude);
                    gravityText = $"{currentGravity:F2}g";
                }
                else
                {
                    gravityText = "?g";
                }
            }
            else if (!string.IsNullOrEmpty(s.BodyName) &&
                     EliteData.GravityCache.TryGetValue(s.BodyName, out var cachedBody))
            {
                // Not near planet but have cached gravity for current body - show on default image
                myBitmap = _defaultImage;
                imgBase64 = _defaultFile;
                gravityText = $"{cachedBody.SurfaceGravity:F2}g";
            }
            else
            {
                // No gravity data - show default image only
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

                                graphics.DrawString(gravityText, testFont, _primaryBrush, (float)x, (float)y);
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
            if (string.IsNullOrEmpty(settings.PrimaryColor))
                settings.PrimaryColor = "#ffffff";

            if (string.IsNullOrEmpty(settings.TextVerticalPosition))
                settings.TextVerticalPosition = "28";

            if (string.IsNullOrEmpty(settings.TextBold))
                settings.TextBold = "true";

            try
            {
                var converter = new ColorConverter();
                _primaryBrush = new SolidBrush((Color)converter.ConvertFromString(settings.PrimaryColor));

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
                Logger.Instance.LogMessage(TracingLevel.FATAL, "Gravity InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}
