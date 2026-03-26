# streamdeck-elite

This is a custom fork of [mhwlng's original project](https://github.com/mhwlng/streamdeck-elite) and [DrFr33ze's .NET 10 conversion](https://github.com/DrFr33ze/streamdeck-elite). I could not do any of this without all the hard work they put into the project! I'm an explorer at heart and have added new buttons and features that reflect how I play Elite Dangerous.

Latest Release: https://github.com/macrossmerrell/streamdeck-elite/releases

---

## Many New & Updated Features (v2.8.0)

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
 ![Explorer Button Additions](images/optional/explorerbuttons.png)

## Sample Ship Status / States  
 ![Ship ](images/optional/shipstatusstates.png)
 
---

## Optional Button Images

A set of custom button images is included in the `Images/Optional` directory, created using [Andechs75's Elite Dangerous icon PowerPoint template](https://github.com/Andechs75/Elite-Dangerous-Streamdeck-Icons/tree/master). These cover the Ship Status states and other common functions. Feel free to use, modify, or create your own using the same template.

> **Tip:** Use the PowerPoint template to design a button, take a snip, then crop in your favourite paint program to fit your Stream Deck button size.

---

## Automatic Profile Switching

The plugin supports automatic Stream Deck profile switching based on game state. To set this up:

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
