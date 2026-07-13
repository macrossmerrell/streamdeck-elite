using System;
using System.Drawing;
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
    public enum OdysseyShipStatusState
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
        SupercruiseOvercharge,
        SupercruiseAssist,
        HyperspaceCharging,
        SupercruiseCharging,
        FsdSupercruiseJumping,
        FsdHyperspaceJumping
    }

    [PluginActionId("com.mhwlng.elite.odysseyshipstatus")]
    public class OdysseyShipStatus : EliteKeypadBase
    {
        protected class PluginSettings
        {
            public static PluginSettings CreateDefaultSettings()
            {
                var instance = new PluginSettings
                {
                    NotActiveImageFilename = string.Empty,

                    SupercruiseChargingImageFilename = string.Empty,
                    SupercruiseChargingTopText = string.Empty,
                    SupercruiseChargingTopColor = "#ffffff",
                    SupercruiseChargingTopTextVerticalPosition = "5",
                    SupercruiseChargingBottomText = string.Empty,
                    SupercruiseChargingBottomColor = "#ffffff",
                    SupercruiseChargingBottomTextVerticalPosition = "190",
                    SupercruiseChargingBold = "true",

                    FsdSupercruiseJumpingImageFilename = string.Empty,
                    FsdSupercruiseJumpingTopText = string.Empty,
                    FsdSupercruiseJumpingTopColor = "#ffffff",
                    FsdSupercruiseJumpingTopTextVerticalPosition = "5",
                    FsdSupercruiseJumpingBottomText = string.Empty,
                    FsdSupercruiseJumpingBottomColor = "#ffffff",
                    FsdSupercruiseJumpingBottomTextVerticalPosition = "190",
                    FsdSupercruiseJumpingBold = "true",

                    SupercruiseImageFilename = string.Empty,
                    SupercruiseTopText = string.Empty,
                    SupercruiseTopColor = "#ffffff",
                    SupercruiseTopTextVerticalPosition = "5",
                    SupercruiseBottomText = string.Empty,
                    SupercruiseBottomColor = "#ffffff",
                    SupercruiseBottomTextVerticalPosition = "190",
                    SupercruiseBold = "true",

                    SupercruiseOverchargeImageFilename = string.Empty,
                    SupercruiseOverchargeTopText = string.Empty,
                    SupercruiseOverchargeTopColor = "#ffffff",
                    SupercruiseOverchargeTopTextVerticalPosition = "5",
                    SupercruiseOverchargeBottomText = string.Empty,
                    SupercruiseOverchargeBottomColor = "#ffffff",
                    SupercruiseOverchargeBottomTextVerticalPosition = "190",
                    SupercruiseOverchargeBold = "true",

                    SupercruiseAssistImageFilename = string.Empty,
                    SupercruiseAssistTopText = string.Empty,
                    SupercruiseAssistTopColor = "#ffffff",
                    SupercruiseAssistTopTextVerticalPosition = "5",
                    SupercruiseAssistBottomText = string.Empty,
                    SupercruiseAssistBottomColor = "#ffffff",
                    SupercruiseAssistBottomTextVerticalPosition = "190",
                    SupercruiseAssistBold = "true",

                    NormalSpaceImageFilename = string.Empty,
                    NormalSpaceTopText = string.Empty,
                    NormalSpaceTopColor = "#ffffff",
                    NormalSpaceTopTextVerticalPosition = "5",
                    NormalSpaceBottomText = string.Empty,
                    NormalSpaceBottomColor = "#ffffff",
                    NormalSpaceBottomTextVerticalPosition = "190",
                    NormalSpaceBold = "true",

                    HyperspaceChargingImageFilename = string.Empty,
                    HyperspaceChargingTopText = string.Empty,
                    HyperspaceChargingTopColor = "#ffffff",
                    HyperspaceChargingTopTextVerticalPosition = "5",
                    HyperspaceChargingBottomText = string.Empty,
                    HyperspaceChargingBottomColor = "#ffffff",
                    HyperspaceChargingBottomTextVerticalPosition = "190",
                    HyperspaceChargingBold = "true",

                    FsdHyperspaceJumpingImageFilename = string.Empty,
                    FsdHyperspaceJumpingTopText = string.Empty,
                    FsdHyperspaceJumpingTopColor = "#ffffff",
                    FsdHyperspaceJumpingTopTextVerticalPosition = "5",
                    FsdHyperspaceJumpingBottomText = string.Empty,
                    FsdHyperspaceJumpingBottomColor = "#ffffff",
                    FsdHyperspaceJumpingBottomTextVerticalPosition = "190",
                    FsdHyperspaceJumpingBold = "true",

                    FuelScoopingImageFilename = string.Empty,
                    FuelScoopingTopText = string.Empty,
                    FuelScoopingTopColor = "#ffffff",
                    FuelScoopingTopTextVerticalPosition = "5",
                    FuelScoopingBottomText = string.Empty,
                    FuelScoopingBottomColor = "#ffffff",
                    FuelScoopingBottomTextVerticalPosition = "190",
                    FuelScoopingBold = "true",

                    PlanetApproachImageFilename = string.Empty,
                    PlanetApproachTopText = string.Empty,
                    PlanetApproachTopColor = "#ffffff",
                    PlanetApproachTopTextVerticalPosition = "5",
                    PlanetApproachBottomText = string.Empty,
                    PlanetApproachBottomColor = "#ffffff",
                    PlanetApproachBottomTextVerticalPosition = "190",
                    PlanetApproachBold = "true",

                    OrbitalCruiseImageFilename = string.Empty,
                    OrbitalCruiseTopText = string.Empty,
                    OrbitalCruiseTopColor = "#ffffff",
                    OrbitalCruiseTopTextVerticalPosition = "5",
                    OrbitalCruiseBottomText = string.Empty,
                    OrbitalCruiseBottomColor = "#ffffff",
                    OrbitalCruiseBottomTextVerticalPosition = "190",
                    OrbitalCruiseBold = "true",

                    DeorbitingImageFilename = string.Empty,
                    DeorbitingTopText = string.Empty,
                    DeorbitingTopColor = "#ffffff",
                    DeorbitingTopTextVerticalPosition = "5",
                    DeorbitingBottomText = string.Empty,
                    DeorbitingBottomColor = "#ffffff",
                    DeorbitingBottomTextVerticalPosition = "190",
                    DeorbitingBold = "true",

                    PlanetFlightImageFilename = string.Empty,
                    PlanetFlightTopText = string.Empty,
                    PlanetFlightTopColor = "#ffffff",
                    PlanetFlightTopTextVerticalPosition = "5",
                    PlanetFlightBottomText = string.Empty,
                    PlanetFlightBottomColor = "#ffffff",
                    PlanetFlightBottomTextVerticalPosition = "190",
                    PlanetFlightBold = "true",

                    LandedImageFilename = string.Empty,
                    LandedTopText = string.Empty,
                    LandedTopColor = "#ffffff",
                    LandedTopTextVerticalPosition = "5",
                    LandedBottomText = string.Empty,
                    LandedBottomColor = "#ffffff",
                    LandedBottomTextVerticalPosition = "190",
                    LandedBold = "true",

                    LiftoffImageFilename = string.Empty,
                    LiftoffTopText = string.Empty,
                    LiftoffTopColor = "#ffffff",
                    LiftoffTopTextVerticalPosition = "5",
                    LiftoffBottomText = string.Empty,
                    LiftoffBottomColor = "#ffffff",
                    LiftoffBottomTextVerticalPosition = "190",
                    LiftoffBold = "true",

                    LeavingPlanetImageFilename = string.Empty,
                    LeavingPlanetTopText = string.Empty,
                    LeavingPlanetTopColor = "#ffffff",
                    LeavingPlanetTopTextVerticalPosition = "5",
                    LeavingPlanetBottomText = string.Empty,
                    LeavingPlanetBottomColor = "#ffffff",
                    LeavingPlanetBottomTextVerticalPosition = "190",
                    LeavingPlanetBold = "true",

                    NoFireZoneImageFilename = string.Empty,
                    NoFireZoneTopText = string.Empty,
                    NoFireZoneTopColor = "#ffffff",
                    NoFireZoneTopTextVerticalPosition = "5",
                    NoFireZoneBottomText = string.Empty,
                    NoFireZoneBottomColor = "#ffffff",
                    NoFireZoneBottomTextVerticalPosition = "190",
                    NoFireZoneBold = "true",

                    StationApproachImageFilename = string.Empty,
                    StationApproachTopText = string.Empty,
                    StationApproachTopColor = "#ffffff",
                    StationApproachTopTextVerticalPosition = "5",
                    StationApproachBottomText = string.Empty,
                    StationApproachBottomColor = "#ffffff",
                    StationApproachBottomTextVerticalPosition = "190",
                    StationApproachBold = "true",

                    DockedAtStationImageFilename = string.Empty,
                    DockedAtStationTopText = string.Empty,
                    DockedAtStationTopColor = "#ffffff",
                    DockedAtStationTopTextVerticalPosition = "5",
                    DockedAtStationBottomText = string.Empty,
                    DockedAtStationBottomColor = "#ffffff",
                    DockedAtStationBottomTextVerticalPosition = "190",
                    DockedAtStationBold = "true",

                    OnFootInStationImageFilename = string.Empty,
                    OnFootInStationTopText = string.Empty,
                    OnFootInStationTopColor = "#ffffff",
                    OnFootInStationTopTextVerticalPosition = "5",
                    OnFootInStationBottomText = string.Empty,
                    OnFootInStationBottomColor = "#ffffff",
                    OnFootInStationBottomTextVerticalPosition = "190",
                    OnFootInStationBold = "true",

                    StationInteriorImageFilename = string.Empty,
                    StationInteriorTopText = string.Empty,
                    StationInteriorTopColor = "#ffffff",
                    StationInteriorTopTextVerticalPosition = "5",
                    StationInteriorBottomText = string.Empty,
                    StationInteriorBottomColor = "#ffffff",
                    StationInteriorBottomTextVerticalPosition = "190",
                    StationInteriorBold = "true",
                };
                return instance;
            }

            // Shown whenever the determined state has no image of its own configured, so the
            // button never falls back to the Stream Deck's own generic default image.
            [FilenameProperty]
            [JsonProperty(PropertyName = "notActiveImage")]
            public string NotActiveImageFilename { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "supercruiseChargingImage")]
            public string SupercruiseChargingImageFilename { get; set; }

            [JsonProperty(PropertyName = "supercruiseChargingTopText")]
            public string SupercruiseChargingTopText { get; set; }

            [JsonProperty(PropertyName = "supercruiseChargingTopColor")]
            public string SupercruiseChargingTopColor { get; set; }

            [JsonProperty(PropertyName = "supercruiseChargingTopTextVerticalPosition")]
            public string SupercruiseChargingTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "supercruiseChargingBottomText")]
            public string SupercruiseChargingBottomText { get; set; }

            [JsonProperty(PropertyName = "supercruiseChargingBottomColor")]
            public string SupercruiseChargingBottomColor { get; set; }

            [JsonProperty(PropertyName = "supercruiseChargingBottomTextVerticalPosition")]
            public string SupercruiseChargingBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "supercruiseChargingBold")]
            public string SupercruiseChargingBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "fsdSupercruiseJumpingImage")]
            public string FsdSupercruiseJumpingImageFilename { get; set; }

            [JsonProperty(PropertyName = "fsdSupercruiseJumpingTopText")]
            public string FsdSupercruiseJumpingTopText { get; set; }

            [JsonProperty(PropertyName = "fsdSupercruiseJumpingTopColor")]
            public string FsdSupercruiseJumpingTopColor { get; set; }

            [JsonProperty(PropertyName = "fsdSupercruiseJumpingTopTextVerticalPosition")]
            public string FsdSupercruiseJumpingTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "fsdSupercruiseJumpingBottomText")]
            public string FsdSupercruiseJumpingBottomText { get; set; }

            [JsonProperty(PropertyName = "fsdSupercruiseJumpingBottomColor")]
            public string FsdSupercruiseJumpingBottomColor { get; set; }

            [JsonProperty(PropertyName = "fsdSupercruiseJumpingBottomTextVerticalPosition")]
            public string FsdSupercruiseJumpingBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "fsdSupercruiseJumpingBold")]
            public string FsdSupercruiseJumpingBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "supercruiseImage")]
            public string SupercruiseImageFilename { get; set; }

            [JsonProperty(PropertyName = "supercruiseTopText")]
            public string SupercruiseTopText { get; set; }

            [JsonProperty(PropertyName = "supercruiseTopColor")]
            public string SupercruiseTopColor { get; set; }

            [JsonProperty(PropertyName = "supercruiseTopTextVerticalPosition")]
            public string SupercruiseTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "supercruiseBottomText")]
            public string SupercruiseBottomText { get; set; }

            [JsonProperty(PropertyName = "supercruiseBottomColor")]
            public string SupercruiseBottomColor { get; set; }

            [JsonProperty(PropertyName = "supercruiseBottomTextVerticalPosition")]
            public string SupercruiseBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "supercruiseBold")]
            public string SupercruiseBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "supercruiseOverchargeImage")]
            public string SupercruiseOverchargeImageFilename { get; set; }

            [JsonProperty(PropertyName = "supercruiseOverchargeTopText")]
            public string SupercruiseOverchargeTopText { get; set; }

            [JsonProperty(PropertyName = "supercruiseOverchargeTopColor")]
            public string SupercruiseOverchargeTopColor { get; set; }

            [JsonProperty(PropertyName = "supercruiseOverchargeTopTextVerticalPosition")]
            public string SupercruiseOverchargeTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "supercruiseOverchargeBottomText")]
            public string SupercruiseOverchargeBottomText { get; set; }

            [JsonProperty(PropertyName = "supercruiseOverchargeBottomColor")]
            public string SupercruiseOverchargeBottomColor { get; set; }

            [JsonProperty(PropertyName = "supercruiseOverchargeBottomTextVerticalPosition")]
            public string SupercruiseOverchargeBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "supercruiseOverchargeBold")]
            public string SupercruiseOverchargeBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "supercruiseAssistImage")]
            public string SupercruiseAssistImageFilename { get; set; }

            [JsonProperty(PropertyName = "supercruiseAssistTopText")]
            public string SupercruiseAssistTopText { get; set; }

            [JsonProperty(PropertyName = "supercruiseAssistTopColor")]
            public string SupercruiseAssistTopColor { get; set; }

            [JsonProperty(PropertyName = "supercruiseAssistTopTextVerticalPosition")]
            public string SupercruiseAssistTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "supercruiseAssistBottomText")]
            public string SupercruiseAssistBottomText { get; set; }

            [JsonProperty(PropertyName = "supercruiseAssistBottomColor")]
            public string SupercruiseAssistBottomColor { get; set; }

            [JsonProperty(PropertyName = "supercruiseAssistBottomTextVerticalPosition")]
            public string SupercruiseAssistBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "supercruiseAssistBold")]
            public string SupercruiseAssistBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "normalSpaceImage")]
            public string NormalSpaceImageFilename { get; set; }

            [JsonProperty(PropertyName = "normalSpaceTopText")]
            public string NormalSpaceTopText { get; set; }

            [JsonProperty(PropertyName = "normalSpaceTopColor")]
            public string NormalSpaceTopColor { get; set; }

            [JsonProperty(PropertyName = "normalSpaceTopTextVerticalPosition")]
            public string NormalSpaceTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "normalSpaceBottomText")]
            public string NormalSpaceBottomText { get; set; }

            [JsonProperty(PropertyName = "normalSpaceBottomColor")]
            public string NormalSpaceBottomColor { get; set; }

            [JsonProperty(PropertyName = "normalSpaceBottomTextVerticalPosition")]
            public string NormalSpaceBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "normalSpaceBold")]
            public string NormalSpaceBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "hyperspaceChargingImage")]
            public string HyperspaceChargingImageFilename { get; set; }

            [JsonProperty(PropertyName = "hyperspaceChargingTopText")]
            public string HyperspaceChargingTopText { get; set; }

            [JsonProperty(PropertyName = "hyperspaceChargingTopColor")]
            public string HyperspaceChargingTopColor { get; set; }

            [JsonProperty(PropertyName = "hyperspaceChargingTopTextVerticalPosition")]
            public string HyperspaceChargingTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "hyperspaceChargingBottomText")]
            public string HyperspaceChargingBottomText { get; set; }

            [JsonProperty(PropertyName = "hyperspaceChargingBottomColor")]
            public string HyperspaceChargingBottomColor { get; set; }

            [JsonProperty(PropertyName = "hyperspaceChargingBottomTextVerticalPosition")]
            public string HyperspaceChargingBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "hyperspaceChargingBold")]
            public string HyperspaceChargingBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "fsdHyperspaceJumpingImage")]
            public string FsdHyperspaceJumpingImageFilename { get; set; }

            [JsonProperty(PropertyName = "fsdHyperspaceJumpingTopText")]
            public string FsdHyperspaceJumpingTopText { get; set; }

            [JsonProperty(PropertyName = "fsdHyperspaceJumpingTopColor")]
            public string FsdHyperspaceJumpingTopColor { get; set; }

            [JsonProperty(PropertyName = "fsdHyperspaceJumpingTopTextVerticalPosition")]
            public string FsdHyperspaceJumpingTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "fsdHyperspaceJumpingBottomText")]
            public string FsdHyperspaceJumpingBottomText { get; set; }

            [JsonProperty(PropertyName = "fsdHyperspaceJumpingBottomColor")]
            public string FsdHyperspaceJumpingBottomColor { get; set; }

            [JsonProperty(PropertyName = "fsdHyperspaceJumpingBottomTextVerticalPosition")]
            public string FsdHyperspaceJumpingBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "fsdHyperspaceJumpingBold")]
            public string FsdHyperspaceJumpingBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "fuelScoopingImage")]
            public string FuelScoopingImageFilename { get; set; }

            [JsonProperty(PropertyName = "fuelScoopingTopText")]
            public string FuelScoopingTopText { get; set; }

            [JsonProperty(PropertyName = "fuelScoopingTopColor")]
            public string FuelScoopingTopColor { get; set; }

            [JsonProperty(PropertyName = "fuelScoopingTopTextVerticalPosition")]
            public string FuelScoopingTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "fuelScoopingBottomText")]
            public string FuelScoopingBottomText { get; set; }

            [JsonProperty(PropertyName = "fuelScoopingBottomColor")]
            public string FuelScoopingBottomColor { get; set; }

            [JsonProperty(PropertyName = "fuelScoopingBottomTextVerticalPosition")]
            public string FuelScoopingBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "fuelScoopingBold")]
            public string FuelScoopingBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "planetApproachImage")]
            public string PlanetApproachImageFilename { get; set; }

            [JsonProperty(PropertyName = "planetApproachTopText")]
            public string PlanetApproachTopText { get; set; }

            [JsonProperty(PropertyName = "planetApproachTopColor")]
            public string PlanetApproachTopColor { get; set; }

            [JsonProperty(PropertyName = "planetApproachTopTextVerticalPosition")]
            public string PlanetApproachTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "planetApproachBottomText")]
            public string PlanetApproachBottomText { get; set; }

            [JsonProperty(PropertyName = "planetApproachBottomColor")]
            public string PlanetApproachBottomColor { get; set; }

            [JsonProperty(PropertyName = "planetApproachBottomTextVerticalPosition")]
            public string PlanetApproachBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "planetApproachBold")]
            public string PlanetApproachBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "orbitalCruiseImage")]
            public string OrbitalCruiseImageFilename { get; set; }

            [JsonProperty(PropertyName = "orbitalCruiseTopText")]
            public string OrbitalCruiseTopText { get; set; }

            [JsonProperty(PropertyName = "orbitalCruiseTopColor")]
            public string OrbitalCruiseTopColor { get; set; }

            [JsonProperty(PropertyName = "orbitalCruiseTopTextVerticalPosition")]
            public string OrbitalCruiseTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "orbitalCruiseBottomText")]
            public string OrbitalCruiseBottomText { get; set; }

            [JsonProperty(PropertyName = "orbitalCruiseBottomColor")]
            public string OrbitalCruiseBottomColor { get; set; }

            [JsonProperty(PropertyName = "orbitalCruiseBottomTextVerticalPosition")]
            public string OrbitalCruiseBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "orbitalCruiseBold")]
            public string OrbitalCruiseBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "deorbitingImage")]
            public string DeorbitingImageFilename { get; set; }

            [JsonProperty(PropertyName = "deorbitingTopText")]
            public string DeorbitingTopText { get; set; }

            [JsonProperty(PropertyName = "deorbitingTopColor")]
            public string DeorbitingTopColor { get; set; }

            [JsonProperty(PropertyName = "deorbitingTopTextVerticalPosition")]
            public string DeorbitingTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "deorbitingBottomText")]
            public string DeorbitingBottomText { get; set; }

            [JsonProperty(PropertyName = "deorbitingBottomColor")]
            public string DeorbitingBottomColor { get; set; }

            [JsonProperty(PropertyName = "deorbitingBottomTextVerticalPosition")]
            public string DeorbitingBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "deorbitingBold")]
            public string DeorbitingBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "planetFlightImage")]
            public string PlanetFlightImageFilename { get; set; }

            [JsonProperty(PropertyName = "planetFlightTopText")]
            public string PlanetFlightTopText { get; set; }

            [JsonProperty(PropertyName = "planetFlightTopColor")]
            public string PlanetFlightTopColor { get; set; }

            [JsonProperty(PropertyName = "planetFlightTopTextVerticalPosition")]
            public string PlanetFlightTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "planetFlightBottomText")]
            public string PlanetFlightBottomText { get; set; }

            [JsonProperty(PropertyName = "planetFlightBottomColor")]
            public string PlanetFlightBottomColor { get; set; }

            [JsonProperty(PropertyName = "planetFlightBottomTextVerticalPosition")]
            public string PlanetFlightBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "planetFlightBold")]
            public string PlanetFlightBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "landedImage")]
            public string LandedImageFilename { get; set; }

            [JsonProperty(PropertyName = "landedTopText")]
            public string LandedTopText { get; set; }

            [JsonProperty(PropertyName = "landedTopColor")]
            public string LandedTopColor { get; set; }

            [JsonProperty(PropertyName = "landedTopTextVerticalPosition")]
            public string LandedTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "landedBottomText")]
            public string LandedBottomText { get; set; }

            [JsonProperty(PropertyName = "landedBottomColor")]
            public string LandedBottomColor { get; set; }

            [JsonProperty(PropertyName = "landedBottomTextVerticalPosition")]
            public string LandedBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "landedBold")]
            public string LandedBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "liftoffImage")]
            public string LiftoffImageFilename { get; set; }

            [JsonProperty(PropertyName = "liftoffTopText")]
            public string LiftoffTopText { get; set; }

            [JsonProperty(PropertyName = "liftoffTopColor")]
            public string LiftoffTopColor { get; set; }

            [JsonProperty(PropertyName = "liftoffTopTextVerticalPosition")]
            public string LiftoffTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "liftoffBottomText")]
            public string LiftoffBottomText { get; set; }

            [JsonProperty(PropertyName = "liftoffBottomColor")]
            public string LiftoffBottomColor { get; set; }

            [JsonProperty(PropertyName = "liftoffBottomTextVerticalPosition")]
            public string LiftoffBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "liftoffBold")]
            public string LiftoffBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "leavingPlanetImage")]
            public string LeavingPlanetImageFilename { get; set; }

            [JsonProperty(PropertyName = "leavingPlanetTopText")]
            public string LeavingPlanetTopText { get; set; }

            [JsonProperty(PropertyName = "leavingPlanetTopColor")]
            public string LeavingPlanetTopColor { get; set; }

            [JsonProperty(PropertyName = "leavingPlanetTopTextVerticalPosition")]
            public string LeavingPlanetTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "leavingPlanetBottomText")]
            public string LeavingPlanetBottomText { get; set; }

            [JsonProperty(PropertyName = "leavingPlanetBottomColor")]
            public string LeavingPlanetBottomColor { get; set; }

            [JsonProperty(PropertyName = "leavingPlanetBottomTextVerticalPosition")]
            public string LeavingPlanetBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "leavingPlanetBold")]
            public string LeavingPlanetBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "noFireZoneImage")]
            public string NoFireZoneImageFilename { get; set; }

            [JsonProperty(PropertyName = "noFireZoneTopText")]
            public string NoFireZoneTopText { get; set; }

            [JsonProperty(PropertyName = "noFireZoneTopColor")]
            public string NoFireZoneTopColor { get; set; }

            [JsonProperty(PropertyName = "noFireZoneTopTextVerticalPosition")]
            public string NoFireZoneTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "noFireZoneBottomText")]
            public string NoFireZoneBottomText { get; set; }

            [JsonProperty(PropertyName = "noFireZoneBottomColor")]
            public string NoFireZoneBottomColor { get; set; }

            [JsonProperty(PropertyName = "noFireZoneBottomTextVerticalPosition")]
            public string NoFireZoneBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "noFireZoneBold")]
            public string NoFireZoneBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "stationApproachImage")]
            public string StationApproachImageFilename { get; set; }

            [JsonProperty(PropertyName = "stationApproachTopText")]
            public string StationApproachTopText { get; set; }

            [JsonProperty(PropertyName = "stationApproachTopColor")]
            public string StationApproachTopColor { get; set; }

            [JsonProperty(PropertyName = "stationApproachTopTextVerticalPosition")]
            public string StationApproachTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "stationApproachBottomText")]
            public string StationApproachBottomText { get; set; }

            [JsonProperty(PropertyName = "stationApproachBottomColor")]
            public string StationApproachBottomColor { get; set; }

            [JsonProperty(PropertyName = "stationApproachBottomTextVerticalPosition")]
            public string StationApproachBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "stationApproachBold")]
            public string StationApproachBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "dockedAtStationImage")]
            public string DockedAtStationImageFilename { get; set; }

            [JsonProperty(PropertyName = "dockedAtStationTopText")]
            public string DockedAtStationTopText { get; set; }

            [JsonProperty(PropertyName = "dockedAtStationTopColor")]
            public string DockedAtStationTopColor { get; set; }

            [JsonProperty(PropertyName = "dockedAtStationTopTextVerticalPosition")]
            public string DockedAtStationTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "dockedAtStationBottomText")]
            public string DockedAtStationBottomText { get; set; }

            [JsonProperty(PropertyName = "dockedAtStationBottomColor")]
            public string DockedAtStationBottomColor { get; set; }

            [JsonProperty(PropertyName = "dockedAtStationBottomTextVerticalPosition")]
            public string DockedAtStationBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "dockedAtStationBold")]
            public string DockedAtStationBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "onFootInStationImage")]
            public string OnFootInStationImageFilename { get; set; }

            [JsonProperty(PropertyName = "onFootInStationTopText")]
            public string OnFootInStationTopText { get; set; }

            [JsonProperty(PropertyName = "onFootInStationTopColor")]
            public string OnFootInStationTopColor { get; set; }

            [JsonProperty(PropertyName = "onFootInStationTopTextVerticalPosition")]
            public string OnFootInStationTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "onFootInStationBottomText")]
            public string OnFootInStationBottomText { get; set; }

            [JsonProperty(PropertyName = "onFootInStationBottomColor")]
            public string OnFootInStationBottomColor { get; set; }

            [JsonProperty(PropertyName = "onFootInStationBottomTextVerticalPosition")]
            public string OnFootInStationBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "onFootInStationBold")]
            public string OnFootInStationBold { get; set; }

            [FilenameProperty]
            [JsonProperty(PropertyName = "stationInteriorImage")]
            public string StationInteriorImageFilename { get; set; }

            [JsonProperty(PropertyName = "stationInteriorTopText")]
            public string StationInteriorTopText { get; set; }

            [JsonProperty(PropertyName = "stationInteriorTopColor")]
            public string StationInteriorTopColor { get; set; }

            [JsonProperty(PropertyName = "stationInteriorTopTextVerticalPosition")]
            public string StationInteriorTopTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "stationInteriorBottomText")]
            public string StationInteriorBottomText { get; set; }

            [JsonProperty(PropertyName = "stationInteriorBottomColor")]
            public string StationInteriorBottomColor { get; set; }

            [JsonProperty(PropertyName = "stationInteriorBottomTextVerticalPosition")]
            public string StationInteriorBottomTextVerticalPosition { get; set; }

            [JsonProperty(PropertyName = "stationInteriorBold")]
            public string StationInteriorBold { get; set; }

        }

        private PluginSettings settings;

        private Bitmap _notActiveImage = null;
        private bool _notActiveImageIsGif = false;
        private string _notActiveFile;

        // Cached base64 image strings / brushes, one set per state
        private Bitmap _supercruiseChargingImage = null;
        private bool _supercruiseChargingImageIsGif = false;
        private string _supercruiseChargingFile;
        private SolidBrush _supercruiseChargingTopBrush = new SolidBrush(Color.White);
        private SolidBrush _supercruiseChargingBottomBrush = new SolidBrush(Color.White);

        private Bitmap _fsdSupercruiseJumpingImage = null;
        private bool _fsdSupercruiseJumpingImageIsGif = false;
        private string _fsdSupercruiseJumpingFile;
        private SolidBrush _fsdSupercruiseJumpingTopBrush = new SolidBrush(Color.White);
        private SolidBrush _fsdSupercruiseJumpingBottomBrush = new SolidBrush(Color.White);

        private Bitmap _supercruiseImage = null;
        private bool _supercruiseImageIsGif = false;
        private string _supercruiseFile;
        private SolidBrush _supercruiseTopBrush = new SolidBrush(Color.White);
        private SolidBrush _supercruiseBottomBrush = new SolidBrush(Color.White);

        private Bitmap _supercruiseOverchargeImage = null;
        private bool _supercruiseOverchargeImageIsGif = false;
        private string _supercruiseOverchargeFile;
        private SolidBrush _supercruiseOverchargeTopBrush = new SolidBrush(Color.White);
        private SolidBrush _supercruiseOverchargeBottomBrush = new SolidBrush(Color.White);

        private Bitmap _supercruiseAssistImage = null;
        private bool _supercruiseAssistImageIsGif = false;
        private string _supercruiseAssistFile;
        private SolidBrush _supercruiseAssistTopBrush = new SolidBrush(Color.White);
        private SolidBrush _supercruiseAssistBottomBrush = new SolidBrush(Color.White);

        private Bitmap _normalSpaceImage = null;
        private bool _normalSpaceImageIsGif = false;
        private string _normalSpaceFile;
        private SolidBrush _normalSpaceTopBrush = new SolidBrush(Color.White);
        private SolidBrush _normalSpaceBottomBrush = new SolidBrush(Color.White);

        private Bitmap _hyperspaceChargingImage = null;
        private bool _hyperspaceChargingImageIsGif = false;
        private string _hyperspaceChargingFile;
        private SolidBrush _hyperspaceChargingTopBrush = new SolidBrush(Color.White);
        private SolidBrush _hyperspaceChargingBottomBrush = new SolidBrush(Color.White);

        private Bitmap _fsdHyperspaceJumpingImage = null;
        private bool _fsdHyperspaceJumpingImageIsGif = false;
        private string _fsdHyperspaceJumpingFile;
        private SolidBrush _fsdHyperspaceJumpingTopBrush = new SolidBrush(Color.White);
        private SolidBrush _fsdHyperspaceJumpingBottomBrush = new SolidBrush(Color.White);

        private Bitmap _fuelScoopingImage = null;
        private bool _fuelScoopingImageIsGif = false;
        private string _fuelScoopingFile;
        private SolidBrush _fuelScoopingTopBrush = new SolidBrush(Color.White);
        private SolidBrush _fuelScoopingBottomBrush = new SolidBrush(Color.White);

        private Bitmap _planetApproachImage = null;
        private bool _planetApproachImageIsGif = false;
        private string _planetApproachFile;
        private SolidBrush _planetApproachTopBrush = new SolidBrush(Color.White);
        private SolidBrush _planetApproachBottomBrush = new SolidBrush(Color.White);

        private Bitmap _orbitalCruiseImage = null;
        private bool _orbitalCruiseImageIsGif = false;
        private string _orbitalCruiseFile;
        private SolidBrush _orbitalCruiseTopBrush = new SolidBrush(Color.White);
        private SolidBrush _orbitalCruiseBottomBrush = new SolidBrush(Color.White);

        private Bitmap _deorbitingImage = null;
        private bool _deorbitingImageIsGif = false;
        private string _deorbitingFile;
        private SolidBrush _deorbitingTopBrush = new SolidBrush(Color.White);
        private SolidBrush _deorbitingBottomBrush = new SolidBrush(Color.White);

        private Bitmap _planetFlightImage = null;
        private bool _planetFlightImageIsGif = false;
        private string _planetFlightFile;
        private SolidBrush _planetFlightTopBrush = new SolidBrush(Color.White);
        private SolidBrush _planetFlightBottomBrush = new SolidBrush(Color.White);

        private Bitmap _landedImage = null;
        private bool _landedImageIsGif = false;
        private string _landedFile;
        private SolidBrush _landedTopBrush = new SolidBrush(Color.White);
        private SolidBrush _landedBottomBrush = new SolidBrush(Color.White);

        private Bitmap _liftoffImage = null;
        private bool _liftoffImageIsGif = false;
        private string _liftoffFile;
        private SolidBrush _liftoffTopBrush = new SolidBrush(Color.White);
        private SolidBrush _liftoffBottomBrush = new SolidBrush(Color.White);

        private Bitmap _leavingPlanetImage = null;
        private bool _leavingPlanetImageIsGif = false;
        private string _leavingPlanetFile;
        private SolidBrush _leavingPlanetTopBrush = new SolidBrush(Color.White);
        private SolidBrush _leavingPlanetBottomBrush = new SolidBrush(Color.White);

        private Bitmap _noFireZoneImage = null;
        private bool _noFireZoneImageIsGif = false;
        private string _noFireZoneFile;
        private SolidBrush _noFireZoneTopBrush = new SolidBrush(Color.White);
        private SolidBrush _noFireZoneBottomBrush = new SolidBrush(Color.White);

        private Bitmap _stationApproachImage = null;
        private bool _stationApproachImageIsGif = false;
        private string _stationApproachFile;
        private SolidBrush _stationApproachTopBrush = new SolidBrush(Color.White);
        private SolidBrush _stationApproachBottomBrush = new SolidBrush(Color.White);

        private Bitmap _dockedAtStationImage = null;
        private bool _dockedAtStationImageIsGif = false;
        private string _dockedAtStationFile;
        private SolidBrush _dockedAtStationTopBrush = new SolidBrush(Color.White);
        private SolidBrush _dockedAtStationBottomBrush = new SolidBrush(Color.White);

        private Bitmap _onFootInStationImage = null;
        private bool _onFootInStationImageIsGif = false;
        private string _onFootInStationFile;
        private SolidBrush _onFootInStationTopBrush = new SolidBrush(Color.White);
        private SolidBrush _onFootInStationBottomBrush = new SolidBrush(Color.White);

        private Bitmap _stationInteriorImage = null;
        private bool _stationInteriorImageIsGif = false;
        private string _stationInteriorFile;
        private SolidBrush _stationInteriorTopBrush = new SolidBrush(Color.White);
        private SolidBrush _stationInteriorBottomBrush = new SolidBrush(Color.White);

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
        private bool _hadLatLong = false;
        private bool _climbing = false;
        private int _fallingTicks = 0;

        // Any sub-state reached while s.Supercruise is true (PlanetApproach, OrbitalCruise,
        // LeavingPlanet, or plain Supercruise) is overridden by the Overcharge/Assist ship
        // features whenever either is active, and falls back to the normal sub-state the
        // moment they end. Deorbiting is intentionally excluded - by the time GlideMode has
        // taken over the ship has already dropped out of supercruise.
        private OdysseyShipStatusState OverrideForSupercruiseFeature(OdysseyShipStatusState fallback)
        {
            var s = EliteData.StatusData;

            if (s.SuperCruiseOvercharge) return OdysseyShipStatusState.SupercruiseOvercharge;
            if (s.SuperCruiseAssist) return OdysseyShipStatusState.SupercruiseAssist;

            return fallback;
        }

        private OdysseyShipStatusState DetermineState()
        {
            var s = EliteData.StatusData;

            // FSD Jump in progress - highest priority
            if (s.FsdJump)
                return _isHyperspaceJump
                    ? OdysseyShipStatusState.FsdHyperspaceJumping
                    : OdysseyShipStatusState.FsdSupercruiseJumping;

            // Hyperspace FSD charging
            if (s.Fsdhyperdrivecharging)
                return OdysseyShipStatusState.HyperspaceCharging;

            // Supercruise charging - FsdCharging set but not hyperspace
            if (s.FsdCharging && !s.Fsdhyperdrivecharging)
                return OdysseyShipStatusState.SupercruiseCharging;

            // On foot inside a space station
            if (s.OnFootInStation)
                return OdysseyShipStatusState.OnFootInStation;

            // Docked at station - check for interior (past mail slot)
            if (s.Docked)
            {
                if (s.OnFoot)
                    return OdysseyShipStatusState.StationInterior;
                return OdysseyShipStatusState.DockedAtStation;
            }

            // Landed on a planet surface
            if (s.Landed)
                return OdysseyShipStatusState.Landed;

            // Liftoff - show for 2.5 seconds after liftoff event
            if ((DateTime.Now - _liftoffTime).TotalSeconds <= 2.5)
                return OdysseyShipStatusState.Liftoff;

            // Fuel scooping
            if (s.ScoopingFuel)
                return OdysseyShipStatusState.FuelScooping;

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
                        return OdysseyShipStatusState.Deorbiting;
                    }

                    if (!_hadLatLong)
                    {
                        _previousAltitude = s.Altitude;
                        _hadLatLong = true;
                        if (!_climbing)
                            return OverrideForSupercruiseFeature(OdysseyShipStatusState.PlanetApproach);
                    }

                    bool altitudeRising = s.Altitude > _previousAltitude;
                    _previousAltitude = s.Altitude;

                    if (_approachingBody)
                        return OverrideForSupercruiseFeature(OdysseyShipStatusState.OrbitalCruise);

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
                            return OverrideForSupercruiseFeature(OdysseyShipStatusState.PlanetApproach);
                        }
                        return OverrideForSupercruiseFeature(OdysseyShipStatusState.LeavingPlanet);
                    }

                    // LeaveBody has fired and we turned around then climbed again - restore LeavingPlanet
                    if (_climbing && _liftedOff && _hadLeaveBody)
                    {
                        _leavingPlanet = true;
                        return OverrideForSupercruiseFeature(OdysseyShipStatusState.LeavingPlanet);
                    }

                    if (_climbing && _liftedOff)
                        return OverrideForSupercruiseFeature(OdysseyShipStatusState.OrbitalCruise);

                    // Climbing while HasLatLong is true means we're pulling away from a body we
                    // were near, even if _liftedOff never got set via a formal ApproachBody/Liftoff
                    // event (e.g. climbing straight out of PlanetApproach). Treat it as leaving.
                    if (_climbing)
                    {
                        _leavingPlanet = true;
                        _liftedOff = true;
                        return OverrideForSupercruiseFeature(OdysseyShipStatusState.LeavingPlanet);
                    }

                    return OverrideForSupercruiseFeature(OdysseyShipStatusState.PlanetApproach);
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

                return OverrideForSupercruiseFeature(OdysseyShipStatusState.Supercruise);
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
                    return OdysseyShipStatusState.Deorbiting;
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
                    return OdysseyShipStatusState.Deorbiting;
                }

                if (_approachingBody)
                    return OdysseyShipStatusState.OrbitalCruise;

                return OdysseyShipStatusState.PlanetFlight;
            }

            // Normal space states (not supercruise, not on planet)
            // Station Approach (docking granted, flying in) takes priority over the more
            // generic No-Fire Zone - both flags go true together on DockingGranted, but
            // Approach is the more specific state and should win until actually docked.
            if (_dockingGranted)
                return OdysseyShipStatusState.StationApproach;

            if (_inNoFireZone)
                return OdysseyShipStatusState.NoFireZone;

            return OdysseyShipStatusState.NormalSpace;
        }

        // ===================== drawing =====================

        private (Bitmap image, string file, bool isGif,
                 string topText, SolidBrush topBrush, string topPosition,
                 string bottomText, SolidBrush bottomBrush, string bottomPosition,
                 bool bold) GetStateVisual(OdysseyShipStatusState state)
        {
            switch (state)
            {
                case OdysseyShipStatusState.SupercruiseCharging:
                    return (_supercruiseChargingImage, _supercruiseChargingFile, _supercruiseChargingImageIsGif,
                        settings.SupercruiseChargingTopText, _supercruiseChargingTopBrush, settings.SupercruiseChargingTopTextVerticalPosition,
                        settings.SupercruiseChargingBottomText, _supercruiseChargingBottomBrush, settings.SupercruiseChargingBottomTextVerticalPosition,
                        settings.SupercruiseChargingBold == "true");
                case OdysseyShipStatusState.FsdSupercruiseJumping:
                    return (_fsdSupercruiseJumpingImage, _fsdSupercruiseJumpingFile, _fsdSupercruiseJumpingImageIsGif,
                        settings.FsdSupercruiseJumpingTopText, _fsdSupercruiseJumpingTopBrush, settings.FsdSupercruiseJumpingTopTextVerticalPosition,
                        settings.FsdSupercruiseJumpingBottomText, _fsdSupercruiseJumpingBottomBrush, settings.FsdSupercruiseJumpingBottomTextVerticalPosition,
                        settings.FsdSupercruiseJumpingBold == "true");
                case OdysseyShipStatusState.Supercruise:
                    return (_supercruiseImage, _supercruiseFile, _supercruiseImageIsGif,
                        settings.SupercruiseTopText, _supercruiseTopBrush, settings.SupercruiseTopTextVerticalPosition,
                        settings.SupercruiseBottomText, _supercruiseBottomBrush, settings.SupercruiseBottomTextVerticalPosition,
                        settings.SupercruiseBold == "true");
                case OdysseyShipStatusState.SupercruiseOvercharge:
                    return (_supercruiseOverchargeImage, _supercruiseOverchargeFile, _supercruiseOverchargeImageIsGif,
                        settings.SupercruiseOverchargeTopText, _supercruiseOverchargeTopBrush, settings.SupercruiseOverchargeTopTextVerticalPosition,
                        settings.SupercruiseOverchargeBottomText, _supercruiseOverchargeBottomBrush, settings.SupercruiseOverchargeBottomTextVerticalPosition,
                        settings.SupercruiseOverchargeBold == "true");
                case OdysseyShipStatusState.SupercruiseAssist:
                    return (_supercruiseAssistImage, _supercruiseAssistFile, _supercruiseAssistImageIsGif,
                        settings.SupercruiseAssistTopText, _supercruiseAssistTopBrush, settings.SupercruiseAssistTopTextVerticalPosition,
                        settings.SupercruiseAssistBottomText, _supercruiseAssistBottomBrush, settings.SupercruiseAssistBottomTextVerticalPosition,
                        settings.SupercruiseAssistBold == "true");
                case OdysseyShipStatusState.NormalSpace:
                    return (_normalSpaceImage, _normalSpaceFile, _normalSpaceImageIsGif,
                        settings.NormalSpaceTopText, _normalSpaceTopBrush, settings.NormalSpaceTopTextVerticalPosition,
                        settings.NormalSpaceBottomText, _normalSpaceBottomBrush, settings.NormalSpaceBottomTextVerticalPosition,
                        settings.NormalSpaceBold == "true");
                case OdysseyShipStatusState.HyperspaceCharging:
                    return (_hyperspaceChargingImage, _hyperspaceChargingFile, _hyperspaceChargingImageIsGif,
                        settings.HyperspaceChargingTopText, _hyperspaceChargingTopBrush, settings.HyperspaceChargingTopTextVerticalPosition,
                        settings.HyperspaceChargingBottomText, _hyperspaceChargingBottomBrush, settings.HyperspaceChargingBottomTextVerticalPosition,
                        settings.HyperspaceChargingBold == "true");
                case OdysseyShipStatusState.FsdHyperspaceJumping:
                    return (_fsdHyperspaceJumpingImage, _fsdHyperspaceJumpingFile, _fsdHyperspaceJumpingImageIsGif,
                        settings.FsdHyperspaceJumpingTopText, _fsdHyperspaceJumpingTopBrush, settings.FsdHyperspaceJumpingTopTextVerticalPosition,
                        settings.FsdHyperspaceJumpingBottomText, _fsdHyperspaceJumpingBottomBrush, settings.FsdHyperspaceJumpingBottomTextVerticalPosition,
                        settings.FsdHyperspaceJumpingBold == "true");
                case OdysseyShipStatusState.FuelScooping:
                    return (_fuelScoopingImage, _fuelScoopingFile, _fuelScoopingImageIsGif,
                        settings.FuelScoopingTopText, _fuelScoopingTopBrush, settings.FuelScoopingTopTextVerticalPosition,
                        settings.FuelScoopingBottomText, _fuelScoopingBottomBrush, settings.FuelScoopingBottomTextVerticalPosition,
                        settings.FuelScoopingBold == "true");
                case OdysseyShipStatusState.PlanetApproach:
                    return (_planetApproachImage, _planetApproachFile, _planetApproachImageIsGif,
                        settings.PlanetApproachTopText, _planetApproachTopBrush, settings.PlanetApproachTopTextVerticalPosition,
                        settings.PlanetApproachBottomText, _planetApproachBottomBrush, settings.PlanetApproachBottomTextVerticalPosition,
                        settings.PlanetApproachBold == "true");
                case OdysseyShipStatusState.OrbitalCruise:
                    return (_orbitalCruiseImage, _orbitalCruiseFile, _orbitalCruiseImageIsGif,
                        settings.OrbitalCruiseTopText, _orbitalCruiseTopBrush, settings.OrbitalCruiseTopTextVerticalPosition,
                        settings.OrbitalCruiseBottomText, _orbitalCruiseBottomBrush, settings.OrbitalCruiseBottomTextVerticalPosition,
                        settings.OrbitalCruiseBold == "true");
                case OdysseyShipStatusState.Deorbiting:
                    return (_deorbitingImage, _deorbitingFile, _deorbitingImageIsGif,
                        settings.DeorbitingTopText, _deorbitingTopBrush, settings.DeorbitingTopTextVerticalPosition,
                        settings.DeorbitingBottomText, _deorbitingBottomBrush, settings.DeorbitingBottomTextVerticalPosition,
                        settings.DeorbitingBold == "true");
                case OdysseyShipStatusState.PlanetFlight:
                    return (_planetFlightImage, _planetFlightFile, _planetFlightImageIsGif,
                        settings.PlanetFlightTopText, _planetFlightTopBrush, settings.PlanetFlightTopTextVerticalPosition,
                        settings.PlanetFlightBottomText, _planetFlightBottomBrush, settings.PlanetFlightBottomTextVerticalPosition,
                        settings.PlanetFlightBold == "true");
                case OdysseyShipStatusState.Landed:
                    return (_landedImage, _landedFile, _landedImageIsGif,
                        settings.LandedTopText, _landedTopBrush, settings.LandedTopTextVerticalPosition,
                        settings.LandedBottomText, _landedBottomBrush, settings.LandedBottomTextVerticalPosition,
                        settings.LandedBold == "true");
                case OdysseyShipStatusState.Liftoff:
                    return (_liftoffImage, _liftoffFile, _liftoffImageIsGif,
                        settings.LiftoffTopText, _liftoffTopBrush, settings.LiftoffTopTextVerticalPosition,
                        settings.LiftoffBottomText, _liftoffBottomBrush, settings.LiftoffBottomTextVerticalPosition,
                        settings.LiftoffBold == "true");
                case OdysseyShipStatusState.LeavingPlanet:
                    return (_leavingPlanetImage, _leavingPlanetFile, _leavingPlanetImageIsGif,
                        settings.LeavingPlanetTopText, _leavingPlanetTopBrush, settings.LeavingPlanetTopTextVerticalPosition,
                        settings.LeavingPlanetBottomText, _leavingPlanetBottomBrush, settings.LeavingPlanetBottomTextVerticalPosition,
                        settings.LeavingPlanetBold == "true");
                case OdysseyShipStatusState.NoFireZone:
                    return (_noFireZoneImage, _noFireZoneFile, _noFireZoneImageIsGif,
                        settings.NoFireZoneTopText, _noFireZoneTopBrush, settings.NoFireZoneTopTextVerticalPosition,
                        settings.NoFireZoneBottomText, _noFireZoneBottomBrush, settings.NoFireZoneBottomTextVerticalPosition,
                        settings.NoFireZoneBold == "true");
                case OdysseyShipStatusState.StationApproach:
                    return (_stationApproachImage, _stationApproachFile, _stationApproachImageIsGif,
                        settings.StationApproachTopText, _stationApproachTopBrush, settings.StationApproachTopTextVerticalPosition,
                        settings.StationApproachBottomText, _stationApproachBottomBrush, settings.StationApproachBottomTextVerticalPosition,
                        settings.StationApproachBold == "true");
                case OdysseyShipStatusState.DockedAtStation:
                    return (_dockedAtStationImage, _dockedAtStationFile, _dockedAtStationImageIsGif,
                        settings.DockedAtStationTopText, _dockedAtStationTopBrush, settings.DockedAtStationTopTextVerticalPosition,
                        settings.DockedAtStationBottomText, _dockedAtStationBottomBrush, settings.DockedAtStationBottomTextVerticalPosition,
                        settings.DockedAtStationBold == "true");
                case OdysseyShipStatusState.OnFootInStation:
                    return (_onFootInStationImage, _onFootInStationFile, _onFootInStationImageIsGif,
                        settings.OnFootInStationTopText, _onFootInStationTopBrush, settings.OnFootInStationTopTextVerticalPosition,
                        settings.OnFootInStationBottomText, _onFootInStationBottomBrush, settings.OnFootInStationBottomTextVerticalPosition,
                        settings.OnFootInStationBold == "true");
                case OdysseyShipStatusState.StationInterior:
                    return (_stationInteriorImage, _stationInteriorFile, _stationInteriorImageIsGif,
                        settings.StationInteriorTopText, _stationInteriorTopBrush, settings.StationInteriorTopTextVerticalPosition,
                        settings.StationInteriorBottomText, _stationInteriorBottomBrush, settings.StationInteriorBottomTextVerticalPosition,
                        settings.StationInteriorBold == "true");
                default:
                    return (null, null, false, string.Empty, _landedTopBrush, "5", string.Empty, _landedBottomBrush, "190", true);
            }
        }

        // Mirrors OnFootExploration.cs / RouteAdv.cs's text fitting: tries font sizes from large to
        // small and picks the largest one whose width still fits the button.
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
            var state = DetermineState();
            var visual = GetStateVisual(state);

            // Fall back to the Not Active image when this state has no image of its own configured,
            // rather than leaving the Stream Deck's own generic default button showing.
            var image = visual.image ?? _notActiveImage;
            var file = visual.image != null ? visual.file : _notActiveFile;
            var isGif = visual.image != null ? visual.isGif : _notActiveImageIsGif;

            if (image == null) return;

            var imgBase64 = file;

            if (!isGif)
            {
                try
                {
                    using (var bitmap = new Bitmap(image))
                    using (var graphics = Graphics.FromImage(bitmap))
                    {
                        var width = bitmap.Width;
                        var topPos = double.TryParse(visual.topPosition, out double tp) ? tp : 5.0;
                        var bottomPos = double.TryParse(visual.bottomPosition, out double bp) ? bp : 190.0;

                        DrawFittedText(graphics, visual.topText, visual.topBrush.Color, topPos, visual.bold, width);
                        DrawFittedText(graphics, visual.bottomText, visual.bottomBrush.Color, bottomPos, visual.bold, width);

                        imgBase64 = BarRaider.SdTools.Tools.ImageToBase64(bitmap, true);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.LogMessage(TracingLevel.FATAL, "OdysseyShipStatus HandleDisplay " + ex);
                }
            }

            await Connection.SetImageAsync(imgBase64);
        }

        public OdysseyShipStatus(SDConnection connection, InitialPayload payload) : base(connection, payload)
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
            var e = args.EventArgs;
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
            if (string.IsNullOrEmpty(settings.SupercruiseChargingTopColor)) settings.SupercruiseChargingTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.SupercruiseChargingBottomColor)) settings.SupercruiseChargingBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.SupercruiseChargingTopTextVerticalPosition)) settings.SupercruiseChargingTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.SupercruiseChargingBottomTextVerticalPosition)) settings.SupercruiseChargingBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.SupercruiseChargingBold)) settings.SupercruiseChargingBold = "true";
            if (string.IsNullOrEmpty(settings.FsdSupercruiseJumpingTopColor)) settings.FsdSupercruiseJumpingTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.FsdSupercruiseJumpingBottomColor)) settings.FsdSupercruiseJumpingBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.FsdSupercruiseJumpingTopTextVerticalPosition)) settings.FsdSupercruiseJumpingTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.FsdSupercruiseJumpingBottomTextVerticalPosition)) settings.FsdSupercruiseJumpingBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.FsdSupercruiseJumpingBold)) settings.FsdSupercruiseJumpingBold = "true";
            if (string.IsNullOrEmpty(settings.SupercruiseTopColor)) settings.SupercruiseTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.SupercruiseBottomColor)) settings.SupercruiseBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.SupercruiseTopTextVerticalPosition)) settings.SupercruiseTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.SupercruiseBottomTextVerticalPosition)) settings.SupercruiseBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.SupercruiseBold)) settings.SupercruiseBold = "true";
            if (string.IsNullOrEmpty(settings.SupercruiseOverchargeTopColor)) settings.SupercruiseOverchargeTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.SupercruiseOverchargeBottomColor)) settings.SupercruiseOverchargeBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.SupercruiseOverchargeTopTextVerticalPosition)) settings.SupercruiseOverchargeTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.SupercruiseOverchargeBottomTextVerticalPosition)) settings.SupercruiseOverchargeBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.SupercruiseOverchargeBold)) settings.SupercruiseOverchargeBold = "true";
            if (string.IsNullOrEmpty(settings.SupercruiseAssistTopColor)) settings.SupercruiseAssistTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.SupercruiseAssistBottomColor)) settings.SupercruiseAssistBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.SupercruiseAssistTopTextVerticalPosition)) settings.SupercruiseAssistTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.SupercruiseAssistBottomTextVerticalPosition)) settings.SupercruiseAssistBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.SupercruiseAssistBold)) settings.SupercruiseAssistBold = "true";
            if (string.IsNullOrEmpty(settings.NormalSpaceTopColor)) settings.NormalSpaceTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.NormalSpaceBottomColor)) settings.NormalSpaceBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.NormalSpaceTopTextVerticalPosition)) settings.NormalSpaceTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.NormalSpaceBottomTextVerticalPosition)) settings.NormalSpaceBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.NormalSpaceBold)) settings.NormalSpaceBold = "true";
            if (string.IsNullOrEmpty(settings.HyperspaceChargingTopColor)) settings.HyperspaceChargingTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.HyperspaceChargingBottomColor)) settings.HyperspaceChargingBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.HyperspaceChargingTopTextVerticalPosition)) settings.HyperspaceChargingTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.HyperspaceChargingBottomTextVerticalPosition)) settings.HyperspaceChargingBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.HyperspaceChargingBold)) settings.HyperspaceChargingBold = "true";
            if (string.IsNullOrEmpty(settings.FsdHyperspaceJumpingTopColor)) settings.FsdHyperspaceJumpingTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.FsdHyperspaceJumpingBottomColor)) settings.FsdHyperspaceJumpingBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.FsdHyperspaceJumpingTopTextVerticalPosition)) settings.FsdHyperspaceJumpingTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.FsdHyperspaceJumpingBottomTextVerticalPosition)) settings.FsdHyperspaceJumpingBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.FsdHyperspaceJumpingBold)) settings.FsdHyperspaceJumpingBold = "true";
            if (string.IsNullOrEmpty(settings.FuelScoopingTopColor)) settings.FuelScoopingTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.FuelScoopingBottomColor)) settings.FuelScoopingBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.FuelScoopingTopTextVerticalPosition)) settings.FuelScoopingTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.FuelScoopingBottomTextVerticalPosition)) settings.FuelScoopingBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.FuelScoopingBold)) settings.FuelScoopingBold = "true";
            if (string.IsNullOrEmpty(settings.PlanetApproachTopColor)) settings.PlanetApproachTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.PlanetApproachBottomColor)) settings.PlanetApproachBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.PlanetApproachTopTextVerticalPosition)) settings.PlanetApproachTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.PlanetApproachBottomTextVerticalPosition)) settings.PlanetApproachBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.PlanetApproachBold)) settings.PlanetApproachBold = "true";
            if (string.IsNullOrEmpty(settings.OrbitalCruiseTopColor)) settings.OrbitalCruiseTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.OrbitalCruiseBottomColor)) settings.OrbitalCruiseBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.OrbitalCruiseTopTextVerticalPosition)) settings.OrbitalCruiseTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.OrbitalCruiseBottomTextVerticalPosition)) settings.OrbitalCruiseBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.OrbitalCruiseBold)) settings.OrbitalCruiseBold = "true";
            if (string.IsNullOrEmpty(settings.DeorbitingTopColor)) settings.DeorbitingTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.DeorbitingBottomColor)) settings.DeorbitingBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.DeorbitingTopTextVerticalPosition)) settings.DeorbitingTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.DeorbitingBottomTextVerticalPosition)) settings.DeorbitingBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.DeorbitingBold)) settings.DeorbitingBold = "true";
            if (string.IsNullOrEmpty(settings.PlanetFlightTopColor)) settings.PlanetFlightTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.PlanetFlightBottomColor)) settings.PlanetFlightBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.PlanetFlightTopTextVerticalPosition)) settings.PlanetFlightTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.PlanetFlightBottomTextVerticalPosition)) settings.PlanetFlightBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.PlanetFlightBold)) settings.PlanetFlightBold = "true";
            if (string.IsNullOrEmpty(settings.LandedTopColor)) settings.LandedTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.LandedBottomColor)) settings.LandedBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.LandedTopTextVerticalPosition)) settings.LandedTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.LandedBottomTextVerticalPosition)) settings.LandedBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.LandedBold)) settings.LandedBold = "true";
            if (string.IsNullOrEmpty(settings.LiftoffTopColor)) settings.LiftoffTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.LiftoffBottomColor)) settings.LiftoffBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.LiftoffTopTextVerticalPosition)) settings.LiftoffTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.LiftoffBottomTextVerticalPosition)) settings.LiftoffBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.LiftoffBold)) settings.LiftoffBold = "true";
            if (string.IsNullOrEmpty(settings.LeavingPlanetTopColor)) settings.LeavingPlanetTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.LeavingPlanetBottomColor)) settings.LeavingPlanetBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.LeavingPlanetTopTextVerticalPosition)) settings.LeavingPlanetTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.LeavingPlanetBottomTextVerticalPosition)) settings.LeavingPlanetBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.LeavingPlanetBold)) settings.LeavingPlanetBold = "true";
            if (string.IsNullOrEmpty(settings.NoFireZoneTopColor)) settings.NoFireZoneTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.NoFireZoneBottomColor)) settings.NoFireZoneBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.NoFireZoneTopTextVerticalPosition)) settings.NoFireZoneTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.NoFireZoneBottomTextVerticalPosition)) settings.NoFireZoneBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.NoFireZoneBold)) settings.NoFireZoneBold = "true";
            if (string.IsNullOrEmpty(settings.StationApproachTopColor)) settings.StationApproachTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.StationApproachBottomColor)) settings.StationApproachBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.StationApproachTopTextVerticalPosition)) settings.StationApproachTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.StationApproachBottomTextVerticalPosition)) settings.StationApproachBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.StationApproachBold)) settings.StationApproachBold = "true";
            if (string.IsNullOrEmpty(settings.DockedAtStationTopColor)) settings.DockedAtStationTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.DockedAtStationBottomColor)) settings.DockedAtStationBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.DockedAtStationTopTextVerticalPosition)) settings.DockedAtStationTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.DockedAtStationBottomTextVerticalPosition)) settings.DockedAtStationBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.DockedAtStationBold)) settings.DockedAtStationBold = "true";
            if (string.IsNullOrEmpty(settings.OnFootInStationTopColor)) settings.OnFootInStationTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.OnFootInStationBottomColor)) settings.OnFootInStationBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.OnFootInStationTopTextVerticalPosition)) settings.OnFootInStationTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.OnFootInStationBottomTextVerticalPosition)) settings.OnFootInStationBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.OnFootInStationBold)) settings.OnFootInStationBold = "true";
            if (string.IsNullOrEmpty(settings.StationInteriorTopColor)) settings.StationInteriorTopColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.StationInteriorBottomColor)) settings.StationInteriorBottomColor = "#ffffff";
            if (string.IsNullOrEmpty(settings.StationInteriorTopTextVerticalPosition)) settings.StationInteriorTopTextVerticalPosition = "5";
            if (string.IsNullOrEmpty(settings.StationInteriorBottomTextVerticalPosition)) settings.StationInteriorBottomTextVerticalPosition = "190";
            if (string.IsNullOrEmpty(settings.StationInteriorBold)) settings.StationInteriorBold = "true";

            try
            {
                _supercruiseChargingTopBrush = ParseBrush(settings.SupercruiseChargingTopColor, Color.White);
                _supercruiseChargingBottomBrush = ParseBrush(settings.SupercruiseChargingBottomColor, Color.White);
                _fsdSupercruiseJumpingTopBrush = ParseBrush(settings.FsdSupercruiseJumpingTopColor, Color.White);
                _fsdSupercruiseJumpingBottomBrush = ParseBrush(settings.FsdSupercruiseJumpingBottomColor, Color.White);
                _supercruiseTopBrush = ParseBrush(settings.SupercruiseTopColor, Color.White);
                _supercruiseBottomBrush = ParseBrush(settings.SupercruiseBottomColor, Color.White);
                _supercruiseOverchargeTopBrush = ParseBrush(settings.SupercruiseOverchargeTopColor, Color.White);
                _supercruiseOverchargeBottomBrush = ParseBrush(settings.SupercruiseOverchargeBottomColor, Color.White);
                _supercruiseAssistTopBrush = ParseBrush(settings.SupercruiseAssistTopColor, Color.White);
                _supercruiseAssistBottomBrush = ParseBrush(settings.SupercruiseAssistBottomColor, Color.White);
                _normalSpaceTopBrush = ParseBrush(settings.NormalSpaceTopColor, Color.White);
                _normalSpaceBottomBrush = ParseBrush(settings.NormalSpaceBottomColor, Color.White);
                _hyperspaceChargingTopBrush = ParseBrush(settings.HyperspaceChargingTopColor, Color.White);
                _hyperspaceChargingBottomBrush = ParseBrush(settings.HyperspaceChargingBottomColor, Color.White);
                _fsdHyperspaceJumpingTopBrush = ParseBrush(settings.FsdHyperspaceJumpingTopColor, Color.White);
                _fsdHyperspaceJumpingBottomBrush = ParseBrush(settings.FsdHyperspaceJumpingBottomColor, Color.White);
                _fuelScoopingTopBrush = ParseBrush(settings.FuelScoopingTopColor, Color.White);
                _fuelScoopingBottomBrush = ParseBrush(settings.FuelScoopingBottomColor, Color.White);
                _planetApproachTopBrush = ParseBrush(settings.PlanetApproachTopColor, Color.White);
                _planetApproachBottomBrush = ParseBrush(settings.PlanetApproachBottomColor, Color.White);
                _orbitalCruiseTopBrush = ParseBrush(settings.OrbitalCruiseTopColor, Color.White);
                _orbitalCruiseBottomBrush = ParseBrush(settings.OrbitalCruiseBottomColor, Color.White);
                _deorbitingTopBrush = ParseBrush(settings.DeorbitingTopColor, Color.White);
                _deorbitingBottomBrush = ParseBrush(settings.DeorbitingBottomColor, Color.White);
                _planetFlightTopBrush = ParseBrush(settings.PlanetFlightTopColor, Color.White);
                _planetFlightBottomBrush = ParseBrush(settings.PlanetFlightBottomColor, Color.White);
                _landedTopBrush = ParseBrush(settings.LandedTopColor, Color.White);
                _landedBottomBrush = ParseBrush(settings.LandedBottomColor, Color.White);
                _liftoffTopBrush = ParseBrush(settings.LiftoffTopColor, Color.White);
                _liftoffBottomBrush = ParseBrush(settings.LiftoffBottomColor, Color.White);
                _leavingPlanetTopBrush = ParseBrush(settings.LeavingPlanetTopColor, Color.White);
                _leavingPlanetBottomBrush = ParseBrush(settings.LeavingPlanetBottomColor, Color.White);
                _noFireZoneTopBrush = ParseBrush(settings.NoFireZoneTopColor, Color.White);
                _noFireZoneBottomBrush = ParseBrush(settings.NoFireZoneBottomColor, Color.White);
                _stationApproachTopBrush = ParseBrush(settings.StationApproachTopColor, Color.White);
                _stationApproachBottomBrush = ParseBrush(settings.StationApproachBottomColor, Color.White);
                _dockedAtStationTopBrush = ParseBrush(settings.DockedAtStationTopColor, Color.White);
                _dockedAtStationBottomBrush = ParseBrush(settings.DockedAtStationBottomColor, Color.White);
                _onFootInStationTopBrush = ParseBrush(settings.OnFootInStationTopColor, Color.White);
                _onFootInStationBottomBrush = ParseBrush(settings.OnFootInStationBottomColor, Color.White);
                _stationInteriorTopBrush = ParseBrush(settings.StationInteriorTopColor, Color.White);
                _stationInteriorBottomBrush = ParseBrush(settings.StationInteriorBottomColor, Color.White);

                LoadImage(ref _notActiveImage, ref _notActiveFile, ref _notActiveImageIsGif, settings.NotActiveImageFilename);
                LoadImage(ref _supercruiseChargingImage, ref _supercruiseChargingFile, ref _supercruiseChargingImageIsGif, settings.SupercruiseChargingImageFilename);
                LoadImage(ref _fsdSupercruiseJumpingImage, ref _fsdSupercruiseJumpingFile, ref _fsdSupercruiseJumpingImageIsGif, settings.FsdSupercruiseJumpingImageFilename);
                LoadImage(ref _supercruiseImage, ref _supercruiseFile, ref _supercruiseImageIsGif, settings.SupercruiseImageFilename);
                LoadImage(ref _supercruiseOverchargeImage, ref _supercruiseOverchargeFile, ref _supercruiseOverchargeImageIsGif, settings.SupercruiseOverchargeImageFilename);
                LoadImage(ref _supercruiseAssistImage, ref _supercruiseAssistFile, ref _supercruiseAssistImageIsGif, settings.SupercruiseAssistImageFilename);
                LoadImage(ref _normalSpaceImage, ref _normalSpaceFile, ref _normalSpaceImageIsGif, settings.NormalSpaceImageFilename);
                LoadImage(ref _hyperspaceChargingImage, ref _hyperspaceChargingFile, ref _hyperspaceChargingImageIsGif, settings.HyperspaceChargingImageFilename);
                LoadImage(ref _fsdHyperspaceJumpingImage, ref _fsdHyperspaceJumpingFile, ref _fsdHyperspaceJumpingImageIsGif, settings.FsdHyperspaceJumpingImageFilename);
                LoadImage(ref _fuelScoopingImage, ref _fuelScoopingFile, ref _fuelScoopingImageIsGif, settings.FuelScoopingImageFilename);
                LoadImage(ref _planetApproachImage, ref _planetApproachFile, ref _planetApproachImageIsGif, settings.PlanetApproachImageFilename);
                LoadImage(ref _orbitalCruiseImage, ref _orbitalCruiseFile, ref _orbitalCruiseImageIsGif, settings.OrbitalCruiseImageFilename);
                LoadImage(ref _deorbitingImage, ref _deorbitingFile, ref _deorbitingImageIsGif, settings.DeorbitingImageFilename);
                LoadImage(ref _planetFlightImage, ref _planetFlightFile, ref _planetFlightImageIsGif, settings.PlanetFlightImageFilename);
                LoadImage(ref _landedImage, ref _landedFile, ref _landedImageIsGif, settings.LandedImageFilename);
                LoadImage(ref _liftoffImage, ref _liftoffFile, ref _liftoffImageIsGif, settings.LiftoffImageFilename);
                LoadImage(ref _leavingPlanetImage, ref _leavingPlanetFile, ref _leavingPlanetImageIsGif, settings.LeavingPlanetImageFilename);
                LoadImage(ref _noFireZoneImage, ref _noFireZoneFile, ref _noFireZoneImageIsGif, settings.NoFireZoneImageFilename);
                LoadImage(ref _stationApproachImage, ref _stationApproachFile, ref _stationApproachImageIsGif, settings.StationApproachImageFilename);
                LoadImage(ref _dockedAtStationImage, ref _dockedAtStationFile, ref _dockedAtStationImageIsGif, settings.DockedAtStationImageFilename);
                LoadImage(ref _onFootInStationImage, ref _onFootInStationFile, ref _onFootInStationImageIsGif, settings.OnFootInStationImageFilename);
                LoadImage(ref _stationInteriorImage, ref _stationInteriorFile, ref _stationInteriorImageIsGif, settings.StationInteriorImageFilename);
            }
            catch (Exception ex)
            {
                Logger.Instance.LogMessage(TracingLevel.FATAL, "OdysseyShipStatus InitializeSettings " + ex);
            }

            Connection.SetSettingsAsync(JObject.FromObject(settings)).Wait();
        }
    }
}
