using System;
using System.Collections.Generic;
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
    [PluginActionId("com.mhwlng.elite.alert")]
    public class Alert : EliteKeypadBase
    {
        // Alert priority order - higher index = lower priority
        public enum AlertType
        {
            SelfDestruct = 0,
            CockpitBreached = 1,
            SystemsShutdown = 2,
            JetConeDamage = 3,
            HeatWarning = 4,
            HeatDamage = 5,
            HullDamage = 6,
            ShieldsDown = 7,
            UnderAttack = 8,
            BeingInterdicted = 9,
            IsInDanger = 10,
            LowFuel = 11,
            DockingDenied = 12
        }

        private class ActiveAlert
        {
            public AlertType Type { get; set; }
            public DateTime ExpiresAt { get; set; } // DateTime.MaxValue = no timeout
        }

        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                return new PluginSettings
                {
                    // Default state
                    DefaultImageFilename = string.Empty,
                    DefaultText = string.Empty,
                    DefaultTextColor = "#ffffff",
                    DefaultTextVerticalPosition = "80",
                    DefaultTextBold = "true",

                    // Self Destruct
                    SelfDestructImageFilename = string.Empty,
                    SelfDestructText = "SELF DESTRUCT",
                    SelfDestructTextColor = "#ff0000",
                    SelfDestructTextVerticalPosition = "14",
                    SelfDestructTextBold = "true",
                    SelfDestructTimeout = "30",

                    // Cockpit Breached
                    CockpitBreachedImageFilename = string.Empty,
                    CockpitBreachedText = "COCKPIT BREACHED",
                    CockpitBreachedTextColor = "#ff4400",
                    CockpitBreachedTextVerticalPosition = "14",
                    CockpitBreachedTextBold = "true",
                    CockpitBreachedTimeout = "20",

                    // Systems Shutdown
                    SystemsShutdownImageFilename = string.Empty,
                    SystemsShutdownText = "SYSTEMS SHUTDOWN",
                    SystemsShutdownTextColor = "#ff4400",
                    SystemsShutdownTextVerticalPosition = "14",
                    SystemsShutdownTextBold = "true",
                    SystemsShutdownTimeout = "10",

                    // Jet Cone Damage
                    JetConeDamageImageFilename = string.Empty,
                    JetConeDamageText = "CONE DAMAGE",
                    JetConeDamageTextColor = "#ff8800",
                    JetConeDamageTextVerticalPosition = "14",
                    JetConeDamageTextBold = "true",
                    JetConeDamageTimeout = "5",

                    // Heat Damage
                    HeatDamageImageFilename = string.Empty,
                    HeatDamageText = "HEAT DAMAGE",
                    HeatDamageTextColor = "#ff4400",
                    HeatDamageTextVerticalPosition = "14",
                    HeatDamageTextBold = "true",
                    HeatDamageTimeout = "5",

                    // Hull Damage
                    HullDamageImageFilename = string.Empty,
                    HullDamageText = "HULL DAMAGE",
                    HullDamageTextColor = "#ff0000",
                    HullDamageTextVerticalPosition = "14",
                    HullDamageTextBold = "true",
                    HullDamageTimeout = "5",

                    // Shields Down
                    ShieldsDownImageFilename = string.Empty,
                    ShieldsDownText = "SHIELDS DOWN",
                    ShieldsDownTextColor = "#ff8800",
                    ShieldsDownTextVerticalPosition = "14",
                    ShieldsDownTextBold = "true",
                    ShieldsDownTimeout = "0",

                    // Heat Warning
                    HeatWarningImageFilename = string.Empty,
                    HeatWarningText = "HEAT WARNING",
                    HeatWarningTextColor = "#ff8800",
                    HeatWarningTextVerticalPosition = "14",
                    HeatWarningTextBold = "true",
                    HeatWarningTimeout = "0",

                    // Under Attack
                    UnderAttackImageFilename = string.Empty,
                    UnderAttackText = "UNDER ATTACK",
                    UnderAttackTextColor = "#ff0000",
                    UnderAttackTextVerticalPosition = "14",
                    UnderAttackTextBold = "true",
                    UnderAttackTimeout = "4",

                    // Being Interdicted
                    BeingInterdictedImageFilename = string.Empty,
                    BeingInterdictedText = "INTERDICTED",
                    BeingInterdictedTextColor = "#ff8800",
                    BeingInterdictedTextVerticalPosition = "14",
                    BeingInterdictedTextBold = "true",
                    BeingInterdictedTimeout = "0",

                    // Is In Danger
                    IsInDangerImageFilename = string.Empty,
                    IsInDangerText = "DANGER",
                    IsInDangerTextColor = "#ff8800",
                    IsInDangerTextVerticalPosition = "14",
                    IsInDangerTextBold = "true",
                    IsInDangerTimeout = "0",

                    // Low Fuel
                    LowFuelImageFilename = string.Empty,
                    LowFuelText = "LOW FUEL",
                    LowFuelTextColor = "#ffff00",
                    LowFuelTextVerticalPosition = "14",
                    LowFuelTextBold = "true",
                    LowFuelTimeout = "0",

                    // Docking Denied
                    DockingDeniedImageFilename = string.Empty,
                    DockingDeniedText = "DOCKING DENIED",
                    DockingDeniedTextColor = "#ff8800",
                    DockingDeniedTextVerticalPosition = "14",
                    DockingDeniedTextBold = "true",
                    DockingDeniedTimeout = "5"
                };
            }

            // Default state
            [FilenameProperty][JsonProperty(PropertyName = "defaultImage")] public string DefaultImageFilename { get; set; }
            [JsonProperty(PropertyName = "defaultText")] public string DefaultText { get; set; }
            [JsonProperty(PropertyName = "defaultTextColor")] public string DefaultTextColor { get; set; }
            [JsonProperty(PropertyName = "defaultTextVerticalPosition")] public string DefaultTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "defaultTextBold")] public string DefaultTextBold { get; set; }

            // Self Destruct
            [FilenameProperty][JsonProperty(PropertyName = "selfDestructImage")] public string SelfDestructImageFilename { get; set; }
            [JsonProperty(PropertyName = "selfDestructText")] public string SelfDestructText { get; set; }
            [JsonProperty(PropertyName = "selfDestructTextColor")] public string SelfDestructTextColor { get; set; }
            [JsonProperty(PropertyName = "selfDestructTextVerticalPosition")] public string SelfDestructTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "selfDestructTextBold")] public string SelfDestructTextBold { get; set; }
            [JsonProperty(PropertyName = "selfDestructTimeout")] public string SelfDestructTimeout { get; set; }

            // Cockpit Breached
            [FilenameProperty][JsonProperty(PropertyName = "cockpitBreachedImage")] public string CockpitBreachedImageFilename { get; set; }
            [JsonProperty(PropertyName = "cockpitBreachedText")] public string CockpitBreachedText { get; set; }
            [JsonProperty(PropertyName = "cockpitBreachedTextColor")] public string CockpitBreachedTextColor { get; set; }
            [JsonProperty(PropertyName = "cockpitBreachedTextVerticalPosition")] public string CockpitBreachedTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "cockpitBreachedTextBold")] public string CockpitBreachedTextBold { get; set; }
            [JsonProperty(PropertyName = "cockpitBreachedTimeout")] public string CockpitBreachedTimeout { get; set; }

            // Systems Shutdown
            [FilenameProperty][JsonProperty(PropertyName = "systemsShutdownImage")] public string SystemsShutdownImageFilename { get; set; }
            [JsonProperty(PropertyName = "systemsShutdownText")] public string SystemsShutdownText { get; set; }
            [JsonProperty(PropertyName = "systemsShutdownTextColor")] public string SystemsShutdownTextColor { get; set; }
            [JsonProperty(PropertyName = "systemsShutdownTextVerticalPosition")] public string SystemsShutdownTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "systemsShutdownTextBold")] public string SystemsShutdownTextBold { get; set; }
            [JsonProperty(PropertyName = "systemsShutdownTimeout")] public string SystemsShutdownTimeout { get; set; }

            // Jet Cone Damage
            [FilenameProperty][JsonProperty(PropertyName = "jetConeDamageImage")] public string JetConeDamageImageFilename { get; set; }
            [JsonProperty(PropertyName = "jetConeDamageText")] public string JetConeDamageText { get; set; }
            [JsonProperty(PropertyName = "jetConeDamageTextColor")] public string JetConeDamageTextColor { get; set; }
            [JsonProperty(PropertyName = "jetConeDamageTextVerticalPosition")] public string JetConeDamageTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "jetConeDamageTextBold")] public string JetConeDamageTextBold { get; set; }
            [JsonProperty(PropertyName = "jetConeDamageTimeout")] public string JetConeDamageTimeout { get; set; }

            // Heat Damage
            [FilenameProperty][JsonProperty(PropertyName = "heatDamageImage")] public string HeatDamageImageFilename { get; set; }
            [JsonProperty(PropertyName = "heatDamageText")] public string HeatDamageText { get; set; }
            [JsonProperty(PropertyName = "heatDamageTextColor")] public string HeatDamageTextColor { get; set; }
            [JsonProperty(PropertyName = "heatDamageTextVerticalPosition")] public string HeatDamageTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "heatDamageTextBold")] public string HeatDamageTextBold { get; set; }
            [JsonProperty(PropertyName = "heatDamageTimeout")] public string HeatDamageTimeout { get; set; }

            // Hull Damage
            [FilenameProperty][JsonProperty(PropertyName = "hullDamageImage")] public string HullDamageImageFilename { get; set; }
            [JsonProperty(PropertyName = "hullDamageText")] public string HullDamageText { get; set; }
            [JsonProperty(PropertyName = "hullDamageTextColor")] public string HullDamageTextColor { get; set; }
            [JsonProperty(PropertyName = "hullDamageTextVerticalPosition")] public string HullDamageTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "hullDamageTextBold")] public string HullDamageTextBold { get; set; }
            [JsonProperty(PropertyName = "hullDamageTimeout")] public string HullDamageTimeout { get; set; }

            // Shields Down
            [FilenameProperty][JsonProperty(PropertyName = "shieldsDownImage")] public string ShieldsDownImageFilename { get; set; }
            [JsonProperty(PropertyName = "shieldsDownText")] public string ShieldsDownText { get; set; }
            [JsonProperty(PropertyName = "shieldsDownTextColor")] public string ShieldsDownTextColor { get; set; }
            [JsonProperty(PropertyName = "shieldsDownTextVerticalPosition")] public string ShieldsDownTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "shieldsDownTextBold")] public string ShieldsDownTextBold { get; set; }
            [JsonProperty(PropertyName = "shieldsDownTimeout")] public string ShieldsDownTimeout { get; set; }

            // Heat Warning
            [FilenameProperty][JsonProperty(PropertyName = "heatWarningImage")] public string HeatWarningImageFilename { get; set; }
            [JsonProperty(PropertyName = "heatWarningText")] public string HeatWarningText { get; set; }
            [JsonProperty(PropertyName = "heatWarningTextColor")] public string HeatWarningTextColor { get; set; }
            [JsonProperty(PropertyName = "heatWarningTextVerticalPosition")] public string HeatWarningTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "heatWarningTextBold")] public string HeatWarningTextBold { get; set; }
            [JsonProperty(PropertyName = "heatWarningTimeout")] public string HeatWarningTimeout { get; set; }

            // Under Attack
            [FilenameProperty][JsonProperty(PropertyName = "underAttackImage")] public string UnderAttackImageFilename { get; set; }
            [JsonProperty(PropertyName = "underAttackText")] public string UnderAttackText { get; set; }
            [JsonProperty(PropertyName = "underAttackTextColor")] public string UnderAttackTextColor { get; set; }
            [JsonProperty(PropertyName = "underAttackTextVerticalPosition")] public string UnderAttackTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "underAttackTextBold")] public string UnderAttackTextBold { get; set; }
            [JsonProperty(PropertyName = "underAttackTimeout")] public string UnderAttackTimeout { get; set; }

            // Being Interdicted
            [FilenameProperty][JsonProperty(PropertyName = "beingInterdictedImage")] public string BeingInterdictedImageFilename { get; set; }
            [JsonProperty(PropertyName = "beingInterdictedText")] public string BeingInterdictedText { get; set; }
            [JsonProperty(PropertyName = "beingInterdictedTextColor")] public string BeingInterdictedTextColor { get; set; }
            [JsonProperty(PropertyName = "beingInterdictedTextVerticalPosition")] public string BeingInterdictedTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "beingInterdictedTextBold")] public string BeingInterdictedTextBold { get; set; }
            [JsonProperty(PropertyName = "beingInterdictedTimeout")] public string BeingInterdictedTimeout { get; set; }

            // Is In Danger
            [FilenameProperty][JsonProperty(PropertyName = "isInDangerImage")] public string IsInDangerImageFilename { get; set; }
            [JsonProperty(PropertyName = "isInDangerText")] public string IsInDangerText { get; set; }
            [JsonProperty(PropertyName = "isInDangerTextColor")] public string IsInDangerTextColor { get; set; }
            [JsonProperty(PropertyName = "isInDangerTextVerticalPosition")] public string IsInDangerTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "isInDangerTextBold")] public string IsInDangerTextBold { get; set; }
            [JsonProperty(PropertyName = "isInDangerTimeout")] public string IsInDangerTimeout { get; set; }

            // Low Fuel
            [FilenameProperty][JsonProperty(PropertyName = "lowFuelImage")] public string LowFuelImageFilename { get; set; }
            [JsonProperty(PropertyName = "lowFuelText")] public string LowFuelText { get; set; }
            [JsonProperty(PropertyName = "lowFuelTextColor")] public string LowFuelTextColor { get; set; }
            [JsonProperty(PropertyName = "lowFuelTextVerticalPosition")] public string LowFuelTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "lowFuelTextBold")] public string LowFuelTextBold { get; set; }
            [JsonProperty(PropertyName = "lowFuelTimeout")] public string LowFuelTimeout { get; set; }

            // Docking Denied
            [FilenameProperty][JsonProperty(PropertyName = "dockingDeniedImage")] public string DockingDeniedImageFilename { get; set; }
            [JsonProperty(PropertyName = "dockingDeniedText")] public string DockingDeniedText { get; set; }
            [JsonProperty(PropertyName = "dockingDeniedTextColor")] public string DockingDeniedTextColor { get; set; }
            [JsonProperty(PropertyName = "dockingDeniedTextVerticalPosition")] public string DockingDeniedTextVerticalPosition { get; set; }
            [JsonProperty(PropertyName = "dockingDeniedTextBold")] public string DockingDeniedTextBold { get; set; }
            [JsonProperty(PropertyName = "dockingDeniedTimeout")] public string DockingDeniedTimeout { get; set; }
        }

        // Cached image files
        private string _defaultFile;
        private readonly Dictionary<AlertType, string> _alertFiles = new Dictionary<AlertType, string>();

        // Alert state
        private readonly List<ActiveAlert> _activeAlerts = new List<ActiveAlert>();
        private int _cycleIndex = 0;
        private DateTime _lastCycleTime = DateTime.MinValue;
        private bool _shieldsWereDown = false;

        private PluginSettings settings;

        private int GetTimeout(string value) =>
            int.TryParse(value, out int t) ? t : 0;

        private void TriggerAlert(AlertType type, int timeoutSeconds)
        {
            lock (_activeAlerts)
            {
                // Remove existing alert of same type
                _activeAlerts.RemoveAll(a => a.Type == type);

                var expiry = timeoutSeconds > 0
                    ? DateTime.Now.AddSeconds(timeoutSeconds)
                    : DateTime.MaxValue;

                _activeAlerts.Add(new ActiveAlert { Type = type, ExpiresAt = expiry });

                // Sort by priority
                _activeAlerts.Sort((a, b) => a.Type.CompareTo(b.Type));
            }
        }

        private void ClearAlert(AlertType type)
        {
            lock (_activeAlerts)
            {
                _activeAlerts.RemoveAll(a => a.Type == type);
            }
        }

        private void PruneExpiredAlerts()
        {
            lock (_activeAlerts)
            {
                _activeAlerts.RemoveAll(a => a.ExpiresAt <= DateTime.Now);
            }
        }

        private void UpdateStatusAlerts()
        {
            var s = EliteData.StatusData;

            // Status-based alerts - trigger or clear based on current flags
            if (s.Overheating)
                TriggerAlert(AlertType.HeatWarning, 0);
            else
            {
                ClearAlert(AlertType.HeatWarning);
                ClearAlert(AlertType.HeatDamage);
            }

            if (s.BeingInterdicted)
                TriggerAlert(AlertType.BeingInterdicted, 0);
            else
                ClearAlert(AlertType.BeingInterdicted);

            if (s.IsInDanger)
                TriggerAlert(AlertType.IsInDanger, 0);
            else
                ClearAlert(AlertType.IsInDanger);

            if (s.LowFuel)
                TriggerAlert(AlertType.LowFuel, 0);
            else
                ClearAlert(AlertType.LowFuel);
        }

        private void DrawText(Graphics graphics, string text, Color color, double verticalPosition, bool bold, int width)
        {
            if (string.IsNullOrEmpty(text)) return;

            var fontStyle = bold ? FontStyle.Bold : FontStyle.Regular;
            // Handle newlines from textarea (real \n or \r\n) and literal \n typed by user
            var lines = text.Replace("\r\n", "\n").Replace("\\n", "\n").Split('\n');
            var brush = new SolidBrush(color);

            for (int adjustedSize = 25; adjustedSize >= 10; adjustedSize -= 1)
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

        private (string text, Color color, double vertPos, bool bold, string imageFile) GetAlertDisplay(AlertType type)
        {
            var converter = new ColorConverter();
            Color ParseColor(string hex) => (Color)converter.ConvertFromString(hex);

            switch (type)
            {
                case AlertType.SelfDestruct:
                    return (settings.SelfDestructText, ParseColor(settings.SelfDestructTextColor),
                        double.TryParse(settings.SelfDestructTextVerticalPosition, out double sd) ? sd : 14,
                        settings.SelfDestructTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.CockpitBreached:
                    return (settings.CockpitBreachedText, ParseColor(settings.CockpitBreachedTextColor),
                        double.TryParse(settings.CockpitBreachedTextVerticalPosition, out double cb) ? cb : 14,
                        settings.CockpitBreachedTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.SystemsShutdown:
                    return (settings.SystemsShutdownText, ParseColor(settings.SystemsShutdownTextColor),
                        double.TryParse(settings.SystemsShutdownTextVerticalPosition, out double ss) ? ss : 14,
                        settings.SystemsShutdownTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.JetConeDamage:
                    return (settings.JetConeDamageText, ParseColor(settings.JetConeDamageTextColor),
                        double.TryParse(settings.JetConeDamageTextVerticalPosition, out double jcd) ? jcd : 14,
                        settings.JetConeDamageTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.HeatDamage:
                    return (settings.HeatDamageText, ParseColor(settings.HeatDamageTextColor),
                        double.TryParse(settings.HeatDamageTextVerticalPosition, out double hd) ? hd : 14,
                        settings.HeatDamageTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.HullDamage:
                    return (settings.HullDamageText, ParseColor(settings.HullDamageTextColor),
                        double.TryParse(settings.HullDamageTextVerticalPosition, out double hud) ? hud : 14,
                        settings.HullDamageTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.ShieldsDown:
                    return (settings.ShieldsDownText, ParseColor(settings.ShieldsDownTextColor),
                        double.TryParse(settings.ShieldsDownTextVerticalPosition, out double sdo) ? sdo : 14,
                        settings.ShieldsDownTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.HeatWarning:
                    return (settings.HeatWarningText, ParseColor(settings.HeatWarningTextColor),
                        double.TryParse(settings.HeatWarningTextVerticalPosition, out double hw) ? hw : 14,
                        settings.HeatWarningTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.UnderAttack:
                    return (settings.UnderAttackText, ParseColor(settings.UnderAttackTextColor),
                        double.TryParse(settings.UnderAttackTextVerticalPosition, out double ua) ? ua : 14,
                        settings.UnderAttackTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.BeingInterdicted:
                    return (settings.BeingInterdictedText, ParseColor(settings.BeingInterdictedTextColor),
                        double.TryParse(settings.BeingInterdictedTextVerticalPosition, out double bi) ? bi : 14,
                        settings.BeingInterdictedTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.IsInDanger:
                    return (settings.IsInDangerText, ParseColor(settings.IsInDangerTextColor),
                        double.TryParse(settings.IsInDangerTextVerticalPosition, out double iid) ? iid : 14,
                        settings.IsInDangerTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.LowFuel:
                    return (settings.LowFuelText, ParseColor(settings.LowFuelTextColor),
                        double.TryParse(settings.LowFuelTextVerticalPosition, out double lf) ? lf : 14,
                        settings.LowFuelTextBold == "true", _alertFiles.GetValueOrDefault(type));
                case AlertType.DockingDenied:
                    return (settings.DockingDeniedText, ParseColor(settings.DockingDeniedTextColor),
                        double.TryParse(settings.DockingDeniedTextVerticalPosition, out double dd) ? dd : 14,
                        settings.DockingDeniedTextBold == "true", _alertFiles.GetValueOrDefault(type));
                default:
                    return (string.Empty, Color.White, 14, true, null);
            }
        }

        private async Task HandleDisplay()
        {
            UpdateStatusAlerts();
            PruneExpiredAlerts();

            List<ActiveAlert> currentAlerts;
            lock (_activeAlerts)
            {
                currentAlerts = _activeAlerts.ToList();
            }

            string imgBase64 = _defaultFile;
            string text = settings.DefaultText;
            var converter = new ColorConverter();
            Color textColor = string.IsNullOrEmpty(settings.DefaultTextColor) ? Color.White : (Color)converter.ConvertFromString(settings.DefaultTextColor);
            double vertPos = double.TryParse(settings.DefaultTextVerticalPosition, out double dvp) ? dvp : 80;
            bool bold = settings.DefaultTextBold == "true";

            if (currentAlerts.Count > 0)
            {
                // Cycle through active alerts every 2 seconds
                if ((DateTime.Now - _lastCycleTime).TotalSeconds >= 2.0)
                {
                    _cycleIndex = (_cycleIndex + 1) % currentAlerts.Count;
                    _lastCycleTime = DateTime.Now;
                }

                if (_cycleIndex >= currentAlerts.Count)
                    _cycleIndex = 0;

                var alert = currentAlerts[_cycleIndex];
                var (alertText, alertColor, alertVertPos, alertBold, alertFile) = GetAlertDisplay(alert.Type);

                imgBase64 = alertFile ?? _defaultFile;
                text = alertText;
                textColor = alertColor;
                vertPos = alertVertPos;
                bold = alertBold;
            }

            if (imgBase64 == null) return;

            try
            {
                // Try to draw on bitmap if we have an image
                Bitmap sourceBitmap = null;
                foreach (var kvp in _alertBitmaps)
                {
                    if (_alertFiles.TryGetValue(kvp.Key, out var f) && f == imgBase64)
                    {
                        sourceBitmap = kvp.Value;
                        break;
                    }
                }
                if (sourceBitmap == null && imgBase64 == _defaultFile)
                    sourceBitmap = _defaultBitmap;

                if (sourceBitmap != null && !string.IsNullOrEmpty(text))
                {
                    using (var bitmap = new Bitmap(sourceBitmap))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        DrawText(graphics, text, textColor, vertPos, bold, bitmap.Width);
                        imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "Alert HandleDisplay " + ex);
            }

            await Connection.SetImageAsync(imgBase64);
        }

        // Cached bitmaps
        private Bitmap _defaultBitmap;
        private readonly Dictionary<AlertType, Bitmap> _alertBitmaps = new Dictionary<AlertType, Bitmap>();

        public Alert(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            var evt = ((JournalEventArgs)e).OriginalEvent.Value<string>("event");
            if (string.IsNullOrWhiteSpace(evt))
            {
                AsyncHelper.RunSync(HandleDisplay);
                return;
            }

            switch (evt)
            {
                case "SelfDestruct":
                    TriggerAlert(AlertType.SelfDestruct, GetTimeout(settings.SelfDestructTimeout));
                    break;
                case "CockpitBreached":
                    TriggerAlert(AlertType.CockpitBreached, GetTimeout(settings.CockpitBreachedTimeout));
                    break;
                case "SystemsShutdown":
                    TriggerAlert(AlertType.SystemsShutdown, GetTimeout(settings.SystemsShutdownTimeout));
                    break;
                case "JetConeDamage":
                    TriggerAlert(AlertType.JetConeDamage, GetTimeout(settings.JetConeDamageTimeout));
                    break;
                case "HeatDamage":
                    TriggerAlert(AlertType.HeatDamage, GetTimeout(settings.HeatDamageTimeout));
                    break;
                case "HullDamage":
                    TriggerAlert(AlertType.HullDamage, GetTimeout(settings.HullDamageTimeout));
                    break;
                case "ShieldState":
                    var shieldsUp = e.OriginalEvent.Value<bool?>("ShieldsUp") ?? true;
                    if (!shieldsUp)
                    {
                        _shieldsWereDown = true;
                        TriggerAlert(AlertType.ShieldsDown, 0);
                    }
                    else if (_shieldsWereDown)
                    {
                        _shieldsWereDown = false;
                        ClearAlert(AlertType.ShieldsDown);
                    }
                    break;
                case "UnderAttack":
                    TriggerAlert(AlertType.UnderAttack, GetTimeout(settings.UnderAttackTimeout));
                    break;
                case "DockingDenied":
                case "DockingTimeout":
                    TriggerAlert(AlertType.DockingDenied, GetTimeout(settings.DockingDeniedTimeout));
                    break;
            }

            AsyncHelper.RunSync(HandleDisplay);
        }

        public override void KeyPressed(KeyPayload payload)
        {
            // Clear all alerts on button press
            lock (_activeAlerts)
            {
                _activeAlerts.Clear();
            }
            AsyncHelper.RunSync(HandleDisplay);
        }

        public override void KeyReleased(KeyPayload payload) { }

        public override void Dispose()
        {
            base.Dispose();
            Program.JournalWatcher.AllEventHandler -= HandleEliteEvents;
            _defaultBitmap?.Dispose();
            foreach (var bmp in _alertBitmaps.Values)
                bmp?.Dispose();
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

        private void LoadImage(AlertType type, string filename)
        {
            if (_alertBitmaps.ContainsKey(type))
            {
                _alertBitmaps[type]?.Dispose();
                _alertBitmaps.Remove(type);
                _alertFiles.Remove(type);
            }

            if (File.Exists(filename))
            {
                _alertBitmaps[type] = (Bitmap)Image.FromFile(filename);
                _alertFiles[type] = Tools.FileToBase64(filename, true);
            }
        }

        private void InitializeSettings()
        {
            // Default image
            _defaultBitmap?.Dispose();
            _defaultBitmap = null;
            _defaultFile = null;

            if (File.Exists(settings.DefaultImageFilename))
            {
                _defaultBitmap = (Bitmap)Image.FromFile(settings.DefaultImageFilename);
                _defaultFile = Tools.FileToBase64(settings.DefaultImageFilename, true);
            }

            // Alert images
            LoadImage(AlertType.SelfDestruct, settings.SelfDestructImageFilename);
            LoadImage(AlertType.CockpitBreached, settings.CockpitBreachedImageFilename);
            LoadImage(AlertType.SystemsShutdown, settings.SystemsShutdownImageFilename);
            LoadImage(AlertType.JetConeDamage, settings.JetConeDamageImageFilename);
            LoadImage(AlertType.HeatDamage, settings.HeatDamageImageFilename);
            LoadImage(AlertType.HullDamage, settings.HullDamageImageFilename);
            LoadImage(AlertType.ShieldsDown, settings.ShieldsDownImageFilename);
            LoadImage(AlertType.HeatWarning, settings.HeatWarningImageFilename);
            LoadImage(AlertType.UnderAttack, settings.UnderAttackImageFilename);
            LoadImage(AlertType.BeingInterdicted, settings.BeingInterdictedImageFilename);
            LoadImage(AlertType.IsInDanger, settings.IsInDangerImageFilename);
            LoadImage(AlertType.LowFuel, settings.LowFuelImageFilename);
            LoadImage(AlertType.DockingDenied, settings.DockingDeniedImageFilename);

            // Null checks for settings
            if (string.IsNullOrEmpty(settings.DefaultTextColor)) settings.DefaultTextColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.DefaultTextVerticalPosition)) settings.DefaultTextVerticalPosition = "80";
            if (string.IsNullOrEmpty(settings.DefaultTextBold)) settings.DefaultTextBold = "true";

            if (string.IsNullOrEmpty(settings.SelfDestructTextColor)) settings.SelfDestructTextColor = "#ff0000";
            if (string.IsNullOrEmpty(settings.SelfDestructTextVerticalPosition)) settings.SelfDestructTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.SelfDestructTextBold)) settings.SelfDestructTextBold = "true";
            if (string.IsNullOrEmpty(settings.SelfDestructTimeout)) settings.SelfDestructTimeout = "30";

            if (string.IsNullOrEmpty(settings.CockpitBreachedTextColor)) settings.CockpitBreachedTextColor = "#ff4400";
            if (string.IsNullOrEmpty(settings.CockpitBreachedTextVerticalPosition)) settings.CockpitBreachedTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.CockpitBreachedTextBold)) settings.CockpitBreachedTextBold = "true";
            if (string.IsNullOrEmpty(settings.CockpitBreachedTimeout)) settings.CockpitBreachedTimeout = "20";

            if (string.IsNullOrEmpty(settings.SystemsShutdownTextColor)) settings.SystemsShutdownTextColor = "#ff4400";
            if (string.IsNullOrEmpty(settings.SystemsShutdownTextVerticalPosition)) settings.SystemsShutdownTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.SystemsShutdownTextBold)) settings.SystemsShutdownTextBold = "true";
            if (string.IsNullOrEmpty(settings.SystemsShutdownTimeout)) settings.SystemsShutdownTimeout = "10";

            if (string.IsNullOrEmpty(settings.JetConeDamageTextColor)) settings.JetConeDamageTextColor = "#ff8800";
            if (string.IsNullOrEmpty(settings.JetConeDamageTextVerticalPosition)) settings.JetConeDamageTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.JetConeDamageTextBold)) settings.JetConeDamageTextBold = "true";
            if (string.IsNullOrEmpty(settings.JetConeDamageTimeout)) settings.JetConeDamageTimeout = "5";

            if (string.IsNullOrEmpty(settings.HeatDamageTextColor)) settings.HeatDamageTextColor = "#ff4400";
            if (string.IsNullOrEmpty(settings.HeatDamageTextVerticalPosition)) settings.HeatDamageTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.HeatDamageTextBold)) settings.HeatDamageTextBold = "true";
            if (string.IsNullOrEmpty(settings.HeatDamageTimeout)) settings.HeatDamageTimeout = "5";

            if (string.IsNullOrEmpty(settings.HullDamageTextColor)) settings.HullDamageTextColor = "#ff0000";
            if (string.IsNullOrEmpty(settings.HullDamageTextVerticalPosition)) settings.HullDamageTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.HullDamageTextBold)) settings.HullDamageTextBold = "true";
            if (string.IsNullOrEmpty(settings.HullDamageTimeout)) settings.HullDamageTimeout = "5";

            if (string.IsNullOrEmpty(settings.ShieldsDownTextColor)) settings.ShieldsDownTextColor = "#ff8800";
            if (string.IsNullOrEmpty(settings.ShieldsDownTextVerticalPosition)) settings.ShieldsDownTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.ShieldsDownTextBold)) settings.ShieldsDownTextBold = "true";
            if (string.IsNullOrEmpty(settings.ShieldsDownTimeout)) settings.ShieldsDownTimeout = "0";

            if (string.IsNullOrEmpty(settings.HeatWarningTextColor)) settings.HeatWarningTextColor = "#ff8800";
            if (string.IsNullOrEmpty(settings.HeatWarningTextVerticalPosition)) settings.HeatWarningTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.HeatWarningTextBold)) settings.HeatWarningTextBold = "true";
            if (string.IsNullOrEmpty(settings.HeatWarningTimeout)) settings.HeatWarningTimeout = "0";

            if (string.IsNullOrEmpty(settings.UnderAttackTextColor)) settings.UnderAttackTextColor = "#ff0000";
            if (string.IsNullOrEmpty(settings.UnderAttackTextVerticalPosition)) settings.UnderAttackTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.UnderAttackTextBold)) settings.UnderAttackTextBold = "true";
            if (string.IsNullOrEmpty(settings.UnderAttackTimeout)) settings.UnderAttackTimeout = "4";

            if (string.IsNullOrEmpty(settings.BeingInterdictedTextColor)) settings.BeingInterdictedTextColor = "#ff8800";
            if (string.IsNullOrEmpty(settings.BeingInterdictedTextVerticalPosition)) settings.BeingInterdictedTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.BeingInterdictedTextBold)) settings.BeingInterdictedTextBold = "true";
            if (string.IsNullOrEmpty(settings.BeingInterdictedTimeout)) settings.BeingInterdictedTimeout = "0";

            if (string.IsNullOrEmpty(settings.IsInDangerTextColor)) settings.IsInDangerTextColor = "#ff8800";
            if (string.IsNullOrEmpty(settings.IsInDangerTextVerticalPosition)) settings.IsInDangerTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.IsInDangerTextBold)) settings.IsInDangerTextBold = "true";
            if (string.IsNullOrEmpty(settings.IsInDangerTimeout)) settings.IsInDangerTimeout = "0";

            if (string.IsNullOrEmpty(settings.LowFuelTextColor)) settings.LowFuelTextColor = "#ffff00";
            if (string.IsNullOrEmpty(settings.LowFuelTextVerticalPosition)) settings.LowFuelTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.LowFuelTextBold)) settings.LowFuelTextBold = "true";
            if (string.IsNullOrEmpty(settings.LowFuelTimeout)) settings.LowFuelTimeout = "0";

            if (string.IsNullOrEmpty(settings.DockingDeniedTextColor)) settings.DockingDeniedTextColor = "#ff8800";
            if (string.IsNullOrEmpty(settings.DockingDeniedTextVerticalPosition)) settings.DockingDeniedTextVerticalPosition = "14";
            if (string.IsNullOrEmpty(settings.DockingDeniedTextBold)) settings.DockingDeniedTextBold = "true";
            if (string.IsNullOrEmpty(settings.DockingDeniedTimeout)) settings.DockingDeniedTimeout = "5";

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}
