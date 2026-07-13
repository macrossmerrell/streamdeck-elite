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
    [PluginActionId("com.mhwlng.elite.exobiology")]
    public class ExoBiology : EliteKeypadBase
    {
        // ── Colony range lookup ───────────────────────────────────────────────────

        private static readonly Dictionary<string, int> ColonyRangeByGenusSpecies =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Bacterium Aurasus",      500 }, { "Bacterium Cerbrus",      500 },
            { "Bacterium Alcyoneum",    500 }, { "Bacterium Bullaris",     500 },
            { "Bacterium Tela",         500 }, { "Bacterium Vesicula",     500 },
            { "Bacterium Informem",     500 }, { "Bacterium Verrata",      500 },
            { "Bacterium Acies",        500 }, { "Bacterium Scopulum",     500 },
            { "Bacterium Omentum",      500 },
            { "Frutexa Acus",           150 }, { "Frutexa Sponsae",        150 },
            { "Frutexa Metallicum",     150 },
            { "Tussock Ignis",          200 }, { "Tussock Ventusa",        200 },
            { "Tussock Virgam",         200 }, { "Tussock Caputus",        200 },
            { "Tussock Cultro",         200 }, { "Tussock Capillum",       200 },
            { "Tussock Pennata",        200 }, { "Tussock Serrati",        200 },
            { "Tussock Albata",         200 }, { "Tussock Triticum",       200 },
            { "Tubus Compagibus",       800 }, { "Tubus Sororibus",        800 },
            { "Stratum Excutitus",      500 }, { "Stratum Paleas",         500 },
            { "Stratum Tectonicas",     500 },
            { "Concha Labiata",         150 }, { "Concha Renibus",         150 },
            { "Cactoida Vermis",        300 }, { "Cactoida Cortexum",      300 },
            { "Clypeus Lacrimam",       150 }, { "Clypeus Margaritus",     150 },
            { "Osseus Discus",          800 }, { "Osseus Fractus",         800 },
            { "Osseus Pumice",          800 }, { "Osseus Pellebantus",     800 },
            { "Fungoida Stabitis",      300 }, { "Fungoida Setisis",       300 },
            { "Recepta Conditivus",     150 }, { "Recepta Deltahedronix",  150 },
            { "Recepta Umbrux",         150 },
            { "Aleoida Laminiae",       150 }, { "Aleoida Coronamus",      150 },
            { "Aleoida Arcus",          150 }, { "Aleoida Gravis",         150 },
            { "Fonticulua Digitos",     500 }, { "Fonticulua Campestris",  500 },
            { "Fonticulua Lapida",      500 },
        };

        private static readonly Dictionary<string, int> ColonyRangeByGenus =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "Bacterium",  500 }, { "Frutexa",   150 }, { "Tussock",   200 },
            { "Tubus",      800 }, { "Stratum",   500 }, { "Concha",    150 },
            { "Cactoida",   300 }, { "Clypeus",   150 }, { "Osseus",    800 },
            { "Fungoida",   300 }, { "Recepta",   150 }, { "Aleoida",   150 },
            { "Fonticulua", 500 },
        };

        private static int GetColonyRange(string genus, string species)
        {
            if (!string.IsNullOrEmpty(genus) && !string.IsNullOrEmpty(species))
                if (ColonyRangeByGenusSpecies.TryGetValue($"{genus} {species}", out int r)) return r;
            if (!string.IsNullOrEmpty(genus) && ColonyRangeByGenus.TryGetValue(genus, out int gr)) return gr;
            return 0;
        }

        // ── Settings ──────────────────────────────────────────────────────────────

        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings() => new PluginSettings
            {
                DefaultImageFilename = string.Empty,
                ZoneAImageFilename   = string.Empty,
                ZoneBImageFilename   = string.Empty,
                ZoneCImageFilename   = string.Empty,
                ZoneDImageFilename   = string.Empty,

                ZoneAGenusColor            = "#00ffcc",
                ZoneAGenusVerticalPosition = "20",
                ZoneAMeterColor            = "#ffffff",
                ZoneAMeterVerticalPosition = "190",

                ZoneBGenusColor            = "#00ffcc",
                ZoneBGenusVerticalPosition = "20",
                ZoneBMeterColor            = "#ffffff",
                ZoneBMeterVerticalPosition = "190",

                ZoneCGenusColor            = "#00ffcc",
                ZoneCGenusVerticalPosition = "20",
                ZoneCMeterColor            = "#ffffff",
                ZoneCMeterVerticalPosition = "190",

                ZoneDGenusColor            = "#00ffcc",
                ZoneDGenusVerticalPosition = "20",
                ZoneDMeterColor            = "#ffffff",
                ZoneDMeterVerticalPosition = "190",

                PipFilledColor       = "#00ff80",
                PipEmptyColor        = "#505050",
                PipSize              = "12",
                PipVerticalPosition  = "200",

                TextBold = "true",
            };

            [FilenameProperty] [JsonProperty("defaultImage")] public string DefaultImageFilename { get; set; }
            [FilenameProperty] [JsonProperty("zoneAImage")]   public string ZoneAImageFilename   { get; set; }
            [FilenameProperty] [JsonProperty("zoneBImage")]   public string ZoneBImageFilename   { get; set; }
            [FilenameProperty] [JsonProperty("zoneCImage")]   public string ZoneCImageFilename   { get; set; }
            [FilenameProperty] [JsonProperty("zoneDImage")]   public string ZoneDImageFilename   { get; set; }

            [JsonProperty("zoneAGenusColor")]            public string ZoneAGenusColor            { get; set; }
            [JsonProperty("zoneAGenusVerticalPosition")] public string ZoneAGenusVerticalPosition { get; set; }
            [JsonProperty("zoneAMeterColor")]            public string ZoneAMeterColor            { get; set; }
            [JsonProperty("zoneAMeterVerticalPosition")] public string ZoneAMeterVerticalPosition { get; set; }

            [JsonProperty("zoneBGenusColor")]            public string ZoneBGenusColor            { get; set; }
            [JsonProperty("zoneBGenusVerticalPosition")] public string ZoneBGenusVerticalPosition { get; set; }
            [JsonProperty("zoneBMeterColor")]            public string ZoneBMeterColor            { get; set; }
            [JsonProperty("zoneBMeterVerticalPosition")] public string ZoneBMeterVerticalPosition { get; set; }

            [JsonProperty("zoneCGenusColor")]            public string ZoneCGenusColor            { get; set; }
            [JsonProperty("zoneCGenusVerticalPosition")] public string ZoneCGenusVerticalPosition { get; set; }
            [JsonProperty("zoneCMeterColor")]            public string ZoneCMeterColor            { get; set; }
            [JsonProperty("zoneCMeterVerticalPosition")] public string ZoneCMeterVerticalPosition { get; set; }

            [JsonProperty("zoneDGenusColor")]            public string ZoneDGenusColor            { get; set; }
            [JsonProperty("zoneDGenusVerticalPosition")] public string ZoneDGenusVerticalPosition { get; set; }
            [JsonProperty("zoneDMeterColor")]            public string ZoneDMeterColor            { get; set; }
            [JsonProperty("zoneDMeterVerticalPosition")] public string ZoneDMeterVerticalPosition { get; set; }

            [JsonProperty("pipFilledColor")]      public string PipFilledColor      { get; set; }
            [JsonProperty("pipEmptyColor")]       public string PipEmptyColor       { get; set; }
            [JsonProperty("pipSize")]             public string PipSize             { get; set; }
            [JsonProperty("pipVerticalPosition")] public string PipVerticalPosition { get; set; }

            [JsonProperty("textBold")] public string TextBold { get; set; }
        }

        private PluginSettings _settings;

        // Per-zone brushes
        private SolidBrush _zoneAGenusBrush = null;
        private SolidBrush _zoneAMeterBrush = null;
        private SolidBrush _zoneBGenusBrush = null;
        private SolidBrush _zoneBMeterBrush = null;
        private SolidBrush _zoneCGenusBrush = null;
        private SolidBrush _zoneCMeterBrush = null;
        private SolidBrush _zoneDGenusBrush = null;
        private SolidBrush _zoneDMeterBrush = null;

        // Pip brushes (rebuilt in InitializeSettings)
        private SolidBrush _pipFilledBrush = null;
        private SolidBrush _pipEmptyBrush  = null;

        // Bitmaps / base64 per zone
        private Bitmap _defaultBitmap = null;
        private Bitmap _zoneABitmap   = null;
        private Bitmap _zoneBBitmap   = null;
        private Bitmap _zoneCBitmap   = null;
        private Bitmap _zoneDBitmap   = null;

        private string _defaultFile = null;
        private string _zoneAFile   = null;
        private string _zoneBFile   = null;
        private string _zoneCFile   = null;
        private string _zoneDFile   = null;

        // Guard against concurrent HandleDisplay calls — bitmap operations are not thread-safe.
        // Journal events fire on a background thread; OnTick fires on another. Without this,
        // rapid journal activity causes multiple simultaneous draws that block each other.
        private volatile bool _isDrawing = false;

        // ── Distance helpers ──────────────────────────────────────────────────────

        private static double HaversineMetres(double lat1, double lon1,
                                               double lat2, double lon2,
                                               double radiusM)
        {
            const double Deg2Rad = Math.PI / 180.0;
            double dLat = (lat2 - lat1) * Deg2Rad;
            double dLon = (lon2 - lon1) * Deg2Rad;
            double a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                     + Math.Cos(lat1 * Deg2Rad) * Math.Cos(lat2 * Deg2Rad)
                       * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            return radiusM * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        }

        /// <summary>
        /// Resolves planet radius in metres for the Haversine distance calculation.
        ///
        /// Always prefers the live StatusData.PlanetRadius (written by the status file every ~1s)
        /// because it reflects the actual body the player is currently on or near. This matches
        /// what the Windows companion app does — reading PlanetRadius fresh from Status.json on
        /// every update rather than caching a value that could have come from a different body.
        ///
        /// Falls back to GravityCache (from journal Scan events) only when StatusData has no value,
        /// such as when the player is in deep space or the status file hasn't updated yet.
        ///
        /// ExoBioSamplePlanetRadius is only used as a last-resort backfill fallback — it is not
        /// written here so it stays available for that purpose without polluting live calculations.
        /// </summary>
        private static double ResolvePlanetRadius()
        {
            var s = EliteData.StatusData;

            // Always use live StatusData first — same source as the Windows app, refreshed every ~1s
            if (s.PlanetRadius > 0)
                return s.PlanetRadius;

            // GravityCache: keyed by the body name recorded at sample time
            if (!string.IsNullOrEmpty(EliteData.ExoBioSampleBodyName) &&
                EliteData.GravityCache.TryGetValue(EliteData.ExoBioSampleBodyName, out var sampledBody) &&
                sampledBody.PlanetRadius > 0)
                return sampledBody.PlanetRadius;

            // GravityCache: current body from StatusData
            var bodyName = !string.IsNullOrEmpty(s.BodyName) ? s.BodyName : s.DestinationName;
            if (!string.IsNullOrEmpty(bodyName) &&
                EliteData.GravityCache.TryGetValue(bodyName, out var currentBody) &&
                currentBody.PlanetRadius > 0)
                return currentBody.PlanetRadius;

            // Last resort: backfill-resolved value
            if (EliteData.ExoBioSamplePlanetRadius > 0)
                return EliteData.ExoBioSamplePlanetRadius;

            return 0;
        }

        /// <summary>
        /// Returns the raw distance in metres from the player's current position to the
        /// CLOSEST stored scan point (Log position and/or Sample position).
        ///
        /// Both positions are tracked simultaneously so the button catches it if the player
        /// drifts back toward an earlier scan point whose colony range may overlap with the
        /// current one. The zone and meter are always driven by whichever point is nearest.
        ///
        /// Returns NaN when no valid position data is available.
        /// </summary>
        private double DistanceFromLastSampleMetres()
        {
            var s = EliteData.StatusData;
            double radius = ResolvePlanetRadius();

            bool haveCurrentPos = !double.IsNaN(s.Latitude)
                               && !double.IsNaN(s.Longitude)
                               && !(s.Latitude == 0.0 && s.Longitude == 0.0);

            if (!haveCurrentPos || radius <= 0)
                return double.NaN;

            double distLog    = double.NaN;
            double distSample = double.NaN;

            // Distance from scan 1 (Log position) — always available once scan 1 is taken
            if (!double.IsNaN(EliteData.ExoBioLogLat) && !double.IsNaN(EliteData.ExoBioLogLon))
                distLog = HaversineMetres(EliteData.ExoBioLogLat, EliteData.ExoBioLogLon,
                                         s.Latitude, s.Longitude, radius);

            // Distance from scan 2 (Sample position) — only available after scan 2
            if (!double.IsNaN(EliteData.ExoBioSampleLat) && !double.IsNaN(EliteData.ExoBioSampleLon))
                distSample = HaversineMetres(EliteData.ExoBioSampleLat, EliteData.ExoBioSampleLon,
                                             s.Latitude, s.Longitude, radius);

            // Return the smaller of the two — closest scan point wins
            if (double.IsNaN(distLog) && double.IsNaN(distSample)) return double.NaN;
            if (double.IsNaN(distLog))    return distSample;
            if (double.IsNaN(distSample)) return distLog;
            return Math.Min(distLog, distSample);
        }

        // ── Zone resolution ───────────────────────────────────────────────────────

        private enum Zone { Default, A, B, C, D }

        /// <summary>
        /// Zone is computed purely from live distance percentage every tick.
        /// No sticky state — if the player walks back inside the colony range after
        /// reaching Zone D, the zone drops back to C/B/A immediately.
        ///   A   0 – 20 %   too close, keep moving
        ///   B  21 – 70 %   moving away
        ///   C  71 – 99 %   almost far enough
        ///   D  ≥ 100 %     ready to scan
        /// </summary>
        private static Zone CurrentZone(double distPct)
        {
            // ScanCount 0 = no active scan (default). ScanCount 1/2/3 = active sequence.
            // Analyse resets to 0, so >= 3 here means all pips filled, still tracking.
            if (EliteData.ExoBioScanCount == 0) return Zone.Default;
            if (double.IsNaN(distPct))                                            return Zone.A;

            if (distPct >= 100.0) return Zone.D;
            if (distPct >= 71.0)  return Zone.C;
            if (distPct >= 21.0)  return Zone.B;
            return Zone.A;
        }

        // ── Drawing ───────────────────────────────────────────────────────────────

        private void DrawGenusText(Graphics g, string line1, string line2,
                                   SolidBrush brush, double verticalPosition, int width)
        {
            if (string.IsNullOrEmpty(line1) || brush == null) return;

            bool isBold    = _settings.TextBold == "true";
            var  fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;
            var  lines     = string.IsNullOrEmpty(line2)
                                 ? new[] { line1 }
                                 : new[] { line1, line2 };

            for (int sz = 25; sz >= 8; sz--)
            {
                using (var font = new Font("Arial", sz, fontStyle))
                {
                    bool  fits   = true;
                    var   heights = new float[lines.Length];

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (string.IsNullOrEmpty(lines[i])) { heights[i] = font.Height; continue; }
                        var sf = new StringFormat(StringFormat.GenericTypographic);
                        sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, lines[i].Length) });
                        var r = g.MeasureCharacterRanges(lines[i], font, new RectangleF(0, 0, 1000, 1000), sf);
                        var b = r[0].GetBounds(g);
                        heights[i] = b.Height;
                        if (b.Width > width * 0.92f) { fits = false; break; }
                    }

                    if (!fits) continue;

                    float y       = (float)(verticalPosition * (width / 256.0));
                    var   drawFmt = new StringFormat(StringFormat.GenericTypographic);

                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (string.IsNullOrEmpty(lines[i])) { y += heights[i] * 1.15f; continue; }
                        var sf2 = new StringFormat(StringFormat.GenericTypographic);
                        sf2.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, lines[i].Length) });
                        var r2 = g.MeasureCharacterRanges(lines[i], font, new RectangleF(0, 0, 1000, 1000), sf2);
                        var b2 = r2[0].GetBounds(g);
                        float x = (width - b2.Width) / 2f;
                        g.DrawString(lines[i], font, brush, x, y - b2.Y, drawFmt);
                        y += b2.Height * 1.15f;
                    }
                    return;
                }
            }
        }

        private void DrawMeterText(Graphics g, string text, SolidBrush brush,
                                   double verticalPosition, int width)
        {
            if (string.IsNullOrEmpty(text) || brush == null) return;

            bool isBold    = _settings.TextBold == "true";
            var  fontStyle = isBold ? FontStyle.Bold : FontStyle.Regular;

            // Cap height to ~22% of button height so short strings like "8m" don't render enormous.
            // Width is capped at 95% of button width for longer strings like "500m".
            float maxHeight = width * 0.32f;

            for (int sz = 28; sz >= 8; sz -= 1)
            {
                using (var font = new Font("Arial", sz, fontStyle))
                {
                    var measured = g.MeasureString(text, font);
                    if (measured.Width > width * 0.95f) continue;
                    if (measured.Height > maxHeight) continue;
                    float x = (width - measured.Width) / 2f;
                    float y = (float)(verticalPosition * (width / 256.0));
                    g.DrawString(text, font, brush, x, y);
                    return;
                }
            }
        }

        /// <summary>
        /// Draws three scan-progress pip dots using fully configurable color, size and position.
        /// pipDiam is the pixel diameter at the button's native 256px width, scaled proportionally.
        /// </summary>
        private void DrawScanPips(Graphics g, int scansDone, int width)
        {
            if (_pipFilledBrush == null || _pipEmptyBrush == null) return;

            int pipDiam = int.TryParse(_settings.PipSize, out int ps) ? ps : 12;
            // Scale to actual button width (buttons are typically 72 or 144px on device)
            pipDiam = Math.Max(2, (int)Math.Round(pipDiam * (width / 256.0)));

            int spacing = (int)Math.Round(pipDiam * 1.8);   // gap between pip centres
            const int PipCount = 3;
            int totalW  = (PipCount - 1) * spacing + pipDiam;
            int startX  = (width - totalW) / 2;

            double vPos = double.TryParse(_settings.PipVerticalPosition, out double pv) ? pv : 200.0;
            int    pipY = (int)Math.Round(vPos * (width / 256.0));

            for (int i = 0; i < PipCount; i++)
            {
                int cx      = startX + i * spacing;
                bool filled = i < scansDone;

                g.FillEllipse(filled ? _pipFilledBrush : _pipEmptyBrush, cx, pipY, pipDiam, pipDiam);

                // Thin outline in the opposite brush colour for legibility
                using (var pen = new Pen(filled ? _pipEmptyBrush.Color : _pipFilledBrush.Color, 1))
                    g.DrawEllipse(pen, cx, pipY, pipDiam, pipDiam);
            }
        }

        // ── Main display update ───────────────────────────────────────────────────

        private async Task HandleDisplay()
        {
            // Skip if a draw is already in progress — the in-flight draw will show current state.
            if (_isDrawing) return;
            _isDrawing = true;
            try
            {

            int    colonyRange = GetColonyRange(EliteData.ExoBioGenus, EliteData.ExoBioSpecies);
            double distMetres  = DistanceFromLastSampleMetres();
            double distPct     = (colonyRange > 0 && !double.IsNaN(distMetres))
                                     ? distMetres / colonyRange * 100.0
                                     : double.NaN;

            Zone zone = CurrentZone(distPct);

            Bitmap     background;
            string     backgroundFile;
            SolidBrush genusBrush;
            double     genusVPos;
            SolidBrush meterBrush;
            double     meterVPos;

            switch (zone)
            {
                case Zone.A:
                    background     = _zoneABitmap ?? _defaultBitmap;
                    backgroundFile = _zoneAFile   ?? _defaultFile;
                    genusBrush     = _zoneAGenusBrush;
                    genusVPos      = ParsePos(_settings.ZoneAGenusVerticalPosition, 20.0);
                    meterBrush     = _zoneAMeterBrush;
                    meterVPos      = ParsePos(_settings.ZoneAMeterVerticalPosition, 190.0);
                    break;
                case Zone.B:
                    background     = _zoneBBitmap ?? _zoneABitmap ?? _defaultBitmap;
                    backgroundFile = _zoneBFile   ?? _zoneAFile   ?? _defaultFile;
                    genusBrush     = _zoneBGenusBrush;
                    genusVPos      = ParsePos(_settings.ZoneBGenusVerticalPosition, 20.0);
                    meterBrush     = _zoneBMeterBrush;
                    meterVPos      = ParsePos(_settings.ZoneBMeterVerticalPosition, 190.0);
                    break;
                case Zone.C:
                    background     = _zoneCBitmap ?? _zoneBBitmap ?? _defaultBitmap;
                    backgroundFile = _zoneCFile   ?? _zoneBFile   ?? _defaultFile;
                    genusBrush     = _zoneCGenusBrush;
                    genusVPos      = ParsePos(_settings.ZoneCGenusVerticalPosition, 20.0);
                    meterBrush     = _zoneCMeterBrush;
                    meterVPos      = ParsePos(_settings.ZoneCMeterVerticalPosition, 190.0);
                    break;
                case Zone.D:
                    background     = _zoneDBitmap ?? _defaultBitmap;
                    backgroundFile = _zoneDFile   ?? _defaultFile;
                    genusBrush     = _zoneDGenusBrush;
                    genusVPos      = ParsePos(_settings.ZoneDGenusVerticalPosition, 20.0);
                    meterBrush     = _zoneDMeterBrush;
                    meterVPos      = ParsePos(_settings.ZoneDMeterVerticalPosition, 190.0);
                    break;
                default: // Zone.Default — no active scan
                    if (!string.IsNullOrEmpty(_defaultFile))
                        await Connection.SetImageAsync(_defaultFile);
                    return;
            }

            if (background == null)
            {
                if (!string.IsNullOrEmpty(backgroundFile))
                    await Connection.SetImageAsync(backgroundFile);
                return;
            }

            string genusLine1 = EliteData.ExoBioGenus?.ToUpper();
            string genusLine2 = !string.IsNullOrEmpty(EliteData.ExoBioSpecies)
                                    ? EliteData.ExoBioSpecies.ToUpper()
                                    : null;

            string meterText = null;
            if (zone != Zone.D && !double.IsNaN(distMetres))
            {
                // Distance from the closest scan point.
                // Shows 0m at the scan point and counts up as the player moves away.
                // Counts back down if the player moves toward either stored scan point.
                // Zone D means colony range reached — meter hidden.
                meterText = $"{distMetres:F0}m";
            }

            string imgBase64 = null;
            try
            {
                using (var bitmap = new Bitmap(background))
                {
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        int width = bitmap.Width;

                        DrawGenusText(graphics, genusLine1, genusLine2, genusBrush, genusVPos, width);

                        if (meterBrush != null)
                            DrawMeterText(graphics, meterText, meterBrush, meterVPos, width);

                        DrawScanPips(graphics, EliteData.ExoBioScanCount, width);
                    }

                    imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "ExoBiology HandleDisplay " + ex);
            }

            if (!string.IsNullOrEmpty(imgBase64))
                await Connection.SetImageAsync(imgBase64);

            } // end try
            finally
            {
                _isDrawing = false;
            }
        }

        private static double ParsePos(string value, double fallback) =>
            double.TryParse(value, out double v) ? v : fallback;

        // ── Journal events ────────────────────────────────────────────────────────

        public void HandleEliteEvents(object sender, MessageReceivedEventArgs args)
        {
            var e = args.EventArgs;
                        var raw = ((JournalEventArgs)e).OriginalEvent;
            var evt = raw?.Value<string>("event");

            if (evt == "ScanOrganic")
            {
                var scanType     = raw.Value<string>("ScanType") ?? "";
                var genusLocal   = raw.Value<string>("Genus_Localised")   ?? raw.Value<string>("Genus")   ?? "";
                var speciesLocal = raw.Value<string>("Species_Localised") ?? raw.Value<string>("Species") ?? "";

                if (genusLocal.StartsWith("$"))   genusLocal   = string.Empty;
                if (speciesLocal.StartsWith("$")) speciesLocal = string.Empty;

                string speciesWord = null;
                if (!string.IsNullOrEmpty(speciesLocal) && !string.IsNullOrEmpty(genusLocal))
                {
                    speciesWord = speciesLocal
                        .Replace(genusLocal, "", StringComparison.OrdinalIgnoreCase)
                        .Trim();
                    if (string.IsNullOrEmpty(speciesWord)) speciesWord = null;
                }

                var s = EliteData.StatusData;

                if (scanType == "Log")
                {
                    // New genus or abandon+restart — full reset then store scan 1 position
                    EliteData.ExoBioGenus              = string.IsNullOrEmpty(genusLocal) ? EliteData.ExoBioGenus : genusLocal;
                    EliteData.ExoBioSpecies            = speciesWord;
                    EliteData.ExoBioScanCount          = 1;
                    EliteData.ExoBioLogLat             = raw.Value<double?>("Latitude")  ?? s.Latitude;
                    EliteData.ExoBioLogLon             = raw.Value<double?>("Longitude") ?? s.Longitude;
                    // Clear scan 2 position — fresh sequence
                    EliteData.ExoBioSampleLat          = double.NaN;
                    EliteData.ExoBioSampleLon          = double.NaN;
                    EliteData.ExoBioSampleBodyName     = !string.IsNullOrEmpty(s.BodyName) ? s.BodyName : s.DestinationName;
                    EliteData.ExoBioSamplePlanetRadius = ResolvePlanetRadius();

                    Logger.Instance.LogMessage(TracingLevel.INFO,
                        $"ExoBiology Log: genus={EliteData.ExoBioGenus} species={EliteData.ExoBioSpecies} " +
                        $"logLat={EliteData.ExoBioLogLat:F4} logLon={EliteData.ExoBioLogLon:F4} " +
                        $"body={EliteData.ExoBioSampleBodyName} radius={EliteData.ExoBioSamplePlanetRadius:F0}m " +
                        $"colonyRange={GetColonyRange(EliteData.ExoBioGenus, EliteData.ExoBioSpecies)}m");
                }
                else if (scanType == "Sample")
                {
                    if (!string.IsNullOrEmpty(genusLocal))  EliteData.ExoBioGenus   = genusLocal;
                    if (!string.IsNullOrEmpty(speciesWord)) EliteData.ExoBioSpecies = speciesWord;

                    // Increment scan count: first Sample → 2 (pip 2), second Sample → 3 (pip 3)
                    EliteData.ExoBioScanCount          = Math.Min(EliteData.ExoBioScanCount + 1, 3);
                    // Store sample position — Log position is preserved so both are tracked
                    EliteData.ExoBioSampleLat          = raw.Value<double?>("Latitude")  ?? s.Latitude;
                    EliteData.ExoBioSampleLon          = raw.Value<double?>("Longitude") ?? s.Longitude;
                    EliteData.ExoBioSampleBodyName     = !string.IsNullOrEmpty(s.BodyName) ? s.BodyName : s.DestinationName;
                    EliteData.ExoBioSamplePlanetRadius = ResolvePlanetRadius();

                    Logger.Instance.LogMessage(TracingLevel.INFO,
                        $"ExoBiology Sample (scan {EliteData.ExoBioScanCount}): genus={EliteData.ExoBioGenus} species={EliteData.ExoBioSpecies} " +
                        $"sampleLat={EliteData.ExoBioSampleLat:F4} sampleLon={EliteData.ExoBioSampleLon:F4} " +
                        $"logLat={EliteData.ExoBioLogLat:F4} logLon={EliteData.ExoBioLogLon:F4} " +
                        $"body={EliteData.ExoBioSampleBodyName}");
                }
                else if (scanType == "Analyse")
                {
                    // All three pips already filled by the second Sample event.
                    // Analyse completes the sequence — reset to default state.
                    Logger.Instance.LogMessage(TracingLevel.INFO,
                        $"ExoBiology Analyse complete: genus={EliteData.ExoBioGenus} species={EliteData.ExoBioSpecies}");
                    EliteData.ResetExoBioState();
                }
            }
            else if (evt == "SellOrganicData")
            {
                // Only a deliberate data sale clears the scan sequence.
                // Liftoff, LeaveBody, boarding the ship, entering SRV etc. do NOT reset —
                // the player needs genus and meter visible while repositioning between samples.
                EliteData.ResetExoBioState();
            }
            else
            {
                // Not an event this button cares about — skip the redraw entirely.
                // OnTick fires every second and will keep the distance meter current.
                return;
            }

            AsyncHelper.RunSync(HandleDisplay);
        }

        // ── Constructor / lifecycle ───────────────────────────────────────────────

        public ExoBiology(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

            Program.JournalWatcher.MessageReceived += HandleEliteEvents;
        }

        public override void KeyPressed(KeyPayload payload)  { }
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
            BarRaider.SdTools.Tools.AutoPopulateSettings(_settings, payload.Settings);
            InitializeSettings();
            AsyncHelper.RunSync(HandleDisplay);
        }

        private void InitializeSettings()
        {
            // Zone A defaults
            if (string.IsNullOrEmpty(_settings.ZoneAGenusColor))            _settings.ZoneAGenusColor            = "#00ffcc";
            if (string.IsNullOrEmpty(_settings.ZoneAGenusVerticalPosition)) _settings.ZoneAGenusVerticalPosition = "20";
            if (string.IsNullOrEmpty(_settings.ZoneAMeterColor))            _settings.ZoneAMeterColor            = "#ffffff";
            if (string.IsNullOrEmpty(_settings.ZoneAMeterVerticalPosition)) _settings.ZoneAMeterVerticalPosition = "190";
            // Zone B defaults
            if (string.IsNullOrEmpty(_settings.ZoneBGenusColor))            _settings.ZoneBGenusColor            = "#00ffcc";
            if (string.IsNullOrEmpty(_settings.ZoneBGenusVerticalPosition)) _settings.ZoneBGenusVerticalPosition = "20";
            if (string.IsNullOrEmpty(_settings.ZoneBMeterColor))            _settings.ZoneBMeterColor            = "#ffffff";
            if (string.IsNullOrEmpty(_settings.ZoneBMeterVerticalPosition)) _settings.ZoneBMeterVerticalPosition = "190";
            // Zone C defaults
            if (string.IsNullOrEmpty(_settings.ZoneCGenusColor))            _settings.ZoneCGenusColor            = "#00ffcc";
            if (string.IsNullOrEmpty(_settings.ZoneCGenusVerticalPosition)) _settings.ZoneCGenusVerticalPosition = "20";
            if (string.IsNullOrEmpty(_settings.ZoneCMeterColor))            _settings.ZoneCMeterColor            = "#ffffff";
            if (string.IsNullOrEmpty(_settings.ZoneCMeterVerticalPosition)) _settings.ZoneCMeterVerticalPosition = "190";
            // Zone D defaults
            if (string.IsNullOrEmpty(_settings.ZoneDGenusColor))            _settings.ZoneDGenusColor            = "#00ffcc";
            if (string.IsNullOrEmpty(_settings.ZoneDGenusVerticalPosition)) _settings.ZoneDGenusVerticalPosition = "20";
            if (string.IsNullOrEmpty(_settings.ZoneDMeterColor))            _settings.ZoneDMeterColor            = "#ffffff";
            if (string.IsNullOrEmpty(_settings.ZoneDMeterVerticalPosition)) _settings.ZoneDMeterVerticalPosition = "190";
            // Pip defaults
            if (string.IsNullOrEmpty(_settings.PipFilledColor))             _settings.PipFilledColor             = "#00ff80";
            if (string.IsNullOrEmpty(_settings.PipEmptyColor))              _settings.PipEmptyColor              = "#505050";
            if (string.IsNullOrEmpty(_settings.PipSize))                    _settings.PipSize                    = "12";
            if (string.IsNullOrEmpty(_settings.PipVerticalPosition))        _settings.PipVerticalPosition        = "200";
            // Global
            if (string.IsNullOrEmpty(_settings.TextBold))                   _settings.TextBold                   = "true";

            try
            {
                var conv = new System.Drawing.ColorConverter();

                SolidBrush MakeBrush(string hex) =>
                    new SolidBrush((Color)conv.ConvertFromString(hex));

                _zoneAGenusBrush?.Dispose(); _zoneAGenusBrush = MakeBrush(_settings.ZoneAGenusColor);
                _zoneAMeterBrush?.Dispose(); _zoneAMeterBrush = MakeBrush(_settings.ZoneAMeterColor);
                _zoneBGenusBrush?.Dispose(); _zoneBGenusBrush = MakeBrush(_settings.ZoneBGenusColor);
                _zoneBMeterBrush?.Dispose(); _zoneBMeterBrush = MakeBrush(_settings.ZoneBMeterColor);
                _zoneCGenusBrush?.Dispose(); _zoneCGenusBrush = MakeBrush(_settings.ZoneCGenusColor);
                _zoneCMeterBrush?.Dispose(); _zoneCMeterBrush = MakeBrush(_settings.ZoneCMeterColor);
                _zoneDGenusBrush?.Dispose(); _zoneDGenusBrush = MakeBrush(_settings.ZoneDGenusColor);
                _zoneDMeterBrush?.Dispose(); _zoneDMeterBrush = MakeBrush(_settings.ZoneDMeterColor);
                _pipFilledBrush?.Dispose();  _pipFilledBrush  = MakeBrush(_settings.PipFilledColor);
                _pipEmptyBrush?.Dispose();   _pipEmptyBrush   = MakeBrush(_settings.PipEmptyColor);

                void Load(string filename, ref Bitmap bmp, ref string b64)
                {
                    bmp?.Dispose(); bmp = null; b64 = null;
                    if (File.Exists(filename))
                    {
                        bmp = (Bitmap)Image.FromFile(filename);
                        b64 = Tools.FileToBase64(filename, true);
                    }
                }

                Load(_settings.DefaultImageFilename, ref _defaultBitmap, ref _defaultFile);
                Load(_settings.ZoneAImageFilename,   ref _zoneABitmap,   ref _zoneAFile);
                Load(_settings.ZoneBImageFilename,   ref _zoneBBitmap,   ref _zoneBFile);
                Load(_settings.ZoneCImageFilename,   ref _zoneCBitmap,   ref _zoneCFile);
                Load(_settings.ZoneDImageFilename,   ref _zoneDBitmap,   ref _zoneDFile);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "ExoBiology InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(_settings)).Wait();
        }
    }
}
