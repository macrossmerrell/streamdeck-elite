using System;
using System.IO;
using System.Threading.Tasks;
using BarRaider.SdTools;
using EliteJournalReader;
using EliteJournalReader.Events;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

// ReSharper disable StringLiteralTypo

namespace Elite.Buttons
{
    // Ship/Commander status states - ordered by priority (higher = checked first)
    public enum ShipStatusState
    {
        Unknown = 0,
        Landed,
        Liftoff,
        OnFootInStation,
        DockedAtStation,
        StationInterior,
        PlanetFlight,
        Deorbiting,
        OrbitalCruise,
        LeavingPlanet,
        PlanetApproach,
        StationApproach,
        NoFireZone,
        NormalSpace,
        FuelScooping,
        Supercruise,
        HyperspaceCharging,
        SupercruiseCharging,
        FsdSupercruiseJumping,
        FsdHyperspaceJumping
    }

    [PluginActionId("com.mhwlng.elite.shipstatus")]
    public class ShipStatus : EliteKeypadBase
    {
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    FsdHyperspaceJumpingImageFilename = string.Empty,
                    HyperspaceChargingImageFilename = string.Empty,
                    SupercruiseChargingImageFilename = string.Empty,
                    FsdSupercruiseJumpingImageFilename = string.Empty,
                    FuelScoopingImageFilename = string.Empty,
                    SupercruiseImageFilename = string.Empty,
                    NormalSpaceImageFilename = string.Empty,
                    NoFireZoneImageFilename = string.Empty,
                    StationApproachImageFilename = string.Empty,
                    DockedAtStationImageFilename = string.Empty,
                    StationInteriorImageFilename = string.Empty,
                    PlanetApproachImageFilename = string.Empty,
                    OrbitalCruiseImageFilename = string.Empty,
                    DeorbitingImageFilename = string.Empty,
                    PlanetFlightImageFilename = string.Empty,
                    LandedImageFilename = string.Empty,
                    LiftoffImageFilename = string.Empty,
                    LeavingPlanetImageFilename = string.Empty
                };
                return instance;
            }

            [FilenameProperty]
            [JsonProperty(PropertyName = "fsdHyperspaceJumpingImage")]
            public string FsdHyperspaceJumpingImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "hyperspaceChargingImage")]
            public string HyperspaceChargingImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "supercruiseChargingImage")]
            public string SupercruiseChargingImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "fsdSupercruiseJumpingImage")]
            public string FsdSupercruiseJumpingImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "fuelScoopingImage")]
            public string FuelScoopingImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "supercruiseImage")]
            public string SupercruiseImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "normalSpaceImage")]
            public string NormalSpaceImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "noFireZoneImage")]
            public string NoFireZoneImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "stationApproachImage")]
            public string StationApproachImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "dockedAtStationImage")]
            public string DockedAtStationImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "stationInteriorImage")]
            public string StationInteriorImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "planetApproachImage")]
            public string PlanetApproachImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "orbitalCruiseImage")]
            public string OrbitalCruiseImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "deorbitingImage")]
            public string DeorbitingImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "planetFlightImage")]
            public string PlanetFlightImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "landedImage")]
            public string LandedImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "liftoffImage")]
            public string LiftoffImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "leavingPlanetImage")]
            public string LeavingPlanetImageFilename { get; set; }
        }

        private PluginSettings settings;

        // Cached base64 image strings
        private string _fsdHyperspaceJumpingFile;
        private string _hyperspaceChargingFile;
        private string _supercruiseChargingFile;
        private string _fsdSupercruiseJumpingFile;
        private string _fuelScoopingFile;
        private string _supercruiseFile;
        private string _normalSpaceFile;
        private string _noFireZoneFile;
        private string _stationApproachFile;
        private string _dockedAtStationFile;
        private string _stationInteriorFile;
        private string _planetApproachFile;
        private string _orbitalCruiseFile;
        private string _deorbitingFile;
        private string _planetFlightFile;
        private string _landedFile;
        private string _liftoffFile;
        private string _leavingPlanetFile;
        private bool _hadLatLong = false;
        private bool _climbing = false;
        private int _fallingTicks = 0;

        // State tracking from journal events
        private bool _dockingGranted = false;
        private bool _approachingBody = false;
        private bool _inNoFireZone = false;
        private bool _isHyperspaceJump = false;
        private double _previousAltitude = 0;
        private DateTime _liftoffTime = DateTime.MinValue;
        private bool _deorbiting = false;
        private bool _hadGlide = false;
        private bool _liftedOff = false;
        private bool _leavingPlanet = false;
        private bool _hadLeaveBody = false;

        private ShipStatusState DetermineState()
        {
            var s = EliteData.StatusData;

            // FSD Jump in progress - highest priority
            if (s.FsdJump)
                return _isHyperspaceJump
                    ? ShipStatusState.FsdHyperspaceJumping
                    : ShipStatusState.FsdSupercruiseJumping;

            // Hyperspace FSD charging
            if (s.Fsdhyperdrivecharging)
                return ShipStatusState.HyperspaceCharging;

            // Supercruise charging - FsdCharging set but not hyperspace
            if (s.FsdCharging && !s.Fsdhyperdrivecharging)
                return ShipStatusState.SupercruiseCharging;

            // On foot inside a space station
            if (s.OnFootInStation)
                return ShipStatusState.OnFootInStation;

            // Docked at station - check for interior (past mail slot)
            if (s.Docked)
            {
                if (s.OnFoot)
                    return ShipStatusState.StationInterior;
                return ShipStatusState.DockedAtStation;
            }

            // Landed on a planet surface
            if (s.Landed)
                return ShipStatusState.Landed;

            // Liftoff - show for 2.5 seconds after liftoff event
            if ((DateTime.Now - _liftoffTime).TotalSeconds <= 2.5)
                return ShipStatusState.Liftoff;

            // Fuel scooping
            if (s.ScoopingFuel)
                return ShipStatusState.FuelScooping;

            // In supercruise
            if (s.Supercruise)
            {
                if (s.HasLatLong)
                {
                    // GlideMode or _deorbiting flag - catch deorbit even while Supercruise still set in status
                    if (s.GlideMode || _deorbiting)
                    {
                        _deorbiting = true;
                        _hadGlide = s.GlideMode;
                        return ShipStatusState.Deorbiting;
                    }

                    if (!_hadLatLong)
                    {
                        _previousAltitude = s.Altitude;
                        _hadLatLong = true;
                        if (!_climbing)
                            return ShipStatusState.PlanetApproach;
                    }

                    bool altitudeRising = s.Altitude > _previousAltitude;
                    _previousAltitude = s.Altitude;

                    if (_approachingBody)
                        return ShipStatusState.OrbitalCruise;

                    if (altitudeRising)
                    {
                        _climbing = true;
                        _fallingTicks = 0;
                    }
                    else if (_climbing)
                    {
                        _fallingTicks++;
                        if (_fallingTicks >= 3)
                        {
                            _climbing = false;
                            _fallingTicks = 0;
                        }
                    }

                    // Leaving planet - show OrbitalCruise while climbing after liftoff
                    // then LeavingPlanet after LeaveBody fires
                    if (_leavingPlanet)
                    {
                        // If altitude starts decreasing, user turned around - treat as approach
                        if (!_climbing)
                        {
                            _leavingPlanet = false;
                            // Keep _liftedOff so we can resume LeavingPlanet if they climb again
                            return ShipStatusState.PlanetApproach;
                        }
                        return ShipStatusState.LeavingPlanet;
                    }

                    // LeaveBody has fired and we turned around then climbed again - restore LeavingPlanet
                    if (_climbing && _liftedOff && _hadLeaveBody)
                    {
                        _leavingPlanet = true;
                        return ShipStatusState.LeavingPlanet;
                    }

                    if (_climbing && _liftedOff)
                        return ShipStatusState.OrbitalCruise;

                    if (_climbing)
                        return ShipStatusState.Supercruise;

                    return ShipStatusState.PlanetApproach;
                }
                else
                {
                    _previousAltitude = 0;
                    _hadLatLong = false;
                    _leavingPlanet = false;
                    _liftedOff = false;
                    _hadLeaveBody = false;
                    _climbing = false;
                    _fallingTicks = 0;
                }

                return ShipStatusState.Supercruise;
            }
            // Planet-related states (HasLatLong means we have planetary coordinates)
            if (s.HasLatLong)
            {
                // Track that we have lat/long so SupercruiseEntry knows we're departing a planet
                _hadLatLong = true;

                if (s.GlideMode)
                {
                    _deorbiting = true;
                    _hadGlide = true;
                    return ShipStatusState.Deorbiting;
                }

                // GlideMode just ended - clear deorbiting and fall through to PlanetFlight
                if (_deorbiting && _hadGlide)
                {
                    _deorbiting = false;
                    _hadGlide = false;
                }
                else if (_deorbiting)
                {
                    // Still in deorbit before glide started
                    return ShipStatusState.Deorbiting;
                }

                if (_approachingBody)
                    return ShipStatusState.OrbitalCruise;

                return ShipStatusState.PlanetFlight;
            }

            // Normal space states (not supercruise, not on planet)
            if (_inNoFireZone)
                return ShipStatusState.NoFireZone;

            if (_dockingGranted)
                return ShipStatusState.StationApproach;

            return ShipStatusState.NormalSpace;
        }

        private async Task HandleDisplay()
        {
            var state = DetermineState();

            string imgBase64 = null;

            switch (state)
            {
                case ShipStatusState.FsdHyperspaceJumping:
                    imgBase64 = _fsdHyperspaceJumpingFile;
                    break;
                case ShipStatusState.HyperspaceCharging:
                    imgBase64 = _hyperspaceChargingFile ?? _fsdHyperspaceJumpingFile;
                    break;
                case ShipStatusState.SupercruiseCharging:
                    imgBase64 = _supercruiseChargingFile ?? _fsdSupercruiseJumpingFile;
                    break;
                case ShipStatusState.FsdSupercruiseJumping:
                    imgBase64 = _fsdSupercruiseJumpingFile;
                    break;
                case ShipStatusState.FuelScooping:
                    imgBase64 = _fuelScoopingFile;
                    break;
                case ShipStatusState.Supercruise:
                    imgBase64 = _supercruiseFile;
                    break;
                case ShipStatusState.NormalSpace:
                    imgBase64 = _normalSpaceFile;
                    break;
                case ShipStatusState.NoFireZone:
                    imgBase64 = _noFireZoneFile;
                    break;
                case ShipStatusState.StationApproach:
                    imgBase64 = _stationApproachFile;
                    break;
                case ShipStatusState.DockedAtStation:
                    imgBase64 = _dockedAtStationFile;
                    break;
                case ShipStatusState.StationInterior:
                    imgBase64 = _stationInteriorFile ?? _dockedAtStationFile;
                    break;
                case ShipStatusState.PlanetApproach:
                    imgBase64 = _planetApproachFile;
                    break;
                case ShipStatusState.OrbitalCruise:
                    imgBase64 = _orbitalCruiseFile ?? _planetApproachFile;
                    break;
                case ShipStatusState.LeavingPlanet:
                    imgBase64 = _leavingPlanetFile ?? _supercruiseFile;
                    break;
                case ShipStatusState.Deorbiting:
                    imgBase64 = _deorbitingFile ?? _planetFlightFile;
                    break;
                case ShipStatusState.PlanetFlight:
                    imgBase64 = _planetFlightFile;
                    break;
                case ShipStatusState.Landed:
                    imgBase64 = _landedFile;
                    break;
                case ShipStatusState.Liftoff:
                    imgBase64 = _liftoffFile ?? _planetFlightFile;
                    break;
                case ShipStatusState.OnFootInStation:
                    imgBase64 = _stationInteriorFile ?? _dockedAtStationFile;
                    break;
            }

            if (!string.IsNullOrEmpty(imgBase64))
            {
                await Connection.SetImageAsync(imgBase64);
            }
        }

        public ShipStatus(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
                case "StartJump":
                {
                    var jumpType = e.OriginalEvent.Value<string>("JumpType");
                    _isHyperspaceJump = jumpType == "Hyperspace";
                    break;
                }

                case "FSDJump":
                    _isHyperspaceJump = false;
                    _approachingBody = false;
                    _climbing = false;
                    _fallingTicks = 0;
                    _hadLatLong = false;
                    _deorbiting = false;
                    _hadGlide = false;
                    _inNoFireZone = false;
                    _dockingGranted = false;
                    _liftoffTime = DateTime.MinValue;
                    _liftedOff = false;
                    _leavingPlanet = false;
                    _hadLeaveBody = false;
                    break;

                case "SupercruiseEntry":
                    _isHyperspaceJump = false;
                    _approachingBody = false;
                    _climbing = true;
                    _fallingTicks = 0;
                    // Capture _hadLatLong BEFORE clearing it
                    _liftedOff = _hadLatLong;
                    _hadLatLong = false;
                    _deorbiting = false;
                    _hadGlide = false;
                    _inNoFireZone = false;
                    _dockingGranted = false;
                    _liftoffTime = DateTime.MinValue;
                    _leavingPlanet = false;
                    break;

                case "SupercruiseExit":
                    _inNoFireZone = false;
                    _approachingBody = false;
                    if (e.OriginalEvent.Value<string>("BodyType") == "Planet")
                        _deorbiting = true;
                    break;

                case "ApproachBody":
                    _approachingBody = true;
                    _climbing = false;
                    _fallingTicks = 0;
                    _dockingGranted = false;
                    _liftedOff = true;
                    _hadLeaveBody = false;
                    break;

                case "LeaveBody":
                    _approachingBody = false;
                    _climbing = true;
                    _hadLeaveBody = true;
                    if (_liftedOff)
                        _leavingPlanet = true;
                    break;

                case "Touchdown":
                    _approachingBody = false;
                    _liftedOff = false;
                    _leavingPlanet = false;
                    break;

                case "Liftoff":
                    _approachingBody = false;
                    _deorbiting = false;
                    _hadGlide = false;
                    _liftoffTime = DateTime.Now;
                    _liftedOff = true;
                    _leavingPlanet = false;
                    break;

                case "DockingGranted":
                    _dockingGranted = true;
                    _inNoFireZone = true;
                    break;

                case "DockingCancelled":
                case "DockingDenied":
                case "DockingTimeout":
                    _dockingGranted = false;
                    _inNoFireZone = false;
                    break;

                case "Docked":
                    _dockingGranted = false;
                    _inNoFireZone = false;
                    break;

                case "Undocked":
                    _inNoFireZone = true;
                    break;
            }

            AsyncHelper.RunSync(HandleDisplay);
        }

        public override void KeyPressed(KeyPayload payload)
        {
            // Display only - no action on press
        }

        public override void KeyReleased(KeyPayload payload)
        {
        }

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
            _fsdHyperspaceJumpingFile = Tools.FileToBase64(settings.FsdHyperspaceJumpingImageFilename, true);
            _hyperspaceChargingFile = Tools.FileToBase64(settings.HyperspaceChargingImageFilename, true);
            _supercruiseChargingFile = Tools.FileToBase64(settings.SupercruiseChargingImageFilename, true);
            _fsdSupercruiseJumpingFile = Tools.FileToBase64(settings.FsdSupercruiseJumpingImageFilename, true);
            _fuelScoopingFile = Tools.FileToBase64(settings.FuelScoopingImageFilename, true);
            _supercruiseFile = Tools.FileToBase64(settings.SupercruiseImageFilename, true);
            _normalSpaceFile = Tools.FileToBase64(settings.NormalSpaceImageFilename, true);
            _noFireZoneFile = Tools.FileToBase64(settings.NoFireZoneImageFilename, true);
            _stationApproachFile = Tools.FileToBase64(settings.StationApproachImageFilename, true);
            _dockedAtStationFile = Tools.FileToBase64(settings.DockedAtStationImageFilename, true);
            _stationInteriorFile = Tools.FileToBase64(settings.StationInteriorImageFilename, true);
            _planetApproachFile = Tools.FileToBase64(settings.PlanetApproachImageFilename, true);
            _orbitalCruiseFile = Tools.FileToBase64(settings.OrbitalCruiseImageFilename, true);
            _deorbitingFile = Tools.FileToBase64(settings.DeorbitingImageFilename, true);
            _planetFlightFile = Tools.FileToBase64(settings.PlanetFlightImageFilename, true);
            _landedFile = Tools.FileToBase64(settings.LandedImageFilename, true);
            _liftoffFile = Tools.FileToBase64(settings.LiftoffImageFilename, true);
            _leavingPlanetFile = Tools.FileToBase64(settings.LeavingPlanetImageFilename, true);

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}
