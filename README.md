# streamdeck-elite

This is a custom fork of [mhwlng's original project](https://github.com/mhwlng/streamdeck-elite) and [DrFr33ze's .NET 10 conversion](https://github.com/DrFr33ze/streamdeck-elite). I could not do any of this without all the hard work they put into the project! I'm an explorer at heart and have added new buttons and features that reflect how I play Elite Dangerous.

**Ensure .NET 10 Desktop Runtime is installed** for proper operation: https://dotnet.microsoft.com/en-us/download/dotnet/10.0

## ⚠️ Important — EliteJournalReader Library Upgrade (v4.0.0)

**If you've forked this project and added your own buttons or features, please read this before updating.**

This release upgrades the plugin's underlying `EliteJournalReader` library (the piece that reads Elite Dangerous's journal and status files) to the current upstream version, targeting .NET 10. This brings access to newer game data — including flags for Supercruise Overcharge — but it also changes some core APIs that custom buttons commonly plug into. If your fork adds its own buttons, there's a good chance you'll need to update them. Thank you [MagicMau](https://github.com/MagicMau/EliteJournalReader) for all your hard work!

Latest Release: https://github.com/macrossmerrell/streamdeck-elite/releases

---
## New & Updated Features (v4.0.0)

### 🆕 Replacement of outdated EliteJournalReader - Thank you MagicMau!
This project now incorporates the latest Odyssey Flags/Flags2 events!
- Buttons have been updated to work with 'Program.JournalWatcher.MessageReceived' events.
- Addresses changes in flag names and incorrect values in the originally packaged EliteJournalReader.
- Build environment bump, using a new Newtonsoft.Json package for .NET 10.

### 🧑‍🚀 🆕 (NEW) On Foot Exploration Button
A cycling on-foot status button for surface exploration. Only active while on foot on a planet surface — automatically shows an "Inactive" image the rest of the time (in your ship, in a station, etc.).

![On Foot Button Example](https://github.com/macrossmerrell/streamdeck-elite/blob/b8ddf42be943f6240e54219dfcef30688d53d651/Elite/Images/Examples/Onfootbutton.png)

**Displays (6 configurable options):**
- **Oxygen** — current suit oxygen level as a percentage
- **Health** — current suit health as a percentage
- **Temperature** — live ambient temperature reading in Kelvin
- **Gravity** — live gravity reading in g at your current location
- **Atmosphere** — shows whether the local atmosphere is breathable
- **Thermal Status** — shows your current thermal state: Safe, Cold, Very Cold, Hot, or Very Hot

**Cycling behaviour:**
- Pressing the button advances to the next enabled option (wraps back to the start)
- **Auto Rotate** mode advances through enabled options automatically on a configurable timer (1–120 seconds)
- Button starts on Oxygen by default

**Every option is independently configurable - Can be used to create multiple buttons for static readouts:**
- Enabled / Disabled toggle
- Custom label text, label position, and label color
- Value position and color (Oxygen, Health, Atmosphere, and Thermal default to the Edge position to avoid overlapping the label)
- Background image
- Bold text toggle

**Oxygen & Health warnings:**
- Each has its own warning color *and* its own warning image, triggered automatically by the game's own LowOxygen/LowHealth flags — the button can visually transform (not just change color) the moment either drops critical
- Falls back to the normal image if no warning image is set

**Atmosphere — grouped by state:**
- Two fully independent states: **Breathable** and **No Oxygen**
- Each state has its own text, its own image, and its own color, grouped together in settings so it's clear which belongs to which
- The correct state is detected automatically and updates live as you move between locations

**Thermal Status — grouped by state:**
- Five fully independent states: **Safe, Cold, Very Cold, Hot, Very Hot**
- Each state has its own label text, its own image, its own label color, *and* its own temperature color — so both the state name and the live temperature reading can be color-matched (or contrasted) to whichever image is showing
- The state name is shown as the label (position configurable, defaults to Very Top); the live temperature reading (`123°K` format) is shown as the value below it (position configurable, defaults to Edge)
- Falls back to the Safe-state image for any state that doesn't have its own image set

**Additional settings:**
- Inactive Image — shown whenever you're not on foot on a planet surface
- Click sound

### 🗺️ 🆕 (NEW) Advanced Route Button
A cycling route information button designed for long-distance exploration. Displays one stat at a time and advances through your enabled options on button press or automatically on a timer.

![Advanced Routing Button Example](https://github.com/macrossmerrell/streamdeck-elite/blob/b8ddf42be943f6240e54219dfcef30688d53d651/Elite/Images/Examples/AdvancedRouteButton.png)

**Displays (6 configurable options):**
- **Jumps Remaining** — whole number of jumps left in the active route
- **Destination** — total light year distance from your current position through every remaining waypoint to your final destination
- **Next System** — straight-line distance in light years to the next system in your route
- **Trip Progress** — percentage of the route completed, based on jumps done vs. total jumps when the route was plotted
- **Fuel Status** — current main tank fuel level in tons
- **Jump Range** — estimated current jump range in light years, calculated from your ship's base range and current fuel/cargo mass

**Cycling behaviour:**
- Pressing the button advances to the next enabled option (wraps back to the start)
- **Auto Rotate** mode advances through enabled options automatically on a configurable timer (1–120 seconds)
- Button starts on Jumps Remaining by default

**Persistence:**
- Trip Progress is saved to disk and survives plugin and game restarts — progress is correctly restored even after relaunching and reopening the Galaxy Map mid-trip
- Route waypoints and Jump Range baseline are backfilled from your journal files on startup, so all options display correct values immediately without needing to replot your route or visit Outfitting

**Each option is independently configurable - Can be used to create multiple buttons for static readouts:**
- Enabled / Disabled toggle
- Custom label text (shown at top of button, e.g. rename "Destination" to whatever makes sense to you)
- Label text position and color
- Value position and color
- Background image
- Bold text toggle

**Additional settings:**
- No Route Image — shown when no route is active
- Click sound and disabled sound

> **Note:** Jump Range is an estimate based on your ship's unladen mass and base jump range, scaled by current fuel and cargo weight. It updates in near real-time as you burn fuel. It does not account for FSD engineering modifiers, but it's close enough :smiley:.

### 🆕 (NEW) Odyssey Ship Status Button

Replaces the old Ship Status Button (now deprecated) with a fully customizable version: every flight/ship state gets its own optional image plus independent top and bottom text, each with its own color and vertical position. Leave the text fields blank on any state to use it as a pure image button, exactly like before.

**States covered (21 total):**
Supercruise Charging, Supercruise Activation, Supercruise (Active), Supercruise Overcharge, Supercruise Assist, Normal Space, Hyperspace Charging, Hyperspace Jump, Fuel Scooping, Planet Approach, Orbital Cruise, Glide/Deorbit, Planetary Flight, Landed, Liftoff, Leaving Planet, No-Fire Zone, Station Approach, Docked at Station, On Foot In Station, and Station Interior.

**New this version:**
- Supercruise Overcharge and Supercruise Assist are now tracked as their own states — while either is active, the button shows whatever you've configured for it instead of plain Supercruise, and drops back the moment it ends.
- Not Active Image — a global fallback image shown whenever the current state doesn't have its own image configured, so the button never falls back to a blank/generic Stream Deck key.

**Settings per state:**
- Image (optional)
- Top Text, Top Text Color, Top Text Position
- Bottom Text, Bottom Text Color, Bottom Text Position
- Bold (applies to both text fields for that state)

**Bug fixes carried over from the original Ship Status Button:**
- Station Approach was previously unreachable (always shadowed by No-Fire Zone) — fixed so it displays correctly once docking is granted.
- Fixed a case where climbing away from a planet without a full landing/liftoff cycle (e.g. pulling up straight out of Planet Approach) would incorrectly show Supercruise (Active) instead of Leaving Planet.

### ✏️ Updated: Planet Info Button — Atmosphere Display

The atmosphere display on the Planet Info button has been improved to handle a wider range of atmosphere types more accurately.

![Full Atmosphere Text Example](https://github.com/macrossmerrell/streamdeck-elite/blob/b8ddf42be943f6240e54219dfcef30688d53d651/Elite/Images/Examples/planetinfo.png)

**Hot / Thin prefix handling:**
Atmosphere descriptions that include temperature or pressure prefixes (e.g. `"hot sulfur dioxide atmosphere"`, `"thin carbon dioxide atmosphere"`) now display the prefix as part of the button text rather than being stripped and discarded. For example, a Hot Sulfur Dioxide atmosphere now displays as:

```
HOT
SULPHUR
DIOXIDE
```

**Three-line layout with auto-sizing:**
The button now supports up to three lines of atmosphere text. Font size automatically scales down to ensure all three lines fit within the top half of the button, keeping the temperature reading clear and unobstructed.

**Rich atmosphere types:**
Atmosphere types with a "rich" qualifier (Neon Rich, Argon Rich, Water Rich, Methane Rich, Ammonia Rich, Carbon Dioxide Rich) now include the RICH label in the display rather than showing only the base gas name.

**Expanded atmosphere coverage:**
Added dictionary entries for several rare atmosphere types that previously fell through to a generic fallback display: Water Rich, Methane Rich, Ammonia Rich, Ammonia and Oxygen, and Suitable for Water-Based Life.

### 🗑️ Removed: (Deprecated) Hyperspace Button

The old `(Deprecated) Hyperspace Button` has been removed from the plugin entirely. It's been superseded by the **Ship Status Button** (and now the new **Odyssey Ship Status Button**), which cover the same states more reliably.

If you still have this button placed on a Stream Deck profile, it will show up as unrecognized/blank after updating — just remove it and replace it with the Ship Status or Odyssey Ship Status button instead.

## New & Update Features (v3.2.0)

### 🆕 (NEW) Heading / Altitude Button
Displays your current heading (top) and altitude (bottom) while flying near a planetary body in Elite Dangerous.
Heading is shown in degrees (e.g. 270°), matching the compass reading in the ED HUD.
Altitude auto-scales based on height:

Below 3,000m — displays in meters (e.g. 1250m)
At or above 3,000m — displays in kilometers (e.g. 12.4km)

The button only activates when the game reports a valid planetary position, so it stays blank in deep space or while docked.
Settings:

Near Planet Image — background image shown when planetary data is active
Not Active Image — background image shown when no planetary data is available
Heading Color — text color for the heading display
Heading Position — vertical position of the heading text on the button
Altitude Color — text color for the altitude display
Altitude Position — vertical position of the altitude text on the button
Bold — toggle bold text on or off

 ![Heading and Altitude Example](https://github.com/macrossmerrell/streamdeck-elite/blob/59a524d1a1d7b0db09d36c955ec8f0ab7fa6cacd/Elite/Images/Examples/altandhead.png)

### ✏️ (Updated) Static Button
Added missing features to the original Static Button:

**Combat group:**
- Shield Cell — Activates a shield cell bank
- Deploy Heatsink — Ejects a heatsink to shed heat instantly
- Chaff — Fires a chaff launcher to break missile lock

**Ship group:**
- Silent Running — Toggles silent running on/off

Note: Silent Running also appears in the Toggles group with explicit ON and OFF variants if you prefer directional control over a toggle.

## New & Update Features (v3.1.0)

### 🔭 🆕 (NEW) Galaxy Search Button
Opens a web browser to an external database page for your current location. Automatically detects context — docked at a station, approaching a body, or just flying through a system — and opens the most relevant page.
Priority logic:

Docked at a station → opens the station page
Approaching a body → opens the system page (body context)
Default (always available) → opens the star system page

Supported services (selectable per button):
- INARA.cz
- EDSM.net
- Spansh.co.uk

Setting Options: 
- Service selector
- Optional background image
- Separate color and position controls for the service name and location label
- Bold text toggle.

Fun note:
You can place multiple Galaxy Search buttons side by side, each set to a different service, for one-press access to all three databases.

### 🆕 Updated: Exobiology Button
The Exobiology button assists with biological scanning in Elite Dangerous Odyssey by tracking your progress through the three-scan sequence required to fully record each organism.
Scan Progress

Once you take your first biological sample, the button displays the genus (and species once identified) and begins tracking your distance from the scan point. Three pip indicators show your progress through the three-scan sequence at a glance.

**Distance Tracking:**
The meter displays how far you are from the nearest scan point, starting at 0m and counting up as you move away. If you change direction the meter counts back down. Once you've taken your second sample, both scan locations are tracked simultaneously — if you drift back toward either previous scan point the meter will count down toward it, warning you that you may be entering an overlapping colony range.

**Zone Indicators:**
The button background changes across four configurable zones based on how much of the colony range you've covered from the nearest scan point:

Zone A (0–20%) — Too close, keep moving
Zone B (21–70%) — Moving away
Zone C (71–99%) — Almost far enough
Zone D (≥100%) — Ready to scan, meter clears

**Persistence:**
The button stays active across all game states — on foot, in your ship, in an SRV — so you can monitor your position while flying back to land or repositioning between samples. Scan state and coordinates are restored automatically when the Stream Deck software restarts, even across game sessions, so you won't lose your place mid-sequence. State clears automatically when organic data is sold.

**Customization:**
Each zone has fully configurable background images, text colors, text positions, and pip appearance, giving you complete freedom to design the button around your own artwork and layout preferences.


 ![Exobiology Example](https://github.com/macrossmerrell/streamdeck-elite/blob/d25031eb1ae25039aa0389b0475f9ae3cb868295/Elite/Images/Examples/Bioscan.png)

## New & Update Features (v3.0.0)

### ✏️ Updated: Planet Info Button now activates when a planet is targeted.

### ✏️ Updated: Gravity Button

- Targeted planet shows FSS scan gravity information (no longer just in orbit / on planet), and still updates in realtime during deorbit.
- Offers a High Gravity Warning State that is configurable based on your ship's capabilities, which defaults to 1.0G.  Offers custom button image settings to make a visually distinct warning.
- Targeted Planet Gravity and High Gravity Warning states have independent text color settings to work with custom images / backgrounds.

![Gravity Example](https://github.com/macrossmerrell/streamdeck-elite/blob/d25031eb1ae25039aa0389b0475f9ae3cb868295/Elite/Images/Examples/GravitySample.png)


## New & Update Features (v2.9.0)

### 🆕 Navigation Target Info Button
After completing a System Scan, the button displays information on a currently selected planet — including additional notificiations on landable planets that have scannable bioligical or geological features, along with notifying if terraformable.

- Supports Active (targeted) and Inactive (no target) background images
- Supports any text color for both Planet Type and Biology / Geology / Terrafromable text.
- Supports button text placement with 11 selectable positions
- Provides option for bold text

### 🆕 Latitude & Longitude Button
Displays current planetary Latitude and Longitude in Ship, SRV, Fighter, and on foot — updates when Elite outputs new coordinates.

- Supports Near Planet and Not Active background images
- Supports any text color for both Latitude and Longitude informational text
- Support button text placement with 11 selectable positions
- Provides option for bold text

![Latitude & Longitude](https://github.com/macrossmerrell/streamdeck-elite/blob/d25031eb1ae25039aa0389b0475f9ae3cb868295/Elite/Images/Examples/latlong.png) 

## New & Update Features (v2.8.0)

### 🆕 Ship Status Button
Displays the current flight state of your ship as a single image that automatically updates as your situation changes. Each state has its own configurable image.

**Supported states (in priority order):**
- Hyperspace Charging
- Hyperspace Jump
- Supercruise Charging
- Supercruise Activation
- Supercruise (Active)
- Normal Space
- Fuel Scooping
- Planet Approach
- Orbital Cruise
- Glide / Deorbiting
- **Leaving Planet** *(new — triggers after LeaveBody event, bounces with Planet Approach based on altitude)*
- Planetary Flight
- Landed
- Liftoff *(displays for 2.5 seconds after liftoff)*
- No-Fire Zone (beta)
- Station Approach (beta)
- Docked at Station (beta)
- Station Interior / On Foot in Station (beta)

The button intelligently handles the full departure and approach sequence — including orbital cruise on the way up, leaving planet after clearing the orbital altitude, and correctly bouncing between Leaving Planet and Planet Approach if you change direction. Works whether you physically lifted off from the surface or just flew up from planetary flight.

---

### 🆕 Planetary Gravity Button
Displays real-time planetary gravity based on your current altitude using the inverse square law:

`g(alt) = surfaceG × (planetRadius / (planetRadius + altitude))²`

- Shows live altitude-adjusted gravity when near a planet (`HasLatLong`)
- Shows cached surface gravity when targeting a scanned planet from supercruise
- Falls back to `?g` for unscanned planets
- Planet scan data is backfilled from recent journal files on startup (bast 10)  — works even after a fresh game session

---

### 🆕 Planet Info Button
Displays atmosphere type and surface temperature for the current planet.

- **Atmosphere** — full capitalized name, split across two lines for multi-word types (e.g. CARBON / DIOXIDE)
- **Temperature** — surface temperature in Kelvin from scan data; switches to live real-time temperature when on foot
- Separate color, position, and bold settings for atmosphere and temperature text — allows for any custom background image
- Auto-scaling text fills the button width for maximum readability — limited size to avoid over-sizing

**Supported atmosphere types:** Silicate Vapour, Oxygen, Ammonia, Nitrogen, Methane, Argon, Water, Sulphur Dioxide, Neon, Carbon Dioxide, Helium, Metallic Vapour, and more

---

### 🆕 Alert Button
A single button that monitors multiple danger conditions simultaneously and cycles through active alerts every 2 seconds.

**Monitored alerts (in priority order):**
1. **Self Destruct** — 30s timer
2. **Cockpit Breached** — 20s timer
3. **Systems Shutdown** — 10s timer
4. **Jet Cone Damage** — 5s timer
5. **Heat Warning** — clears when overheating flag clears
6. **Heat Damage** — 5s timer, clears when overheating ends
7. **Hull Damage** — 5s timer
8. **Shields Down** — clears when shields restore
9. **Under Attack** — 4s timer
10. **Being Interdicted** — clears when interdiction ends
11. **Is In Danger** — clears when danger flag clears
12. **Low Fuel** — clears when fuel restored
13. **Docking Denied** — 5s timer

Each alert has its own configurable image, text, text color, text position, bold setting, and timeout duration. Image must be set for alerts to work.
**Press the button to manually dismiss all active alerts.**

When no alerts are active, the button shows a configurable default state (image + text).  

---

### ✏️ Updated: Route Button
- Larger auto-scaling font
- Bold text option
- Vertical position adjustment to better fit custom button graphics

### ✏️ Updated: Toggle Button
- Added **Genetic Sampler** option — shows button state change when the sampler is deployed or stowed, useful for explorers

---
### New Explorer Buttons Information
 ![Explorer Button Additions](https://github.com/macrossmerrell/streamdeck-elite/blob/d25031eb1ae25039aa0389b0475f9ae3cb868295/Elite/Images/Examples/explorerbuttons.png)

## Sample Ship Status / States  
 ![Ship Status](https://github.com/macrossmerrell/streamdeck-elite/blob/d25031eb1ae25039aa0389b0475f9ae3cb868295/Elite/Images/Examples/shipstatusstates.png)
 
---

## Optional Button Images

A set of custom button images is included in the `Images/Optional` directory, created using [Andechs75's Elite Dangerous icon PowerPoint template](https://github.com/Andechs75/Elite-Dangerous-Streamdeck-Icons/tree/master). These cover the Ship Status states and other common functions. Feel free to use, modify, or create your own using the same template.

> **Tip:** Use the PowerPoint template to design a button, take a snip, then crop in your favourite paint program to fit your Stream Deck button size.

---

## Automatic Profile Switching

The plugin supports automatic Stream Deck profile switching based on game state. To set this up:

### Follow the instructions from CMDR Galyock
[Automatic Profile Switching Setup](https://github.com/macrossmerrell/streamdeck-elite/blob/master/Automatic%20Profile%20Switching%20Instructions.md).

1. Create profiles in the Stream Deck software with these exact names:
   - `Elite Main` — default ship profile
   - `Elite OnFoot` — switches when on foot in Odyssey
   - `Elite InSRV` — switches when SRV is deployed
   - `Elite InFighter` — switches when in a fighter
2. Export each profile from Stream Deck software as a `.streamDeckProfile` file
3. Place the exported files in the `Profiles` folder inside the plugin directory
4. Reinstall the plugin — Stream Deck will prompt to import the profiles

> **Note:** Profile files are device-specific and tied to your hardware UUID. They cannot be shared universally, which is why this folder ships empty. This is an advanced setup for users who want it.

More information on the [original wiki](https://github.com/mhwlng/streamdeck-elite/wiki/Automatic-Profile-Switching).

---

## Installation

Download the latest `com.mhwlng.elite.streamDeckPlugin` from the [Releases page](https://github.com/macrossmerrell/streamdeck-elite/releases) and double-click to install.

> If the plugin is already installed, uninstall it first (right-click any button → Uninstall), then reinstall.

The plugin installs to:
```
%appdata%\Elgato\StreamDeck\Plugins\com.mhwlng.elite.sdPlugin
```

**Before uninstalling**, save any custom images or profiles you have stored in the plugin directory.

### Updating

1. Stop the Stream Deck application
2. Delete (or back up) the `com.mhwlng.elite.sdPlugin` directory
3. Restart Stream Deck
4. Double-click the new `.streamDeckPlugin` file to install

### Troubleshooting

If buttons aren't responding, check the `pluginlog.log` file in the plugin directory. A common issue is missing keyboard bindings:

```
file not found C:\Users\xxx\AppData\Local\Frontier Developments\Elite Dangerous\Options\Bindings\Custom.4.2.binds
```

If you see this, try running `StreamDeck.exe` as administrator.

**All bindings must be 'custom'** — this happens automatically once you make at least one on-foot keyboard binding. Default binding names will cause the plugin to not work correctly.

---

## Original Plugin Documentation

The sections below are from the original plugin and remain relevant.

---

Elgato Stream Deck button plugin for Elite Dangerous

![Elgato Stream Deck and Flight Instrument Panel](https://i.imgur.com/bE2ODlF.jpg)

This plugin connects to Elite Dangerous, to get the on/off status for 14 different toggle-buttons, 
4 buttons to control the power distributor pips, 4 alarm buttons, 3 FSD related buttons, an FSS toggle button, a firegroup selection button and a generic limpet controller button.

If you press the relevant button on your keyboard or hotas, then the image on the stream deck will change correctly.

When a button has no effect (e.g. when docked) then the image won't change.

There is also a STATIC button type, that works in a similar way to the streamdeck 'Hotkey' button type.
So, there is only one image and there is no game state feedback for these buttons.
The differences with the 'Hotkey' buttons are, that it gets the keyboard binding from the game and doesn't repeat the key when the streamdeck button is held.
For Odyssey, various new buttons are available here.

A sound can be played when pressing a static button.

The static buttons can also be used with multi-action buttons.

The static buttons under the 'Toggles' and 'Fire Group' groups (like Combat Mode or Deploy Hardpoints) are meant for multi-actions. 
These kinds of buttons should not be pressed multiple times in quick succession, because it takes some time for the plugin to receive the game state change.

There is also a 'Repeating Static Button' type. This button is used only for keys, that need to be held down.
So, when the stream deck button is pushed, the 'key down' event is sent to the keyboard
and only after the stream deck button is released, the 'key up' event is sent to the keyboard.
The streamdeck 'hotkey' button also has this behaviour.
For Odyssey, the 'Open Access Panel' button is available here.

The plugin also has a Dial button for use with the 4 dials on the Streamdeck+ model.

There are 5 bindings (They must be keyboard bindings, you can't bind the mouse wheel!) :

- Dial Clockwise
- Dial Counter-Clockwise
- Dial Press
- Touch screen press
- Touch screen long press

When a dial is rotated, the 'key down' event is sent to the keyboard once. 
When you let go of the dial for at least 100ms : the 'key up' event is sent to the keyboard. 

When a dial button is pushed, the 'key down' event is sent to the keyboard. 
When a dial button is released, the 'key up' event is sent to the keyboard. 

When the touch screen is pressed or long-pressed, the behaviour is like the static button.

The plugin also has a Firegroup Dial button for use with the 4 dials on the Streamdeck+ model.

After you install the plugin in the streamdeck software, then there will be several new button types in the streamdeck software.

Choose a button in the streamdeck software (drag and drop), then choose an Elite Dangerous function for that button (that must have a keyboard binding in Elite Dangerous!) and then choose any pictures for that button.

**Example button images, like in above picture, can be found in the source code images directory.**

ONLY add an image to a (repeating) STATIC and DIAL button in this way, do NOT set this image for any of the other button types :

![Button Image](https://i.imgur.com/xkgy7uZ.png)

Animated gif files are only supported for the (repeating) STATIC and DIAL buttons. Dial images are 200x50

If .gif images are configured for Power/Limpet/Hyperspace/Route buttons, then no texts or pips are drawn on top of them.

**You can clear the image/sound path, by clicking on the label in front of the file picker edit box.**

Supported devices: Stream Deck Classic, Mini, XL, Mobile, Plus and **Neo**.

The supported toggle-buttons are:
- Analysis Mode
- Cargo Scoop
- Flight Assist
- Galaxy Map
- Hardpoints
- Landing Gear
- Lights
- Night Vision
- Silent Running
- SRV Drive Assist
- SRV Handbrake
- SRV Turret
- Supercruise (no longer needed)
- System Map
- Comms Panel
- Nav Panel
- Role Panel
- Systems Panel

For Odyssey, when On Foot, the Galaxy Map, System Map, Lights & Night Vision buttons will call the on-foot key bindings, 
but there is no state feedback. So the button image won't change.

The supported power distributor pips buttons are:
- Reset
- System
- Engines
- Weapons

A long press on a button will set the power distributor to 4 pips.

The supported FSD related buttons are:
- Toggle FSD, also shows Remaining Jumps In Route
- Supercruise
- Hyperspace Jump, also shows Remaining Jumps In Route
- Route, also shows Remaining Jumps In Route

The plugin looks for a StartPreset.start file in this Elite Dangerous key bindings directory:

`%LocalAppData%\Frontier Developments\Elite Dangerous\Options\Bindings\`

This plugin only works with keyboard bindings. When there is only a binding to a joystick / controller / mouse for a function, you need to add a keyboard binding.

---

## Credits & Thanks

- [mhwlng/streamdeck-elite](https://github.com/mhwlng/streamdeck-elite) — original plugin
- [DrFr33ze/streamdeck-elite](https://github.com/DrFr33ze/streamdeck-elite) — .NET 10 conversion and Neo support
- [BarRaider/streamdeck-tools](https://github.com/BarRaider/streamdeck-tools)
- [MagicMau/EliteJournalReader](https://github.com/MagicMau/EliteJournalReader)
- [ishaaniMittal/inputsimulator](https://github.com/ishaaniMittal/inputsimulator)
- [Andechs75 — Elite Dangerous Stream Deck Icons](https://github.com/Andechs75/Elite-Dangerous-Streamdeck-Icons/tree/master) — fantastic button icon set and PowerPoint template
- [nerdordie.com](https://nerdordie.com/product/stream-deck-key-icons/)

Also see companion application for Logitech Flight Instrument Panel and VR:
https://github.com/mhwlng/fip-elite
