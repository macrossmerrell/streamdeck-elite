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
    [PluginActionId("com.mhwlng.elite.navtarget")]
    public class NavTarget : EliteKeypadBase
    {
        // ── Planet type → rotating display phrases ─────────────────────────────
        // Single-element arrays display static text.
        // Multi-element arrays cycle every 2 seconds via OnTick.
        // \n within a phrase triggers auto-scaled multi-line rendering.
        private static readonly Dictionary<string, string[]> PlanetTypeLines =
            new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            { "Metal rich body",                    new[] { "METAL\nRICH" } },
            { "High metal content body",            new[] { "HIGH\nMETAL" } },
            { "Rocky body",                         new[] { "ROCKY" } },
            { "Icy body",                           new[] { "ICY" } },
            { "Rocky ice body",                     new[] { "ROCKY\nICE" } },
            { "Earthlike body",                     new[] { "EARTH\nLIKE" } },
            { "Water world",                        new[] { "WATER\nWORLD" } },
            { "Ammonia world",                      new[] { "AMMONIA\nWORLD" } },
            { "Water giant",                        new[] { "WATER\nGIANT" } },
            { "Water giant with life",              new[] { "WATER\nGIANT", "WITH\nLIFE" } },
            { "Gas giant with water based life",    new[] { "GAS\nGIANT", "WATER\nBASED LIFE" } },
            { "Gas giant with ammonia based life",  new[] { "GAS\nGIANT", "AMMONIA\nBASED LIFE" } },
            { "Sudarsky class I gas giant",         new[] { "GAS\nGIANT", "CLASS\nI" } },
            { "Sudarsky class II gas giant",        new[] { "GAS\nGIANT", "CLASS\nII" } },
            { "Sudarsky class III gas giant",       new[] { "GAS\nGIANT", "CLASS\nIII" } },
            { "Sudarsky class IV gas giant",        new[] { "GAS\nGIANT", "CLASS\nIV" } },
            { "Sudarsky class V gas giant",         new[] { "GAS\nGIANT", "CLASS\nV" } },
            { "Helium rich gas giant",              new[] { "GAS\nGIANT", "HELIUM\nRICH" } },
            { "Helium gas giant",                   new[] { "GAS\nGIANT", "HELIUM" } },
        };

        // ── Rotation state ─────────────────────────────────────────────────────
        private int    _planetRotationIndex = 0;
        private int    _signalRotationIndex = 0;
        private int    _tickCount           = 0;
        private string _lastDestinationName = null;

        // ── Settings ───────────────────────────────────────────────────────────
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                return new PluginSettings
                {
                    ActiveImageFilename        = string.Empty,
                    DefaultImageFilename       = string.Empty,
                    PlanetTypeColor            = "#00cfff",
                    SignalColor                = "#00ff88",
                    PlanetTypeVerticalPosition = "34",
                    SignalVerticalPosition     = "148",
                    TextBold                   = "true"
                };
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "activeImage")]
            public string ActiveImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "defaultImage")]
            public string DefaultImageFilename { get; set; }

            [JsonProperty(PropertyName = "planetTypeColor")]
            public string PlanetTypeColor { get; set; }

            [JsonProperty(PropertyName = "signalColor")]
            public string SignalColor { get; set; }

            [JsonProperty(PropertyName = "planetTypeVerticalPosition")]
            public string PlanetTypeVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "signalVerticalPosition")]
            public string SignalVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "textBold")]
            public string TextBold { get; set; }
        }

        private PluginSettings _settings;
        private Bitmap     _activeImage  = null;
        private Bitmap     _defaultImage = null;
        private string     _activeFile;
        private string     _defaultFile;
        private SolidBrush _planetBrush = new SolidBrush(Color.FromArgb(0, 207, 255));
        private SolidBrush _signalBrush = new SolidBrush(Color.FromArgb(0, 255, 136));

        // ── Text rendering ─────────────────────────────────────────────────────

        /// <summary>
        /// Renders multi-line text (lines separated by \n) with auto-scaled font,
        /// centred horizontally, starting at verticalPosition.
        /// Uses MeasureCharacterRanges for pixel-accurate width (same as PlanetInfo).
        /// Steps down from maxFontSize until every line fits within 90% of button width.
        /// </summary>
        private void DrawAutoScaledText(Graphics g, string text, SolidBrush brush,
                                        double verticalPosition, int width,
                                        int maxFontSize = 25)
        {
            if (string.IsNullOrEmpty(text)) return;

            var isBold    = _settings.TextBold == "true";
            var fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;
            var lines     = text.Replace("\r\n", "\n").Replace("\\n", "\n").Split('\n');

            for (int sz = maxFontSize; sz >= 8; sz--)
            {
                using (var font = new Font("Arial", sz, fontStyle))
                {
                    bool fits       = true;
                    var lineHeights = new float[lines.Length];

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        if (string.IsNullOrEmpty(line))
                        {
                            lineHeights[i] = font.Height;
                            continue;
                        }

                        var sf = new StringFormat(StringFormat.GenericTypographic);
                        sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, line.Length) });
                        var regions = g.MeasureCharacterRanges(
                            line, font, new RectangleF(0, 0, 1000, 1000), sf);
                        var bounds  = regions[0].GetBounds(g);
                        lineHeights[i] = bounds.Height;

                        if (bounds.Width > width * 0.90f)
                        {
                            fits = false;
                            break;
                        }
                    }

                    if (!fits) continue;

                    // All lines fit at this size — draw them
                    var drawFmt  = new StringFormat(StringFormat.GenericTypographic);
                    float currentY = (float)(verticalPosition * (width / 256.0));

                    for (int i = 0; i < lines.Length; i++)
                    {
                        var line = lines[i];
                        if (string.IsNullOrEmpty(line))
                        {
                            currentY += lineHeights[i] * 1.1f;
                            continue;
                        }

                        var sf2 = new StringFormat(StringFormat.GenericTypographic);
                        sf2.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, line.Length) });
                        var regions2 = g.MeasureCharacterRanges(
                            line, font, new RectangleF(0, 0, 1000, 1000), sf2);
                        var b = regions2[0].GetBounds(g);
                        var x = (width - b.Width) / 2.0f;
                        // Subtract b.Y to correct GDI+ internal leading offset
                        g.DrawString(line, font, brush, x, currentY - b.Y, drawFmt);
                        currentY += b.Height * 1.1f;
                    }
                    return;
                }
            }
        }

        // ── Display ────────────────────────────────────────────────────────────

        private async Task HandleDisplay()
        {
            var s        = EliteData.StatusData;
            var destName = s?.DestinationName;

            // Reset rotation indices whenever the navigation target changes
            if (destName != _lastDestinationName)
            {
                _planetRotationIndex = 0;
                _signalRotationIndex = 0;
                _lastDestinationName = destName;
            }

            // No nav target → show inactive image
            if (string.IsNullOrEmpty(destName))
            {
                if (!string.IsNullOrEmpty(_defaultFile))
                    await Connection.SetImageAsync(_defaultFile);
                return;
            }

            // Look up cached data from EliteData (populated by live events + backfill)
            EliteData.GravityCache.TryGetValue(destName, out var bodyGravity);
            EliteData.SignalCache.TryGetValue(destName, out var bodySignals);
            Logger.Instance.LogMessage(TracingLevel.INFO, $"NavTarget lookup: '{destName}' planetClass='{bodyGravity.PlanetClass}' bio={bodySignals.BiologyCount} geo={bodySignals.GeologyCount}");

            // Determine which planet-type phrase array applies
            string[] planetLines = null;
            if (!string.IsNullOrEmpty(bodyGravity.PlanetClass))
                PlanetTypeLines.TryGetValue(bodyGravity.PlanetClass, out planetLines);

            // Use active image when we have recognisable planet data, default otherwise
            var myBitmap  = (planetLines != null) ? _activeImage  : _defaultImage;
            var imgBase64 = (planetLines != null) ? _activeFile   : _defaultFile;

            if (myBitmap == null)
            {
                if (!string.IsNullOrEmpty(imgBase64))
                    await Connection.SetImageAsync(imgBase64);
                return;
            }

            // Current rotating planet-type phrase
            string planetText = null;
            if (planetLines != null && planetLines.Length > 0)
            {
                _planetRotationIndex = _planetRotationIndex % planetLines.Length;
                planetText = planetLines[_planetRotationIndex];
            }

            // Current bio/geo/terraform signal label
            string signalText = null;
            bool hasBio = bodySignals.BiologyCount > 0;
            bool hasGeo = bodySignals.GeologyCount > 0;
            bool hasTerraform = !string.IsNullOrEmpty(bodyGravity.TerraformState) &&
                                bodyGravity.TerraformState.IndexOf("Terraformable", StringComparison.OrdinalIgnoreCase) >= 0;

            var signalLabels = new List<string>();
            if (hasBio) signalLabels.Add("BIOLOGY");
            if (hasGeo) signalLabels.Add("GEOLOGY");
            if (hasTerraform) signalLabels.Add("TERRAFORM");

            if (signalLabels.Count > 1)
            {
                _signalRotationIndex = _signalRotationIndex % signalLabels.Count;
                signalText = signalLabels[_signalRotationIndex];
            }
            else if (signalLabels.Count == 1)
            {
                signalText = signalLabels[0];
            }

            // Render text onto a copy of the image
            try
            {
                using (var bitmap = new Bitmap(myBitmap))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var width   = bitmap.Width;
                        var typePos = double.TryParse(_settings.PlanetTypeVerticalPosition,
                                          out double tp) ? tp : 34.0;
                        var sigPos  = double.TryParse(_settings.SignalVerticalPosition,
                                          out double sp) ? sp : 148.0;

                        if (planetText != null)
                            DrawAutoScaledText(graphics, planetText, _planetBrush, typePos, width);

                        if (signalText != null)
                            DrawAutoScaledText(graphics, signalText, _signalBrush, sigPos, width);
                    }

                    imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "NavTarget HandleDisplay " + ex);
            }

            await Connection.SetImageAsync(imgBase64);
        }

        // ── Journal event handler ──────────────────────────────────────────────
        // EliteData.HandleEliteEvents already caches Scan / FSSBodySignals /
        // SAASignalsFound into GravityCache and SignalCache, so we just redraw.

        public void HandleEliteEvents(object sender, JournalEventArgs e)
        {
            System.Threading.Thread.Sleep(100);
            AsyncHelper.RunSync(HandleDisplay);
        }

        // ── Lifecycle ──────────────────────────────────────────────────────────

        public NavTarget(SDConnection connection, InitialPayload payload) : base(connection, payload)
        {
            if (payload.Settings == null || payload.Settings.Count == 0)
            {
                _settings = PluginSettings.CreateDefaultSettings();
                Connection.SetSettingsAsync(JObject.FromObject(_settings)).Wait();
            }
            else
            {
                _settings = payload.Settings.ToObject<PluginSettings>();
                InitializeSettings();
                AsyncHelper.RunSync(HandleDisplay);
            }

            Program.JournalWatcher.AllEventHandler += HandleEliteEvents;
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

            // OnTick fires ~every 1 second.
            // Advance rotation indices every 2 ticks (≈2 s).
            _tickCount++;
            if (_tickCount >= 2)
            {
                _tickCount = 0;

                if (!string.IsNullOrEmpty(_lastDestinationName))
                {
                    EliteData.GravityCache.TryGetValue(_lastDestinationName, out var bd);
                    EliteData.SignalCache.TryGetValue(_lastDestinationName, out var bs);

                    // Advance planet-type phrase only when there is more than one phrase
                    if (!string.IsNullOrEmpty(bd.PlanetClass) &&
                        PlanetTypeLines.TryGetValue(bd.PlanetClass, out var lines) &&
                        lines.Length > 1)
                    {
                        _planetRotationIndex = (_planetRotationIndex + 1) % lines.Length;
                    }

                    // Advance signal label when more than one label is present
                    bool obBio = bs.BiologyCount > 0;
                    bool obGeo = bs.GeologyCount > 0;
                    bool obTerraform = !string.IsNullOrEmpty(bd.TerraformState) &&
                                      bd.TerraformState.IndexOf("Terraformable", StringComparison.OrdinalIgnoreCase) >= 0;
                    int labelCount = (obBio ? 1 : 0) + (obGeo ? 1 : 0) + (obTerraform ? 1 : 0);
                    if (labelCount > 1)
                        _signalRotationIndex = (_signalRotationIndex + 1) % labelCount;
                }
            }

            await HandleDisplay();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            BarRaider.SdTools.Tools.AutoPopulateSettings(_settings, payload.Settings);
            InitializeSettings();
            AsyncHelper.RunSync(HandleDisplay);
        }

        private void InitializeSettings()
        {
            if (string.IsNullOrEmpty(_settings.PlanetTypeColor))
                _settings.PlanetTypeColor = "#00cfff";
            if (string.IsNullOrEmpty(_settings.SignalColor))
                _settings.SignalColor = "#00ff88";
            if (string.IsNullOrEmpty(_settings.PlanetTypeVerticalPosition))
                _settings.PlanetTypeVerticalPosition = "34";
            if (string.IsNullOrEmpty(_settings.SignalVerticalPosition))
                _settings.SignalVerticalPosition = "148";
            if (string.IsNullOrEmpty(_settings.TextBold))
                _settings.TextBold = "true";

            try
            {
                var converter = new ColorConverter();
                _planetBrush = new SolidBrush((Color)converter.ConvertFromString(_settings.PlanetTypeColor));
                _signalBrush = new SolidBrush((Color)converter.ConvertFromString(_settings.SignalColor));

                if (_activeImage  != null) { _activeImage.Dispose();  _activeImage  = null; _activeFile  = null; }
                if (_defaultImage != null) { _defaultImage.Dispose(); _defaultImage = null; _defaultFile = null; }

                if (File.Exists(_settings.ActiveImageFilename))
                {
                    _activeImage = (Bitmap)Image.FromFile(_settings.ActiveImageFilename);
                    _activeFile  = Tools.FileToBase64(_settings.ActiveImageFilename, true);
                }

                if (File.Exists(_settings.DefaultImageFilename))
                {
                    _defaultImage = (Bitmap)Image.FromFile(_settings.DefaultImageFilename);
                    _defaultFile  = Tools.FileToBase64(_settings.DefaultImageFilename, true);
                }
                else
                {
                    // No separate inactive image provided — fall back to active image
                    _defaultImage = _activeImage;
                    _defaultFile  = _activeFile;
                }

                if (_activeImage == null)
                {
                    _activeImage = _defaultImage;
                    _activeFile  = _defaultFile;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "NavTarget InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(_settings)).Wait();
        }
    }
}
