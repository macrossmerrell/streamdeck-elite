using System;
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
    [PluginActionId("com.mhwlng.elite.onfootexploration")]
    public class OnFootExploration : EliteKeypadBase
    {
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                return new PluginSettings
                {
                    InactiveImageFilename = string.Empty,

                    AutoRotateEnabled = "false",
                    AutoRotateSeconds = "5",

                    OxygenEnabled = "true",
                    OxygenImageFilename = string.Empty,
                    OxygenLabelText = "OXYGEN",
                    OxygenLabelTextVerticalPosition = "5",
                    OxygenLabelColor = "#ffffff",
                    OxygenTextVerticalPosition = "190",
                    OxygenTextBold = "true",
                    OxygenColor = "#ffffff",
                    OxygenWarningColor = "#ff4444",

                    HealthEnabled = "true",
                    HealthImageFilename = string.Empty,
                    HealthLabelText = "HEALTH",
                    HealthLabelTextVerticalPosition = "5",
                    HealthLabelColor = "#ffffff",
                    HealthTextVerticalPosition = "190",
                    HealthTextBold = "true",
                    HealthColor = "#ffffff",
                    HealthWarningColor = "#ff4444",

                    TemperatureEnabled = "false",
                    TemperatureImageFilename = string.Empty,
                    TemperatureLabelText = "TEMPERATURE",
                    TemperatureLabelTextVerticalPosition = "5",
                    TemperatureLabelColor = "#ffffff",
                    TemperatureTextVerticalPosition = "190",
                    TemperatureTextBold = "true",
                    TemperatureColor = "#ffffff",

                    GravityEnabled = "false",
                    GravityImageFilename = string.Empty,
                    GravityLabelText = "GRAVITY",
                    GravityLabelTextVerticalPosition = "5",
                    GravityLabelColor = "#ffffff",
                    GravityTextVerticalPosition = "190",
                    GravityTextBold = "true",
                    GravityColor = "#ffffff",

                    AtmosphereEnabled = "false",
                    AtmosphereImageFilename = string.Empty,
                    AtmosphereWarningImageFilename = string.Empty,
                    AtmosphereLabelText = "ATMOSPHERE",
                    AtmosphereOkText = "BREATHABLE",
                    AtmosphereWarningText = "NO OXYGEN",
                    AtmosphereLabelTextVerticalPosition = "5",
                    AtmosphereLabelOkColor = "#ffffff",
                    AtmosphereLabelWarningColor = "#ffffff",
                    AtmosphereTextVerticalPosition = "190",
                    AtmosphereTextBold = "true",
                    AtmosphereColor = "#00ff88",
                    AtmosphereWarningColor = "#ff4444",

                    ThermalEnabled = "false",
                    ThermalImageFilename = string.Empty,
                    ThermalColdImageFilename = string.Empty,
                    ThermalVeryColdImageFilename = string.Empty,
                    ThermalHotImageFilename = string.Empty,
                    ThermalVeryHotImageFilename = string.Empty,
                    ThermalOkText = "SAFE",
                    ThermalColdText = "COLD",
                    ThermalVeryColdText = "VERY COLD",
                    ThermalHotText = "HOT",
                    ThermalVeryHotText = "VERY HOT",
                    ThermalLabelTextVerticalPosition = "5",
                    ThermalTextVerticalPosition = "190",
                    ThermalTextBold = "true",
                    ThermalTempOkColor = "#ffffff",
                    ThermalTempColdColor = "#ffffff",
                    ThermalTempVeryColdColor = "#ffffff",
                    ThermalTempHotColor = "#ffffff",
                    ThermalTempVeryHotColor = "#ffffff",
                    ThermalOkColor = "#ffffff",
                    ThermalColdColor = "#44aaff",
                    ThermalVeryColdColor = "#0055ff",
                    ThermalHotColor = "#ffaa00",
                    ThermalVeryHotColor = "#ff4400",

                    ClickSoundFilename = string.Empty
                };
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "inactiveImage")]
            public string InactiveImageFilename { get; set; }

            [JsonProperty(PropertyName = "autoRotateEnabled")]
            public string AutoRotateEnabled { get; set; }

            [JsonProperty(PropertyName = "autoRotateSeconds")]
            public string AutoRotateSeconds { get; set; }

            // ----- Oxygen -----
            [JsonProperty(PropertyName = "oxygenEnabled")]
            public string OxygenEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "oxygenImage")]
            public string OxygenImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "oxygenWarningImage")]
            public string OxygenWarningImageFilename { get; set; }

            [JsonProperty(PropertyName = "oxygenLabelText")]
            public string OxygenLabelText { get; set; }

            [JsonProperty(PropertyName = "oxygenLabelTextVerticalPosition")]
            public string OxygenLabelTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "oxygenLabelColor")]
            public string OxygenLabelColor { get; set; }

            [JsonProperty(PropertyName = "oxygenTextVerticalPosition")]
            public string OxygenTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "oxygenTextBold")]
            public string OxygenTextBold { get; set; }

            [JsonProperty(PropertyName = "oxygenColor")]
            public string OxygenColor { get; set; }

            [JsonProperty(PropertyName = "oxygenWarningColor")]
            public string OxygenWarningColor { get; set; }

            // ----- Health -----
            [JsonProperty(PropertyName = "healthEnabled")]
            public string HealthEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "healthImage")]
            public string HealthImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "healthWarningImage")]
            public string HealthWarningImageFilename { get; set; }

            [JsonProperty(PropertyName = "healthLabelText")]
            public string HealthLabelText { get; set; }

            [JsonProperty(PropertyName = "healthLabelTextVerticalPosition")]
            public string HealthLabelTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "healthLabelColor")]
            public string HealthLabelColor { get; set; }

            [JsonProperty(PropertyName = "healthTextVerticalPosition")]
            public string HealthTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "healthTextBold")]
            public string HealthTextBold { get; set; }

            [JsonProperty(PropertyName = "healthColor")]
            public string HealthColor { get; set; }

            [JsonProperty(PropertyName = "healthWarningColor")]
            public string HealthWarningColor { get; set; }

            // ----- Temperature -----
            [JsonProperty(PropertyName = "temperatureEnabled")]
            public string TemperatureEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "temperatureImage")]
            public string TemperatureImageFilename { get; set; }

            [JsonProperty(PropertyName = "temperatureLabelText")]
            public string TemperatureLabelText { get; set; }

            [JsonProperty(PropertyName = "temperatureLabelTextVerticalPosition")]
            public string TemperatureLabelTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "temperatureLabelColor")]
            public string TemperatureLabelColor { get; set; }

            [JsonProperty(PropertyName = "temperatureTextVerticalPosition")]
            public string TemperatureTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "temperatureTextBold")]
            public string TemperatureTextBold { get; set; }

            [JsonProperty(PropertyName = "temperatureColor")]
            public string TemperatureColor { get; set; }

            // ----- Gravity -----
            [JsonProperty(PropertyName = "gravityEnabled")]
            public string GravityEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "gravityImage")]
            public string GravityImageFilename { get; set; }

            [JsonProperty(PropertyName = "gravityLabelText")]
            public string GravityLabelText { get; set; }

            [JsonProperty(PropertyName = "gravityLabelTextVerticalPosition")]
            public string GravityLabelTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "gravityLabelColor")]
            public string GravityLabelColor { get; set; }

            [JsonProperty(PropertyName = "gravityTextVerticalPosition")]
            public string GravityTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "gravityTextBold")]
            public string GravityTextBold { get; set; }

            [JsonProperty(PropertyName = "gravityColor")]
            public string GravityColor { get; set; }

            // ----- Atmosphere -----
            [JsonProperty(PropertyName = "atmosphereEnabled")]
            public string AtmosphereEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "atmosphereImage")]
            public string AtmosphereImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "atmosphereWarningImage")]
            public string AtmosphereWarningImageFilename { get; set; }

            [JsonProperty(PropertyName = "atmosphereLabelText")]
            public string AtmosphereLabelText { get; set; }

            [JsonProperty(PropertyName = "atmosphereOkText")]
            public string AtmosphereOkText { get; set; }

            [JsonProperty(PropertyName = "atmosphereWarningText")]
            public string AtmosphereWarningText { get; set; }

            [JsonProperty(PropertyName = "atmosphereLabelTextVerticalPosition")]
            public string AtmosphereLabelTextVerticalPosition { get; set; }

            // Label color, one per atmosphere state (breathable / no oxygen)
            [JsonProperty(PropertyName = "atmosphereLabelOkColor")]
            public string AtmosphereLabelOkColor { get; set; }

            [JsonProperty(PropertyName = "atmosphereLabelWarningColor")]
            public string AtmosphereLabelWarningColor { get; set; }

            [JsonProperty(PropertyName = "atmosphereTextVerticalPosition")]
            public string AtmosphereTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "atmosphereTextBold")]
            public string AtmosphereTextBold { get; set; }

            // Normal (breathable) color and warning (no oxygen) color
            [JsonProperty(PropertyName = "atmosphereColor")]
            public string AtmosphereColor { get; set; }

            [JsonProperty(PropertyName = "atmosphereWarningColor")]
            public string AtmosphereWarningColor { get; set; }

            // ----- Thermal -----
            [JsonProperty(PropertyName = "thermalEnabled")]
            public string ThermalEnabled { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "thermalImage")]
            public string ThermalImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "thermalColdImage")]
            public string ThermalColdImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "thermalVeryColdImage")]
            public string ThermalVeryColdImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "thermalHotImage")]
            public string ThermalHotImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "thermalVeryHotImage")]
            public string ThermalVeryHotImageFilename { get; set; }

            [JsonProperty(PropertyName = "thermalOkText")]
            public string ThermalOkText { get; set; }

            [JsonProperty(PropertyName = "thermalColdText")]
            public string ThermalColdText { get; set; }

            [JsonProperty(PropertyName = "thermalVeryColdText")]
            public string ThermalVeryColdText { get; set; }

            [JsonProperty(PropertyName = "thermalHotText")]
            public string ThermalHotText { get; set; }

            [JsonProperty(PropertyName = "thermalVeryHotText")]
            public string ThermalVeryHotText { get; set; }

            [JsonProperty(PropertyName = "thermalLabelTextVerticalPosition")]
            public string ThermalLabelTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "thermalTextVerticalPosition")]
            public string ThermalTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "thermalTextBold")]
            public string ThermalTextBold { get; set; }

            // Color of the temperature reading, one per thermal state (shown at the bottom)
            [JsonProperty(PropertyName = "thermalTempOkColor")]
            public string ThermalTempOkColor { get; set; }

            [JsonProperty(PropertyName = "thermalTempColdColor")]
            public string ThermalTempColdColor { get; set; }

            [JsonProperty(PropertyName = "thermalTempVeryColdColor")]
            public string ThermalTempVeryColdColor { get; set; }

            [JsonProperty(PropertyName = "thermalTempHotColor")]
            public string ThermalTempHotColor { get; set; }

            [JsonProperty(PropertyName = "thermalTempVeryHotColor")]
            public string ThermalTempVeryHotColor { get; set; }

            // One color per thermal state (used for the state name shown at the top)
            [JsonProperty(PropertyName = "thermalOkColor")]
            public string ThermalOkColor { get; set; }

            [JsonProperty(PropertyName = "thermalColdColor")]
            public string ThermalColdColor { get; set; }

            [JsonProperty(PropertyName = "thermalVeryColdColor")]
            public string ThermalVeryColdColor { get; set; }

            [JsonProperty(PropertyName = "thermalHotColor")]
            public string ThermalHotColor { get; set; }

            [JsonProperty(PropertyName = "thermalVeryHotColor")]
            public string ThermalVeryHotColor { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "clickSound")]
            public string ClickSoundFilename { get; set; }
        }

        // ===================== fields =====================

        private PluginSettings settings;

        private static readonly string[] OptionOrder = { "Oxygen", "Health", "Temperature", "Gravity", "Atmosphere", "Thermal" };
        private int _currentOptionIndex = 0;
        private int _autoRotateTickCount = 0;

        private Bitmap _inactiveImage = null;
        private bool _inactiveImageIsGif = false;
        private string _inactiveFile;

        private Bitmap _oxygenImage = null;
        private bool _oxygenImageIsGif = false;
        private string _oxygenFile;
        private Bitmap _oxygenWarningImage = null;
        private bool _oxygenWarningImageIsGif = false;
        private string _oxygenWarningFile;
        private SolidBrush _oxygenLabelBrush = new SolidBrush(Color.White);
        private SolidBrush _oxygenBrush = new SolidBrush(Color.White);
        private SolidBrush _oxygenWarningBrush = new SolidBrush(Color.Red);

        private Bitmap _healthImage = null;
        private bool _healthImageIsGif = false;
        private string _healthFile;
        private Bitmap _healthWarningImage = null;
        private bool _healthWarningImageIsGif = false;
        private string _healthWarningFile;
        private SolidBrush _healthLabelBrush = new SolidBrush(Color.White);
        private SolidBrush _healthBrush = new SolidBrush(Color.White);
        private SolidBrush _healthWarningBrush = new SolidBrush(Color.Red);

        private Bitmap _temperatureImage = null;
        private bool _temperatureImageIsGif = false;
        private string _temperatureFile;
        private SolidBrush _temperatureLabelBrush = new SolidBrush(Color.White);
        private SolidBrush _temperatureBrush = new SolidBrush(Color.White);

        private Bitmap _gravityImage = null;
        private bool _gravityImageIsGif = false;
        private string _gravityFile;
        private SolidBrush _gravityLabelBrush = new SolidBrush(Color.White);
        private SolidBrush _gravityBrush = new SolidBrush(Color.White);

        private Bitmap _atmosphereImage = null;
        private bool _atmosphereImageIsGif = false;
        private string _atmosphereFile;
        private Bitmap _atmosphereWarningImage = null;
        private bool _atmosphereWarningImageIsGif = false;
        private string _atmosphereWarningFile;
        private SolidBrush _atmosphereLabelOkBrush = new SolidBrush(Color.White);
        private SolidBrush _atmosphereLabelWarningBrush = new SolidBrush(Color.White);
        private SolidBrush _atmosphereBrush = new SolidBrush(Color.White);
        private SolidBrush _atmosphereWarningBrush = new SolidBrush(Color.Red);

        private Bitmap _thermalImage = null;
        private bool _thermalImageIsGif = false;
        private string _thermalFile;
        private Bitmap _thermalColdImage = null;
        private bool _thermalColdImageIsGif = false;
        private string _thermalColdFile;
        private Bitmap _thermalVeryColdImage = null;
        private bool _thermalVeryColdImageIsGif = false;
        private string _thermalVeryColdFile;
        private Bitmap _thermalHotImage = null;
        private bool _thermalHotImageIsGif = false;
        private string _thermalHotFile;
        private Bitmap _thermalVeryHotImage = null;
        private bool _thermalVeryHotImageIsGif = false;
        private string _thermalVeryHotFile;
        private SolidBrush _thermalTempOkBrush = new SolidBrush(Color.White);
        private SolidBrush _thermalTempColdBrush = new SolidBrush(Color.White);
        private SolidBrush _thermalTempVeryColdBrush = new SolidBrush(Color.White);
        private SolidBrush _thermalTempHotBrush = new SolidBrush(Color.White);
        private SolidBrush _thermalTempVeryHotBrush = new SolidBrush(Color.White);
        private SolidBrush _thermalOkBrush = new SolidBrush(Color.White);
        private SolidBrush _thermalColdBrush = new SolidBrush(Color.Blue);
        private SolidBrush _thermalVeryColdBrush = new SolidBrush(Color.Blue);
        private SolidBrush _thermalHotBrush = new SolidBrush(Color.Orange);
        private SolidBrush _thermalVeryHotBrush = new SolidBrush(Color.OrangeRed);

        private CachedSound _clickSound = null;

        // ===================== cycling helpers =====================

        private bool IsOptionEnabled(string optionName)
        {
            switch (optionName)
            {
                case "Oxygen":      return settings.OxygenEnabled == "true";
                case "Health":      return settings.HealthEnabled == "true";
                case "Temperature": return settings.TemperatureEnabled == "true";
                case "Gravity":     return settings.GravityEnabled == "true";
                case "Atmosphere":  return settings.AtmosphereEnabled == "true";
                case "Thermal":     return settings.ThermalEnabled == "true";
                default:            return false;
            }
        }

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

        private void EnsureCurrentOptionIsEnabled()
        {
            if (IsOptionEnabled(OptionOrder[_currentOptionIndex])) return;

            for (int i = 0; i < OptionOrder.Length; i++)
            {
                if (IsOptionEnabled(OptionOrder[i]))
                {
                    _currentOptionIndex = i;
                    return;
                }
            }
        }

        // ===================== value helpers =====================

        private (string text, SolidBrush valueBrush, SolidBrush labelBrush, Bitmap image, string imageFile, bool imageIsGif,
                 string valuePosition, string labelPosition, string bold) GetOptionData(string optionName)
        {
            switch (optionName)
            {
                case "Oxygen":
                {
                    var pct = EliteData.StatusData.Oxygen * 100.0;
                    var isWarning = EliteData.StatusData.LowOxygen;
                    var brush = isWarning ? _oxygenWarningBrush : _oxygenBrush;
                    var img   = (isWarning && _oxygenWarningImage != null) ? _oxygenWarningImage : _oxygenImage;
                    var file  = (isWarning && _oxygenWarningImage != null) ? _oxygenWarningFile  : _oxygenFile;
                    var gif   = (isWarning && _oxygenWarningImage != null) ? _oxygenWarningImageIsGif : _oxygenImageIsGif;
                    return (pct.ToString("0") + "%", brush, _oxygenLabelBrush,
                            img, file, gif,
                            settings.OxygenTextVerticalPosition, settings.OxygenLabelTextVerticalPosition, settings.OxygenTextBold);
                }
                case "Health":
                {
                    var pct = EliteData.StatusData.Health * 100.0;
                    var isWarning = EliteData.StatusData.LowHealth;
                    var brush = isWarning ? _healthWarningBrush : _healthBrush;
                    var img   = (isWarning && _healthWarningImage != null) ? _healthWarningImage : _healthImage;
                    var file  = (isWarning && _healthWarningImage != null) ? _healthWarningFile  : _healthFile;
                    var gif   = (isWarning && _healthWarningImage != null) ? _healthWarningImageIsGif : _healthImageIsGif;
                    return (pct.ToString("0") + "%", brush, _healthLabelBrush,
                            img, file, gif,
                            settings.HealthTextVerticalPosition, settings.HealthLabelTextVerticalPosition, settings.HealthTextBold);
                }
                case "Temperature":
                {
                    var text = EliteData.StatusData.Temperature.ToString("0") + "K";
                    return (text, _temperatureBrush, _temperatureLabelBrush,
                            _temperatureImage, _temperatureFile, _temperatureImageIsGif,
                            settings.TemperatureTextVerticalPosition, settings.TemperatureLabelTextVerticalPosition, settings.TemperatureTextBold);
                }
                case "Gravity":
                {
                    var text = EliteData.StatusData.Gravity.ToString("0.00") + "g";
                    return (text, _gravityBrush, _gravityLabelBrush,
                            _gravityImage, _gravityFile, _gravityImageIsGif,
                            settings.GravityTextVerticalPosition, settings.GravityLabelTextVerticalPosition, settings.GravityTextBold);
                }
                case "Atmosphere":
                {
                    var breathable = EliteData.StatusData.BreathableAtmosphere;
                    var text  = breathable ? settings.AtmosphereOkText      : settings.AtmosphereWarningText;
                    var brush = breathable ? _atmosphereBrush               : _atmosphereWarningBrush;
                    var labelBrush = breathable ? _atmosphereLabelOkBrush   : _atmosphereLabelWarningBrush;
                    var img   = breathable ? _atmosphereImage                : (_atmosphereWarningImage ?? _atmosphereImage);
                    var file  = breathable ? _atmosphereFile                 : (_atmosphereWarningImage != null ? _atmosphereWarningFile : _atmosphereFile);
                    var gif   = breathable ? _atmosphereImageIsGif           : (_atmosphereWarningImage != null ? _atmosphereWarningImageIsGif : _atmosphereImageIsGif);
                    return (text, brush, labelBrush,
                            img, file, gif,
                            settings.AtmosphereTextVerticalPosition, settings.AtmosphereLabelTextVerticalPosition, settings.AtmosphereTextBold);
                }
                case "Thermal":
                {
                    SolidBrush stateBrush;
                    SolidBrush tempBrush;
                    Bitmap img;
                    string file;
                    bool gif;

                    if (EliteData.StatusData.VeryHot)
                    {
                        stateBrush = _thermalVeryHotBrush;
                        tempBrush  = _thermalTempVeryHotBrush;
                        img  = _thermalVeryHotImage ?? _thermalImage;
                        file = _thermalVeryHotImage != null ? _thermalVeryHotFile : _thermalFile;
                        gif  = _thermalVeryHotImage != null ? _thermalVeryHotImageIsGif : _thermalImageIsGif;
                    }
                    else if (EliteData.StatusData.Hot)
                    {
                        stateBrush = _thermalHotBrush;
                        tempBrush  = _thermalTempHotBrush;
                        img  = _thermalHotImage ?? _thermalImage;
                        file = _thermalHotImage != null ? _thermalHotFile : _thermalFile;
                        gif  = _thermalHotImage != null ? _thermalHotImageIsGif : _thermalImageIsGif;
                    }
                    else if (EliteData.StatusData.VeryCold)
                    {
                        stateBrush = _thermalVeryColdBrush;
                        tempBrush  = _thermalTempVeryColdBrush;
                        img  = _thermalVeryColdImage ?? _thermalImage;
                        file = _thermalVeryColdImage != null ? _thermalVeryColdFile : _thermalFile;
                        gif  = _thermalVeryColdImage != null ? _thermalVeryColdImageIsGif : _thermalImageIsGif;
                    }
                    else if (EliteData.StatusData.Cold)
                    {
                        stateBrush = _thermalColdBrush;
                        tempBrush  = _thermalTempColdBrush;
                        img  = _thermalColdImage ?? _thermalImage;
                        file = _thermalColdImage != null ? _thermalColdFile : _thermalFile;
                        gif  = _thermalColdImage != null ? _thermalColdImageIsGif : _thermalImageIsGif;
                    }
                    else
                    {
                        stateBrush = _thermalOkBrush;
                        tempBrush  = _thermalTempOkBrush;
                        img  = _thermalImage;
                        file = _thermalFile;
                        gif  = _thermalImageIsGif;
                    }

                    // Label (Very Top, fixed) = current state name, shown in that state's color.
                    // Value (bottom, default Edge) = the live temperature reading, in that
                    // state's own temperature color (independently configurable per state).
                    var tempText = EliteData.StatusData.Temperature.ToString("0") + "\u00B0K";

                    return (tempText, tempBrush, stateBrush,
                            img, file, gif,
                            settings.ThermalTextVerticalPosition, settings.ThermalLabelTextVerticalPosition, settings.ThermalTextBold);
                }
                default:
                    return (string.Empty, _oxygenBrush, _oxygenLabelBrush, null, null, false, "160", "5", "true");
            }
        }

        private string GetThermalStateText()
        {
            if (EliteData.StatusData.VeryHot)  return settings.ThermalVeryHotText;
            if (EliteData.StatusData.Hot)       return settings.ThermalHotText;
            if (EliteData.StatusData.VeryCold)  return settings.ThermalVeryColdText;
            if (EliteData.StatusData.Cold)      return settings.ThermalColdText;
            return settings.ThermalOkText;
        }

        private string GetLabelText(string optionName)
        {
            switch (optionName)
            {
                case "Oxygen":      return settings.OxygenLabelText;
                case "Health":      return settings.HealthLabelText;
                case "Temperature": return settings.TemperatureLabelText;
                case "Gravity":     return settings.GravityLabelText;
                case "Atmosphere":  return settings.AtmosphereLabelText;
                case "Thermal":     return GetThermalStateText();
                default:            return string.Empty;
            }
        }

        // ===================== drawing =====================

        private void DrawFittedText(Graphics graphics, string text, Color color, double verticalPosition, bool bold, int width)
        {
            if (string.IsNullOrEmpty(text)) return;

            var fontStyle = bold ? FontStyle.Bold : FontStyle.Regular;
            var lines = text.Replace("\r\n", "\n").Replace("\\n", "\n").Split('\n');
            var brush = new SolidBrush(color);

            var maxFontSize = (int)(48 * (width / 256.0));
            if (maxFontSize < 10) maxFontSize = 10;

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
            // Only active on foot on a planet surface
            if (!EliteData.StatusData.OnFootOnPlanet)
            {
                if (_inactiveImage != null)
                    await Connection.SetImageAsync(_inactiveFile);
                return;
            }

            EnsureCurrentOptionIsEnabled();

            var optionData = GetOptionData(OptionOrder[_currentOptionIndex]);
            var labelText = GetLabelText(OptionOrder[_currentOptionIndex]);

            var baseImage = optionData.image ?? _inactiveImage;
            var baseFile = optionData.image != null ? optionData.imageFile : _inactiveFile;
            var baseIsGif = optionData.image != null ? optionData.imageIsGif : _inactiveImageIsGif;

            if (baseImage == null) return;

            var imgBase64 = baseFile;

            if (!baseIsGif)
            {
                try
                {
                    using (var bitmap = new Bitmap(baseImage))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var width = bitmap.Width;
                        var isBold = optionData.bold == "true";
                        var valuePos = double.TryParse(optionData.valuePosition, out double vp) ? vp : 160.0;
                        var labelPos = double.TryParse(optionData.labelPosition, out double lp) ? lp : 5.0;

                        DrawFittedText(graphics, labelText, optionData.labelBrush.Color, labelPos, isBold, width);
                        DrawFittedText(graphics, optionData.text, optionData.valueBrush.Color, valuePos, isBold, width);

                        imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.FATAL, "OnFootExploration HandleDisplay " + ex);
                }
            }

            await Connection.SetImageAsync(imgBase64);
        }

        // ===================== lifecycle =====================

        public OnFootExploration(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            }

            Connection.SetTitleAsync(null).Wait();
            AsyncHelper.RunSync(HandleDisplay);

            Program.StatusWatcher.StatusUpdated += HandleStatusUpdate;
        }

        private void HandleStatusUpdate(object sender, EventArgs e)
        {
            AsyncHelper.RunSync(HandleDisplay);
        }

        public override void KeyPressed(KeyPayload payload)
        {
            if (!EliteData.StatusData.OnFootOnPlanet) return;

            AdvanceToNextEnabledOption();
            _autoRotateTickCount = 0;

            if (_clickSound != null)
            {
                try { AudioPlaybackEngine.Instance.PlaySound(_clickSound); }
                catch (Exception ex) { Logger.Instance.LogMessage(TracingLevel.FATAL, $"PlaySound: {ex}"); }
            }

            AsyncHelper.RunSync(HandleDisplay);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void Dispose()
        {
            Program.StatusWatcher.StatusUpdated -= HandleStatusUpdate;
            base.Dispose();
        }

        public override async void OnTick()
        {
            base.OnTick();

            if (settings.AutoRotateEnabled == "true" && EliteData.StatusData.OnFootOnPlanet)
            {
                var rotateSeconds = int.TryParse(settings.AutoRotateSeconds, out int parsedSeconds) && parsedSeconds > 0
                    ? parsedSeconds : 5;

                _autoRotateTickCount++;
                if (_autoRotateTickCount >= rotateSeconds)
                {
                    _autoRotateTickCount = 0;
                    AdvanceToNextEnabledOption();
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

        // ===================== initialization =====================

        private void LoadImage(ref Bitmap image, ref string file, ref bool isGif, string filename)
        {
            if (image != null) { image.Dispose(); image = null; file = null; isGif = false; }

            if (File.Exists(filename))
            {
                image = (Bitmap)Image.FromFile(filename);
                file = Tools.FileToBase64(filename, true);
                isGif = StreamDeckCommon.CheckForGif(filename);
            }
        }

        private SolidBrush ParseBrush(string hex, Color fallback)
        {
            try { return new SolidBrush((Color)new ColorConverter().ConvertFromString(hex)); }
            catch { return new SolidBrush(fallback); }
        }

        private void InitializeSettings()
        {
            // ── Enabled defaults ──
            if (string.IsNullOrEmpty(settings.OxygenEnabled))      settings.OxygenEnabled      = "true";
            if (string.IsNullOrEmpty(settings.HealthEnabled))       settings.HealthEnabled       = "true";
            if (string.IsNullOrEmpty(settings.TemperatureEnabled))  settings.TemperatureEnabled  = "false";
            if (string.IsNullOrEmpty(settings.GravityEnabled))      settings.GravityEnabled      = "false";
            if (string.IsNullOrEmpty(settings.AtmosphereEnabled))   settings.AtmosphereEnabled   = "false";
            if (string.IsNullOrEmpty(settings.ThermalEnabled))      settings.ThermalEnabled      = "false";

            // ── Auto-rotate defaults ──
            if (string.IsNullOrEmpty(settings.AutoRotateEnabled))   settings.AutoRotateEnabled   = "false";
            if (string.IsNullOrEmpty(settings.AutoRotateSeconds))   settings.AutoRotateSeconds   = "5";

            // ── Label text defaults ──
            if (string.IsNullOrEmpty(settings.OxygenLabelText))      settings.OxygenLabelText      = "OXYGEN";
            if (string.IsNullOrEmpty(settings.HealthLabelText))       settings.HealthLabelText       = "HEALTH";
            if (string.IsNullOrEmpty(settings.TemperatureLabelText))  settings.TemperatureLabelText  = "TEMPERATURE";
            if (string.IsNullOrEmpty(settings.GravityLabelText))      settings.GravityLabelText      = "GRAVITY";
            if (string.IsNullOrEmpty(settings.AtmosphereLabelText))   settings.AtmosphereLabelText   = "ATMOSPHERE";
            if (string.IsNullOrEmpty(settings.AtmosphereOkText))      settings.AtmosphereOkText      = "BREATHABLE";
            if (string.IsNullOrEmpty(settings.AtmosphereWarningText)) settings.AtmosphereWarningText = "NO OXYGEN";
            if (string.IsNullOrEmpty(settings.ThermalOkText))         settings.ThermalOkText         = "SAFE";
            if (string.IsNullOrEmpty(settings.ThermalColdText))       settings.ThermalColdText       = "COLD";
            if (string.IsNullOrEmpty(settings.ThermalVeryColdText))   settings.ThermalVeryColdText   = "VERY COLD";
            if (string.IsNullOrEmpty(settings.ThermalHotText))        settings.ThermalHotText        = "HOT";
            if (string.IsNullOrEmpty(settings.ThermalVeryHotText))    settings.ThermalVeryHotText    = "VERY HOT";

            // ── Label position defaults ──
            if (string.IsNullOrEmpty(settings.OxygenLabelTextVerticalPosition))      settings.OxygenLabelTextVerticalPosition      = "5";
            if (string.IsNullOrEmpty(settings.HealthLabelTextVerticalPosition))       settings.HealthLabelTextVerticalPosition       = "5";
            if (string.IsNullOrEmpty(settings.TemperatureLabelTextVerticalPosition))  settings.TemperatureLabelTextVerticalPosition  = "5";
            if (string.IsNullOrEmpty(settings.GravityLabelTextVerticalPosition))      settings.GravityLabelTextVerticalPosition      = "5";
            if (string.IsNullOrEmpty(settings.AtmosphereLabelTextVerticalPosition))   settings.AtmosphereLabelTextVerticalPosition   = "5";

            // ── Value position defaults ──
            if (string.IsNullOrEmpty(settings.OxygenTextVerticalPosition))      settings.OxygenTextVerticalPosition      = "190";
            if (string.IsNullOrEmpty(settings.HealthTextVerticalPosition))       settings.HealthTextVerticalPosition       = "190";
            if (string.IsNullOrEmpty(settings.TemperatureTextVerticalPosition))  settings.TemperatureTextVerticalPosition  = "190";
            if (string.IsNullOrEmpty(settings.GravityTextVerticalPosition))      settings.GravityTextVerticalPosition      = "190";
            if (string.IsNullOrEmpty(settings.AtmosphereTextVerticalPosition))   settings.AtmosphereTextVerticalPosition   = "190";
            if (string.IsNullOrEmpty(settings.ThermalLabelTextVerticalPosition))      settings.ThermalLabelTextVerticalPosition      = "5";
            if (string.IsNullOrEmpty(settings.ThermalTextVerticalPosition))      settings.ThermalTextVerticalPosition      = "190";

            // ── Bold defaults ──
            if (string.IsNullOrEmpty(settings.OxygenTextBold))      settings.OxygenTextBold      = "true";
            if (string.IsNullOrEmpty(settings.HealthTextBold))       settings.HealthTextBold       = "true";
            if (string.IsNullOrEmpty(settings.TemperatureTextBold))  settings.TemperatureTextBold  = "true";
            if (string.IsNullOrEmpty(settings.GravityTextBold))      settings.GravityTextBold      = "true";
            if (string.IsNullOrEmpty(settings.AtmosphereTextBold))   settings.AtmosphereTextBold   = "true";
            if (string.IsNullOrEmpty(settings.ThermalTextBold))      settings.ThermalTextBold      = "true";

            // ── Color defaults ──
            if (string.IsNullOrEmpty(settings.OxygenLabelColor))       settings.OxygenLabelColor       = "#ffffff";
            if (string.IsNullOrEmpty(settings.OxygenColor))            settings.OxygenColor            = "#ffffff";
            if (string.IsNullOrEmpty(settings.OxygenWarningColor))     settings.OxygenWarningColor     = "#ff4444";

            if (string.IsNullOrEmpty(settings.HealthLabelColor))       settings.HealthLabelColor       = "#ffffff";
            if (string.IsNullOrEmpty(settings.HealthColor))            settings.HealthColor            = "#ffffff";
            if (string.IsNullOrEmpty(settings.HealthWarningColor))     settings.HealthWarningColor     = "#ff4444";

            if (string.IsNullOrEmpty(settings.TemperatureLabelColor))  settings.TemperatureLabelColor  = "#ffffff";
            if (string.IsNullOrEmpty(settings.TemperatureColor))       settings.TemperatureColor       = "#ffffff";

            if (string.IsNullOrEmpty(settings.GravityLabelColor))      settings.GravityLabelColor      = "#ffffff";
            if (string.IsNullOrEmpty(settings.GravityColor))           settings.GravityColor           = "#ffffff";

            if (string.IsNullOrEmpty(settings.AtmosphereLabelOkColor))      settings.AtmosphereLabelOkColor      = "#ffffff";
            if (string.IsNullOrEmpty(settings.AtmosphereLabelWarningColor)) settings.AtmosphereLabelWarningColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.AtmosphereColor))        settings.AtmosphereColor        = "#00ff88";
            if (string.IsNullOrEmpty(settings.AtmosphereWarningColor)) settings.AtmosphereWarningColor = "#ff4444";

            if (string.IsNullOrEmpty(settings.ThermalTempOkColor))      settings.ThermalTempOkColor      = "#ffffff";
            if (string.IsNullOrEmpty(settings.ThermalTempColdColor))    settings.ThermalTempColdColor    = "#ffffff";
            if (string.IsNullOrEmpty(settings.ThermalTempVeryColdColor)) settings.ThermalTempVeryColdColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.ThermalTempHotColor))     settings.ThermalTempHotColor     = "#ffffff";
            if (string.IsNullOrEmpty(settings.ThermalTempVeryHotColor)) settings.ThermalTempVeryHotColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.ThermalOkColor))         settings.ThermalOkColor         = "#ffffff";
            if (string.IsNullOrEmpty(settings.ThermalColdColor))       settings.ThermalColdColor       = "#44aaff";
            if (string.IsNullOrEmpty(settings.ThermalVeryColdColor))   settings.ThermalVeryColdColor   = "#0055ff";
            if (string.IsNullOrEmpty(settings.ThermalHotColor))        settings.ThermalHotColor        = "#ffaa00";
            if (string.IsNullOrEmpty(settings.ThermalVeryHotColor))    settings.ThermalVeryHotColor    = "#ff4400";

            try
            {
                _oxygenLabelBrush    = ParseBrush(settings.OxygenLabelColor,       Color.White);
                _oxygenBrush         = ParseBrush(settings.OxygenColor,            Color.White);
                _oxygenWarningBrush  = ParseBrush(settings.OxygenWarningColor,     Color.Red);

                _healthLabelBrush    = ParseBrush(settings.HealthLabelColor,       Color.White);
                _healthBrush         = ParseBrush(settings.HealthColor,            Color.White);
                _healthWarningBrush  = ParseBrush(settings.HealthWarningColor,     Color.Red);

                _temperatureLabelBrush = ParseBrush(settings.TemperatureLabelColor, Color.White);
                _temperatureBrush      = ParseBrush(settings.TemperatureColor,      Color.White);

                _gravityLabelBrush   = ParseBrush(settings.GravityLabelColor,      Color.White);
                _gravityBrush        = ParseBrush(settings.GravityColor,           Color.White);

                _atmosphereLabelOkBrush      = ParseBrush(settings.AtmosphereLabelOkColor,      Color.White);
                _atmosphereLabelWarningBrush = ParseBrush(settings.AtmosphereLabelWarningColor, Color.White);
                _atmosphereBrush        = ParseBrush(settings.AtmosphereColor,        Color.FromArgb(0, 255, 136));
                _atmosphereWarningBrush = ParseBrush(settings.AtmosphereWarningColor, Color.Red);

                _thermalTempOkBrush       = ParseBrush(settings.ThermalTempOkColor,       Color.White);
                _thermalTempColdBrush     = ParseBrush(settings.ThermalTempColdColor,     Color.White);
                _thermalTempVeryColdBrush = ParseBrush(settings.ThermalTempVeryColdColor, Color.White);
                _thermalTempHotBrush      = ParseBrush(settings.ThermalTempHotColor,      Color.White);
                _thermalTempVeryHotBrush  = ParseBrush(settings.ThermalTempVeryHotColor,  Color.White);
                _thermalOkBrush       = ParseBrush(settings.ThermalOkColor,        Color.White);
                _thermalColdBrush     = ParseBrush(settings.ThermalColdColor,      Color.CornflowerBlue);
                _thermalVeryColdBrush = ParseBrush(settings.ThermalVeryColdColor,  Color.Blue);
                _thermalHotBrush      = ParseBrush(settings.ThermalHotColor,       Color.Orange);
                _thermalVeryHotBrush  = ParseBrush(settings.ThermalVeryHotColor,   Color.OrangeRed);

                LoadImage(ref _inactiveImage, ref _inactiveFile, ref _inactiveImageIsGif, settings.InactiveImageFilename);
                LoadImage(ref _oxygenImage,        ref _oxygenFile,        ref _oxygenImageIsGif,        settings.OxygenImageFilename);
                LoadImage(ref _oxygenWarningImage,  ref _oxygenWarningFile,  ref _oxygenWarningImageIsGif,  settings.OxygenWarningImageFilename);
                LoadImage(ref _healthImage,        ref _healthFile,        ref _healthImageIsGif,        settings.HealthImageFilename);
                LoadImage(ref _healthWarningImage,  ref _healthWarningFile,  ref _healthWarningImageIsGif,  settings.HealthWarningImageFilename);
                LoadImage(ref _temperatureImage,   ref _temperatureFile,   ref _temperatureImageIsGif,   settings.TemperatureImageFilename);
                LoadImage(ref _gravityImage,       ref _gravityFile,       ref _gravityImageIsGif,       settings.GravityImageFilename);
                LoadImage(ref _atmosphereImage,    ref _atmosphereFile,    ref _atmosphereImageIsGif,    settings.AtmosphereImageFilename);
                LoadImage(ref _atmosphereWarningImage, ref _atmosphereWarningFile, ref _atmosphereWarningImageIsGif, settings.AtmosphereWarningImageFilename);
                LoadImage(ref _thermalImage,       ref _thermalFile,       ref _thermalImageIsGif,       settings.ThermalImageFilename);
                LoadImage(ref _thermalColdImage,   ref _thermalColdFile,   ref _thermalColdImageIsGif,   settings.ThermalColdImageFilename);
                LoadImage(ref _thermalVeryColdImage, ref _thermalVeryColdFile, ref _thermalVeryColdImageIsGif, settings.ThermalVeryColdImageFilename);
                LoadImage(ref _thermalHotImage,    ref _thermalHotFile,    ref _thermalHotImageIsGif,    settings.ThermalHotImageFilename);
                LoadImage(ref _thermalVeryHotImage, ref _thermalVeryHotFile, ref _thermalVeryHotImageIsGif, settings.ThermalVeryHotImageFilename);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "OnFootExploration InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}
