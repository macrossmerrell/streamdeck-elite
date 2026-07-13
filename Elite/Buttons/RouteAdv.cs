using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BarRaider.SdTools;
using EliteJournalReader;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable StringLiteralTypo

namespace Elite.Buttons
{

    [PluginActionId("com.mhwlng.elite.routeadv")]
    public class RouteAdv : EliteKeypadBase
    {
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    NoRouteImageFilename = string.Empty,

                    AutoRotateEnabled = "false",
                    AutoRotateSeconds = "5",

                    JumpsEnabled = "true",
                    JumpsImageFilename = string.Empty,
                    JumpsColor = "#ffffff",
                    JumpsLabelColor = "#ffffff",
                    JumpsTextVerticalPosition = "160",
                    JumpsTextBold = "true",
                    JumpsLabelText = "Jumps Remaining",
                    JumpsLabelTextVerticalPosition = "5",

                    RemainingEnabled = "true",
                    RemainingImageFilename = string.Empty,
                    RemainingColor = "#ffffff",
                    RemainingLabelColor = "#ffffff",
                    RemainingTextVerticalPosition = "160",
                    RemainingTextBold = "true",
                    RemainingLabelText = "Destination",
                    RemainingLabelTextVerticalPosition = "5",

                    NextEnabled = "false",
                    NextImageFilename = string.Empty,
                    NextColor = "#ffffff",
                    NextLabelColor = "#ffffff",
                    NextTextVerticalPosition = "160",
                    NextTextBold = "true",
                    NextLabelText = "Next System",
                    NextLabelTextVerticalPosition = "5",

                    ProgressEnabled = "false",
                    ProgressImageFilename = string.Empty,
                    ProgressColor = "#ffffff",
                    ProgressLabelColor = "#ffffff",
                    ProgressTextVerticalPosition = "160",
                    ProgressTextBold = "true",
                    ProgressLabelText = "Trip Progress",
                    ProgressLabelTextVerticalPosition = "5",

                    FuelEnabled = "false",
                    FuelImageFilename = string.Empty,
                    FuelColor = "#ffffff",
                    FuelLabelColor = "#ffffff",
                    FuelTextVerticalPosition = "160",
                    FuelTextBold = "true",
                    FuelLabelText = "Fuel Status",
                    FuelLabelTextVerticalPosition = "5",

                    JumpRangeEnabled = "false",
                    JumpRangeImageFilename = string.Empty,
                    JumpRangeColor = "#ffffff",
                    JumpRangeLabelColor = "#ffffff",
                    JumpRangeTextVerticalPosition = "160",
                    JumpRangeTextBold = "true",
                    JumpRangeLabelText = "Jump Range",
                    JumpRangeLabelTextVerticalPosition = "5",

                    ClickSoundFilename = string.Empty,
                    ErrorSoundFilename = string.Empty
                };

                return instance;
            }

            // Shown when there is no active route (RemainingJumpsInRoute == 0)
            [FilenameProperty]
            [JsonProperty(PropertyName = "noRouteImage")]
            public string NoRouteImageFilename { get; set; }

            // Cycling behaviour: button press always advances to the next enabled option.
            // AutoRotateEnabled additionally advances automatically every AutoRotateSeconds.
            [JsonProperty(PropertyName = "autoRotateEnabled")]
            public string AutoRotateEnabled { get; set; }

            [JsonProperty(PropertyName = "autoRotateSeconds")]
            public string AutoRotateSeconds { get; set; }

            // ----- Jumps remaining (shown first/by default) -----
            [JsonProperty(PropertyName = "jumpsEnabled")]
            public string JumpsEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "jumpsImage")]
            public string JumpsImageFilename { get; set; }

            [JsonProperty(PropertyName = "jumpsColor")]
            public string JumpsColor { get; set; }

            [JsonProperty(PropertyName = "jumpsLabelColor")]
            public string JumpsLabelColor { get; set; }

            [JsonProperty(PropertyName = "jumpsTextVerticalPosition")]
            public string JumpsTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "jumpsTextBold")]
            public string JumpsTextBold { get; set; }

            [JsonProperty(PropertyName = "jumpsLabelText")]
            public string JumpsLabelText { get; set; }

            [JsonProperty(PropertyName = "jumpsLabelTextVerticalPosition")]
            public string JumpsLabelTextVerticalPosition { get; set; }

            // ----- Remaining LY to final waypoint -----
            [JsonProperty(PropertyName = "remainingEnabled")]
            public string RemainingEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "remainingImage")]
            public string RemainingImageFilename { get; set; }

            [JsonProperty(PropertyName = "remainingColor")]
            public string RemainingColor { get; set; }

            [JsonProperty(PropertyName = "remainingLabelColor")]
            public string RemainingLabelColor { get; set; }

            [JsonProperty(PropertyName = "remainingTextVerticalPosition")]
            public string RemainingTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "remainingTextBold")]
            public string RemainingTextBold { get; set; }

            [JsonProperty(PropertyName = "remainingLabelText")]
            public string RemainingLabelText { get; set; }

            [JsonProperty(PropertyName = "remainingLabelTextVerticalPosition")]
            public string RemainingLabelTextVerticalPosition { get; set; }

            // ----- Distance to next system in route -----
            [JsonProperty(PropertyName = "nextEnabled")]
            public string NextEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "nextImage")]
            public string NextImageFilename { get; set; }

            [JsonProperty(PropertyName = "nextColor")]
            public string NextColor { get; set; }

            [JsonProperty(PropertyName = "nextLabelColor")]
            public string NextLabelColor { get; set; }

            [JsonProperty(PropertyName = "nextTextVerticalPosition")]
            public string NextTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "nextTextBold")]
            public string NextTextBold { get; set; }

            [JsonProperty(PropertyName = "nextLabelText")]
            public string NextLabelText { get; set; }

            [JsonProperty(PropertyName = "nextLabelTextVerticalPosition")]
            public string NextLabelTextVerticalPosition { get; set; }

            // ----- Fuel available -----
            [JsonProperty(PropertyName = "fuelEnabled")]
            public string FuelEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "fuelImage")]
            public string FuelImageFilename { get; set; }

            [JsonProperty(PropertyName = "fuelColor")]
            public string FuelColor { get; set; }

            [JsonProperty(PropertyName = "fuelLabelColor")]
            public string FuelLabelColor { get; set; }

            [JsonProperty(PropertyName = "fuelTextVerticalPosition")]
            public string FuelTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "fuelTextBold")]
            public string FuelTextBold { get; set; }

            [JsonProperty(PropertyName = "fuelLabelText")]
            public string FuelLabelText { get; set; }

            [JsonProperty(PropertyName = "fuelLabelTextVerticalPosition")]
            public string FuelLabelTextVerticalPosition { get; set; }

            // ----- Route progress % -----
            [JsonProperty(PropertyName = "progressEnabled")]
            public string ProgressEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "progressImage")]
            public string ProgressImageFilename { get; set; }

            [JsonProperty(PropertyName = "progressColor")]
            public string ProgressColor { get; set; }

            [JsonProperty(PropertyName = "progressLabelColor")]
            public string ProgressLabelColor { get; set; }

            [JsonProperty(PropertyName = "progressTextVerticalPosition")]
            public string ProgressTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "progressTextBold")]
            public string ProgressTextBold { get; set; }

            [JsonProperty(PropertyName = "progressLabelText")]
            public string ProgressLabelText { get; set; }

            [JsonProperty(PropertyName = "progressLabelTextVerticalPosition")]
            public string ProgressLabelTextVerticalPosition { get; set; }

            // ----- Estimated current jump range -----
            [JsonProperty(PropertyName = "jumpRangeEnabled")]
            public string JumpRangeEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "jumpRangeImage")]
            public string JumpRangeImageFilename { get; set; }

            [JsonProperty(PropertyName = "jumpRangeColor")]
            public string JumpRangeColor { get; set; }

            [JsonProperty(PropertyName = "jumpRangeLabelColor")]
            public string JumpRangeLabelColor { get; set; }

            [JsonProperty(PropertyName = "jumpRangeTextVerticalPosition")]
            public string JumpRangeTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "jumpRangeTextBold")]
            public string JumpRangeTextBold { get; set; }

            [JsonProperty(PropertyName = "jumpRangeLabelText")]
            public string JumpRangeLabelText { get; set; }

            [JsonProperty(PropertyName = "jumpRangeLabelTextVerticalPosition")]
            public string JumpRangeLabelTextVerticalPosition { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "clickSound")]
            public string ClickSoundFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "errorSound")]
            public string ErrorSoundFilename { get; set; }
        }

        private PluginSettings settings;

        private Bitmap _noRouteImage = null;
        private bool _noRouteImageIsGif = false;
        private string _noRouteFile;

        // Fixed cycle order: Jumps is always first (the default display)
        private static readonly string[] OptionOrder = { "Jumps", "Remaining", "Next", "Progress", "Fuel", "JumpRange" };
        private int _currentOptionIndex = 0;
        private int _autoRotateTickCount = 0;

        private Bitmap _jumpsImage = null;
        private bool _jumpsImageIsGif = false;
        private string _jumpsFile;
        private SolidBrush _jumpsBrush = new SolidBrush(Color.White);
        private SolidBrush _jumpsLabelBrush = new SolidBrush(Color.White);

        private Bitmap _remainingImage = null;
        private bool _remainingImageIsGif = false;
        private string _remainingFile;
        private SolidBrush _remainingBrush = new SolidBrush(Color.White);
        private SolidBrush _remainingLabelBrush = new SolidBrush(Color.White);

        private Bitmap _nextImage = null;
        private bool _nextImageIsGif = false;
        private string _nextFile;
        private SolidBrush _nextBrush = new SolidBrush(Color.White);
        private SolidBrush _nextLabelBrush = new SolidBrush(Color.White);

        private Bitmap _fuelImage = null;
        private bool _fuelImageIsGif = false;
        private string _fuelFile;
        private SolidBrush _fuelBrush = new SolidBrush(Color.White);
        private SolidBrush _fuelLabelBrush = new SolidBrush(Color.White);

        private Bitmap _progressImage = null;
        private bool _progressImageIsGif = false;
        private string _progressFile;
        private SolidBrush _progressBrush = new SolidBrush(Color.White);
        private SolidBrush _progressLabelBrush = new SolidBrush(Color.White);

        private Bitmap _jumpRangeImage = null;
        private bool _jumpRangeImageIsGif = false;
        private string _jumpRangeFile;
        private SolidBrush _jumpRangeBrush = new SolidBrush(Color.White);
        private SolidBrush _jumpRangeLabelBrush = new SolidBrush(Color.White);

        private CachedSound _clickSound = null;
        private CachedSound _errorSound = null;

        // ===================== value calculations =====================

        private static double Distance3D(SystemPosition a, SystemPosition b)
        {
            var dx = (double)(a.X - b.X);
            var dy = (double)(a.Y - b.Y);
            var dz = (double)(a.Z - b.Z);

            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        // EliteData.RouteList holds the FULL plotted route (minus the origin) exactly as it was
        // when NavRoute.json was last read - it does NOT shrink as jumps are made, because the
        // game only writes that file once, when the route is set. EliteData.RemainingJumpsInRoute,
        // on the other hand, IS kept live and correct by the game itself (via FSDTarget). So rather
        // than trying to mutate RouteList in place as jumps happen, every distance calculation
        // realigns the two on the fly: skip however many waypoints the live jump count says are
        // already behind us. This self-corrects on every read, regardless of backfill timing.
        private static System.Collections.Generic.IReadOnlyList<RouteItem> GetEffectiveRemainingRoute()
        {
            var routeList = EliteData.RouteList;

            if (routeList == null || routeList.Count == 0)
            {
                return routeList;
            }

            var remaining = EliteData.RemainingJumpsInRoute;

            if (remaining <= 0 || remaining >= routeList.Count)
            {
                return routeList;
            }

            var alreadyCompleted = routeList.Count - remaining;

            return routeList.Skip(alreadyCompleted).ToList();
        }

        // Straight-line distance from current position to the next system in the route
        private double GetNextSystemDistanceLy()
        {
            var route = GetEffectiveRemainingRoute();

            if (route == null || route.Count == 0 || EliteData.CurrentStarPos is null)
            {
                return 0;
            }

            return Distance3D(EliteData.CurrentStarPos, route[0].StarPos);
        }

        // Sum of leg distances from current position through every remaining route waypoint
        private double GetRemainingRouteLengthLy()
        {
            var route = GetEffectiveRemainingRoute();

            if (route == null || route.Count == 0 || EliteData.CurrentStarPos is null)
            {
                return 0;
            }

            double total = 0;
            var previousPos = EliteData.CurrentStarPos;

            foreach (var routeItem in route)
            {
                total += Distance3D(previousPos, routeItem.StarPos);
                previousPos = routeItem.StarPos;
            }

            return total;
        }

        private double GetFuelLevel()
        {
            return EliteData.StatusData?.Fuel?.FuelMain ?? 0;
        }

        private double GetProgressPercent()
        {
            if (EliteData.TotalJumpsInRoute <= 0)
            {
                return 0;
            }

            var done = EliteData.TotalJumpsInRoute - EliteData.RemainingJumpsInRoute;

            if (done < 0)
            {
                done = 0;
            }

            return (done / (double)EliteData.TotalJumpsInRoute) * 100.0;
        }

        // The game doesn't write out a live "current jump range" anywhere in the journal - the
        // closest it gives us is BaseJumpRange (Loadout's MaxJumpRange: best case, empty cargo,
        // fuel for one jump) and UnladenMass (hull + modules only, from that same event).
        //
        // FSD jump range scales roughly inversely with total ship mass, so we approximate today's
        // range by scaling BaseJumpRange down using how much heavier the ship is right now (current
        // fuel + cargo on top of UnladenMass) versus that baseline unladen weight:
        //
        //     estimate = BaseJumpRange x (UnladenMass / currentTotalMass)
        //
        // This is an estimate, not the exact in-game formula (it ignores the small amount of fuel
        // the baseline figure already assumes for one jump), but it tracks fuel burn and cargo
        // weight closely enough for an at-a-glance Stream Deck readout.
        private double GetEstimatedJumpRangeLy()
        {
            if (EliteData.BaseJumpRange <= 0 || EliteData.UnladenMass <= 0)
            {
                return 0;
            }

            var currentTotalMass = EliteData.UnladenMass + GetFuelLevel() + (EliteData.StatusData?.Cargo ?? 0);

            if (currentTotalMass <= 0)
            {
                return 0;
            }

            return EliteData.BaseJumpRange * (EliteData.UnladenMass / currentTotalMass);
        }

        // ===================== cycling helpers =====================

        // Is the option at this OptionOrder name turned on in settings?
        private bool IsOptionEnabled(string optionName)
        {
            switch (optionName)
            {
                case "Jumps": return settings.JumpsEnabled == "true";
                case "Remaining": return settings.RemainingEnabled == "true";
                case "Next": return settings.NextEnabled == "true";
                case "Fuel": return settings.FuelEnabled == "true";
                case "JumpRange": return settings.JumpRangeEnabled == "true";
                case "Progress": return settings.ProgressEnabled == "true";
                default: return false;
            }
        }

        // Returns (bitmap, base64, isGif, brush, verticalPosition, bold, displayText, labelText, labelPosition, labelBrush) for a given option name
        private (Bitmap image, string file, bool isGif, SolidBrush brush, string verticalPosition, string bold, string text, string label, string labelPosition, SolidBrush labelBrush) GetOptionData(string optionName)
        {
            switch (optionName)
            {
                case "Jumps":
                    return (_jumpsImage, _jumpsFile, _jumpsImageIsGif, _jumpsBrush, settings.JumpsTextVerticalPosition, settings.JumpsTextBold,
                        EliteData.RemainingJumpsInRoute.ToString(), settings.JumpsLabelText, settings.JumpsLabelTextVerticalPosition, _jumpsLabelBrush);
                case "Remaining":
                    return (_remainingImage, _remainingFile, _remainingImageIsGif, _remainingBrush, settings.RemainingTextVerticalPosition, settings.RemainingTextBold,
                        GetRemainingRouteLengthLy().ToString("0.0") + " Ly", settings.RemainingLabelText, settings.RemainingLabelTextVerticalPosition, _remainingLabelBrush);
                case "Next":
                    return (_nextImage, _nextFile, _nextImageIsGif, _nextBrush, settings.NextTextVerticalPosition, settings.NextTextBold,
                        GetNextSystemDistanceLy().ToString("0.0") + " Ly", settings.NextLabelText, settings.NextLabelTextVerticalPosition, _nextLabelBrush);
                case "Fuel":
                    return (_fuelImage, _fuelFile, _fuelImageIsGif, _fuelBrush, settings.FuelTextVerticalPosition, settings.FuelTextBold,
                        GetFuelLevel().ToString("0.0") + "T", settings.FuelLabelText, settings.FuelLabelTextVerticalPosition, _fuelLabelBrush);
                case "Progress":
                    return (_progressImage, _progressFile, _progressImageIsGif, _progressBrush, settings.ProgressTextVerticalPosition, settings.ProgressTextBold,
                        GetProgressPercent().ToString("0") + "%", settings.ProgressLabelText, settings.ProgressLabelTextVerticalPosition, _progressLabelBrush);
                case "JumpRange":
                    return (_jumpRangeImage, _jumpRangeFile, _jumpRangeImageIsGif, _jumpRangeBrush, settings.JumpRangeTextVerticalPosition, settings.JumpRangeTextBold,
                        GetEstimatedJumpRangeLy().ToString("0.0") + " Ly", settings.JumpRangeLabelText, settings.JumpRangeLabelTextVerticalPosition, _jumpRangeLabelBrush);
                default:
                    return (null, null, false, null, "160", "true", string.Empty, string.Empty, "5", null);
            }
        }

        // Moves _currentOptionIndex forward to the next enabled option. Wraps around. Does nothing if none enabled.
        private void AdvanceToNextEnabledOption()
        {
            for (int step = 1; step <= OptionOrder.Length; step++)
            {
                var candidate = (_currentOptionIndex + step) % OptionOrder.Length;
                if (IsOptionEnabled(OptionOrder[candidate]))
                {
                    _currentOptionIndex = candidate;
                    return;
                }
            }
        }

        // If the currently selected option got disabled in settings, jump to the first enabled one (starting from Jumps)
        private void EnsureCurrentOptionIsEnabled()
        {
            if (IsOptionEnabled(OptionOrder[_currentOptionIndex]))
            {
                return;
            }

            for (int i = 0; i < OptionOrder.Length; i++)
            {
                if (IsOptionEnabled(OptionOrder[i]))
                {
                    _currentOptionIndex = i;
                    return;
                }
            }
        }

        // ===================== drawing =====================

        // Mirrors Alert.cs's text fitting: tries font sizes from large to small and picks the
        // largest one whose width still fits the button, so both the label and the value are
        // drawn as large as possible. verticalPosition follows the same 256-wide scale used
        // throughout this plugin (e.g. 5 = very top, 160 = very bottom).
        private void DrawFittedText(Graphics graphics, string text, Color color, double verticalPosition, bool bold, int width)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var fontStyle = bold ? FontStyle.Bold : FontStyle.Regular;
            var lines = text.Replace("\r\n", "\n").Replace("\\n", "\n").Split('\n');
            var brush = new SolidBrush(color);

            // Scale the ceiling by image width (same 256-baseline convention used elsewhere in this
            // plugin), but keep it modest - the label and value share one image (top half / bottom
            // half), so neither should be sized as if it owned the whole button.
            var maxFontSize = (int)(48 * (width / 256.0));
            if (maxFontSize < 10)
            {
                maxFontSize = 10;
            }

            // Treat the button as roughly square: cap text height to a bit under half the button,
            // so a short value like "14" stops growing once it would crowd the label above/below it.
            var maxLineHeight = width * 0.40f;

            for (int adjustedSize = maxFontSize; adjustedSize >= 10; adjustedSize--)
            {
                var testFont = new Font("Arial", adjustedSize, fontStyle);
                bool fits = true;
                var lineHeights = new float[lines.Length];

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i];
                    if (string.IsNullOrEmpty(line)) { lineHeights[i] = testFont.Height; continue; }

                    var sf = new StringFormat(StringFormat.GenericTypographic);
                    sf.SetMeasurableCharacterRanges(new[] { new CharacterRange(0, line.Length) });
                    var regions = graphics.MeasureCharacterRanges(line, testFont, new RectangleF(0, 0, 1000, 1000), sf);
                    var bounds = regions[0].GetBounds(graphics);
                    lineHeights[i] = bounds.Height;

                    if (bounds.Width > width * 0.85f || bounds.Height > maxLineHeight)
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
                        graphics.DrawString(lines[i], testFont, brush, x, currentY - b.Y, drawFmt);
                        currentY += b.Height * 1.1f;
                    }
                    testFont.Dispose();
                    brush.Dispose();
                    return;
                }

                testFont.Dispose();
            }

            brush.Dispose();
        }

        
        private async Task HandleDisplay()
        {
            var remainingJumpsInRoute = EliteData.RemainingJumpsInRoute;
            var hasRoute = remainingJumpsInRoute > 0 && EliteData.RouteList != null && EliteData.RouteList.Count > 0;

            if (!hasRoute)
            {
                if (_noRouteImage != null)
                {
                    await Connection.SetImageAsync(_noRouteFile);
                }
                return;
            }

            EnsureCurrentOptionIsEnabled();

            var optionData = GetOptionData(OptionOrder[_currentOptionIndex]);
            var baseImage = optionData.image;
            var baseFile = optionData.file;
            var baseIsGif = optionData.isGif;

            // Fall back to the No Route image if the selected option has no image of its own set
            if (baseImage == null)
            {
                baseImage = _noRouteImage; baseFile = _noRouteFile; baseIsGif = _noRouteImageIsGif;
            }

            if (baseImage == null)
            {
                return;
            }

            var imgBase64 = baseFile;

            if (!baseIsGif)
            {
                try
                {
                    using (var bitmap = new Bitmap(baseImage))
                    {
                        using (var graphics = Graphics.FromImage(bitmap))
                        {
                            var width = bitmap.Width;
                            var isBold = optionData.bold == "true";
                            var valuePosition = double.TryParse(optionData.verticalPosition, out double parsedValuePosition) ? parsedValuePosition : 160.0;
                            var labelPosition = double.TryParse(optionData.labelPosition, out double parsedLabelPosition) ? parsedLabelPosition : 5.0;

                            DrawFittedText(graphics, optionData.label, optionData.labelBrush.Color, labelPosition, isBold, width);
                            DrawFittedText(graphics, optionData.text, optionData.brush.Color, valuePosition, isBold, width);
                        }

                        imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.FATAL, "RouteAdv HandleDisplay " + ex);
                }
            }

            await Connection.SetImageAsync(imgBase64);
        }

        public RouteAdv(SDConnection connection, InitialPayload payload) : base(connection, payload)
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

        public override void KeyPressed(KeyPayload payload)
        {
            if (StreamDeckCommon.InputRunning || Program.Binding == null)
            {
                StreamDeckCommon.ForceStop = true;
                return;
            }

            StreamDeckCommon.ForceStop = false;

            var remainingJumpsInRoute = EliteData.RemainingJumpsInRoute;
            var isDisabled = remainingJumpsInRoute == 0;

            if (!isDisabled)
            {
                AdvanceToNextEnabledOption();
                _autoRotateTickCount = 0;

                if (_clickSound != null)
                {
                    try
                    {
                        AudioPlaybackEngine.Instance.PlaySound(_clickSound);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogMessage(TracingLevel.FATAL, $"PlaySound: {ex}");
                    }
                }
            }
            else
            {
                if (_errorSound != null)
                {
                    try
                    {
                        AudioPlaybackEngine.Instance.PlaySound(_errorSound);
                    }
                    catch (Exception ex)
                    {
                        Logger.Instance.LogMessage(TracingLevel.FATAL, $"PlaySound: {ex}");
                    }
                }
            }

            AsyncHelper.RunSync(HandleDisplay);
        }

        public override void KeyReleased(KeyPayload payload)
        {

        }

        public override void Dispose()
        {
            base.Dispose();

            Program.JournalWatcher.MessageReceived -= HandleEliteEvents;
        }

        public override async void OnTick()
        {
            base.OnTick();

            if (settings.AutoRotateEnabled == "true")
            {
                var hasRoute = EliteData.RemainingJumpsInRoute > 0 && EliteData.RouteList != null && EliteData.RouteList.Count > 0;

                if (hasRoute)
                {
                    var rotateSeconds = int.TryParse(settings.AutoRotateSeconds, out int parsedSeconds) && parsedSeconds > 0 ? parsedSeconds : 5;

                    _autoRotateTickCount++;

                    if (_autoRotateTickCount >= rotateSeconds)
                    {
                        _autoRotateTickCount = 0;
                        AdvanceToNextEnabledOption();
                    }
                }
            }

            await HandleDisplay();
        }

        public override void ReceivedSettings(ReceivedSettingsPayload payload)
        {
            BarRaider.SdTools.Tools.AutoPopulateSettings(settings, payload.Settings);
            InitializeSettings();

            AsyncHelper.RunSync(HandleDisplay);
        }

        private void LoadImage(ref Bitmap image, ref string file, ref bool isGif, string filename)
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
                file = null;
                isGif = false;
            }

            if (File.Exists(filename))
            {
                image = (Bitmap)Image.FromFile(filename);
                file = Tools.FileToBase64(filename, true);
                isGif = StreamDeckCommon.CheckForGif(filename);
            }
        }

        private void InitializeSettings()
        {
            _clickSound = null;
            if (File.Exists(settings.ClickSoundFilename))
            {
                try
                {
                    _clickSound = new CachedSound(settings.ClickSoundFilename);
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.FATAL, $"CachedSound: {settings.ClickSoundFilename} {ex}");

                    _clickSound = null;
                    settings.ClickSoundFilename = null;
                }
            }

            _errorSound = null;
            if (File.Exists(settings.ErrorSoundFilename))
            {
                try
                {
                    _errorSound = new CachedSound(settings.ErrorSoundFilename);
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.FATAL, $"CachedSound: {settings.ErrorSoundFilename} {ex}");

                    _errorSound = null;
                    settings.ErrorSoundFilename = null;
                }
            }

            if (string.IsNullOrEmpty(settings.AutoRotateEnabled)) settings.AutoRotateEnabled = "false";
            if (string.IsNullOrEmpty(settings.AutoRotateSeconds)) settings.AutoRotateSeconds = "5";

            if (string.IsNullOrEmpty(settings.JumpsEnabled)) settings.JumpsEnabled = "true";
            if (string.IsNullOrEmpty(settings.RemainingEnabled)) settings.RemainingEnabled = "true";
            if (string.IsNullOrEmpty(settings.NextEnabled)) settings.NextEnabled = "false";
            if (string.IsNullOrEmpty(settings.FuelEnabled)) settings.FuelEnabled = "false";
            if (string.IsNullOrEmpty(settings.ProgressEnabled)) settings.ProgressEnabled = "false";
            if (string.IsNullOrEmpty(settings.JumpRangeEnabled)) settings.JumpRangeEnabled = "false";

            if (string.IsNullOrEmpty(settings.JumpsColor)) settings.JumpsColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.RemainingColor)) settings.RemainingColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.NextColor)) settings.NextColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.FuelColor)) settings.FuelColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.ProgressColor)) settings.ProgressColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.JumpRangeColor)) settings.JumpRangeColor = "#ffffff";

            if (string.IsNullOrEmpty(settings.JumpsLabelColor)) settings.JumpsLabelColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.RemainingLabelColor)) settings.RemainingLabelColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.NextLabelColor)) settings.NextLabelColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.FuelLabelColor)) settings.FuelLabelColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.ProgressLabelColor)) settings.ProgressLabelColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.JumpRangeLabelColor)) settings.JumpRangeLabelColor = "#ffffff";

            if (string.IsNullOrEmpty(settings.JumpsTextVerticalPosition)) settings.JumpsTextVerticalPosition = "160";
            if (string.IsNullOrEmpty(settings.RemainingTextVerticalPosition)) settings.RemainingTextVerticalPosition = "160";
            if (string.IsNullOrEmpty(settings.NextTextVerticalPosition)) settings.NextTextVerticalPosition = "160";
            if (string.IsNullOrEmpty(settings.FuelTextVerticalPosition)) settings.FuelTextVerticalPosition = "160";
            if (string.IsNullOrEmpty(settings.ProgressTextVerticalPosition)) settings.ProgressTextVerticalPosition = "160";
            if (string.IsNullOrEmpty(settings.JumpRangeTextVerticalPosition)) settings.JumpRangeTextVerticalPosition = "160";

            if (string.IsNullOrEmpty(settings.JumpsTextBold)) settings.JumpsTextBold = "true";
            if (string.IsNullOrEmpty(settings.RemainingTextBold)) settings.RemainingTextBold = "true";
            if (string.IsNullOrEmpty(settings.NextTextBold)) settings.NextTextBold = "true";
            if (string.IsNullOrEmpty(settings.FuelTextBold)) settings.FuelTextBold = "true";
            if (string.IsNullOrEmpty(settings.ProgressTextBold)) settings.ProgressTextBold = "true";
            if (string.IsNullOrEmpty(settings.JumpRangeTextBold)) settings.JumpRangeTextBold = "true";

            if (string.IsNullOrEmpty(settings.JumpsLabelText)) settings.JumpsLabelText = "Jumps Remaining";
            if (string.IsNullOrEmpty(settings.RemainingLabelText)) settings.RemainingLabelText = "Destination";
            if (string.IsNullOrEmpty(settings.NextLabelText)) settings.NextLabelText = "Next System";
            if (string.IsNullOrEmpty(settings.ProgressLabelText)) settings.ProgressLabelText = "Trip Progress";
            if (string.IsNullOrEmpty(settings.FuelLabelText)) settings.FuelLabelText = "Fuel Status";
            if (string.IsNullOrEmpty(settings.JumpRangeLabelText)) settings.JumpRangeLabelText = "Jump Range";

            if (string.IsNullOrEmpty(settings.JumpsLabelTextVerticalPosition)) settings.JumpsLabelTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.RemainingLabelTextVerticalPosition)) settings.RemainingLabelTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.NextLabelTextVerticalPosition)) settings.NextLabelTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.FuelLabelTextVerticalPosition)) settings.FuelLabelTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.ProgressLabelTextVerticalPosition)) settings.ProgressLabelTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.JumpRangeLabelTextVerticalPosition)) settings.JumpRangeLabelTextVerticalPosition = "5";

            try
            {
                var converter = new ColorConverter();

                _jumpsBrush = new SolidBrush((Color)converter.ConvertFromString(settings.JumpsColor));
                _remainingBrush = new SolidBrush((Color)converter.ConvertFromString(settings.RemainingColor));
                _nextBrush = new SolidBrush((Color)converter.ConvertFromString(settings.NextColor));
                _fuelBrush = new SolidBrush((Color)converter.ConvertFromString(settings.FuelColor));
                _progressBrush = new SolidBrush((Color)converter.ConvertFromString(settings.ProgressColor));
                _jumpRangeBrush = new SolidBrush((Color)converter.ConvertFromString(settings.JumpRangeColor));

                _jumpsLabelBrush = new SolidBrush((Color)converter.ConvertFromString(settings.JumpsLabelColor));
                _remainingLabelBrush = new SolidBrush((Color)converter.ConvertFromString(settings.RemainingLabelColor));
                _nextLabelBrush = new SolidBrush((Color)converter.ConvertFromString(settings.NextLabelColor));
                _fuelLabelBrush = new SolidBrush((Color)converter.ConvertFromString(settings.FuelLabelColor));
                _progressLabelBrush = new SolidBrush((Color)converter.ConvertFromString(settings.ProgressLabelColor));
                _jumpRangeLabelBrush = new SolidBrush((Color)converter.ConvertFromString(settings.JumpRangeLabelColor));

                LoadImage(ref _noRouteImage, ref _noRouteFile, ref _noRouteImageIsGif, settings.NoRouteImageFilename);
                LoadImage(ref _jumpsImage, ref _jumpsFile, ref _jumpsImageIsGif, settings.JumpsImageFilename);
                LoadImage(ref _remainingImage, ref _remainingFile, ref _remainingImageIsGif, settings.RemainingImageFilename);
                LoadImage(ref _nextImage, ref _nextFile, ref _nextImageIsGif, settings.NextImageFilename);
                LoadImage(ref _fuelImage, ref _fuelFile, ref _fuelImageIsGif, settings.FuelImageFilename);
                LoadImage(ref _progressImage, ref _progressFile, ref _progressImageIsGif, settings.ProgressImageFilename);
                LoadImage(ref _jumpRangeImage, ref _jumpRangeFile, ref _jumpRangeImageIsGif, settings.JumpRangeImageFilename);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "RouteAdv InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }

    }
}
