## Automatic Profile Switching Setup Instructions from CMDR Galyock

1. Create 4 profiles in Stream Deck software with these exact names in quotes (don't include the number sequence or quote marks, just the text inside the quotes); don't put in any buttons yet.  You should be able to ignore any you don't want the automatic switching profile behavior for that game state:
    - "Elite Main" -- default when you're sitting in your ship's cockpit
    - "Elite OnFoot" -- switches when you Disembark or go on foot in Odyssey
    - "Elite InSRV" -- switches when you deploy your SRV
    - "Elite InFighter" -- switches when you're in your ship-launched fighter

1. OPTIONAL:  If you wish to use some of the original plugin's automatic profile switching functionality, create those additional profiles following the documented naming convention (e.g., "Elite STATENAME" where "STATENAME" is the in-game state value pulled from the list on the original plugin's Wiki.  For example, "Elite InvAnalysisMode Hardpoints" is a profile that would (allegedly? haven't tested...) get auto-switched to if you're NOT in analysis mode and deploy your hard points.  Read the original Wiki instructions for details.

1. Create a generic profile (name doesn't matter), set it as the default / active profile within Stream Deck, and drag any one button from the Elite StreamDeck plugin to an available slot

1. Export from Streak Deck each "Elite" profile you created in Step 1 (and Optional Step 2) as a .streamDeckProfile to the root folder of the plugin's directory:   
```js
%appdata%\Elgato\StreamDeck\Plugins\com.mhwlng.elite.sdPlugin
```



5. In StreamDeck, delete the profiles you just exported

1. If you are using "old style" profiles from Optional Step 2 above, see the "Editing manifest.json" section below then come back here and continue with the next step

1. Close the StreamDeck configuration window then quit StreamDeck software completely -- click the ^ near the right corner of the Windows Task Bar, look for the StreamDeck icon, right-click it and choose "Quit completely"

1. Start Elite Dangerous and load into the game (whether in your cockpit, on foot, in an SRV, etc)

1. Open the StreamDeck software; in a few seconds you should see a popup prompt about installing profiles; click "Install Profiles"

1. It likely only installed one profile, corresponding to the current game state you're in (e.g., in your cockpit vs on foot etc).  If so, you'll need to repeat Steps 7 through 9 for each remaining game state automatic switching profile you created in Step 1 and Optional Step 2.  You don't need to keep quitting & reopening the game, you just need to change the game state before reopening Stream Deck each time.

1. Once you've imported all the profiles you wanted for automatic profile switching, you can start adding buttons and customizing them in Stream Deck.  For extra safety, you should probably export the profiles once you're satisfied and save them someplace safe.

## Editing manifest.json

You only need to edit this file to include additional "original plugin" profiles you created in Optional Step 2.  If you're only using the 4 profiles created in Step 1, you don't need to do anything further -- ignore this section (and by default, the one below it).

1. In NotePad or similar plain text editor, edit the following file:  
```js
%appdata%\Elgato\StreamDeck\Plugins\com.mhwlng.elite.sdPlugin\manifest.json
```
2. Scroll down the file until you see:
```
"Profiles": [    {
          "Name":"AnyProfileNameHere",
          "ReadOnly":value,
          "DeviceType":value,
          "DontAutoSwitchWhenInstalled":value
        },
        ...more profiles...
        ]
```

3. Click just before that closing square bracket (after all the listed profiles), then paste in / insert 1 "entry" for each of your additional profiles (using the exact profile name you created in Optional Step 2) using the format noted below in the section "manifest.json Profile Entry Format", adding a comma between each entry as necessary
4. Save the file when you're done then go back to Step 7 of the Automatic Profile Switching Setup Instructions

## manifest.json Profile Entry Format

Ignore this section unless you created "old style" automatic switching profiles in Optional Step 2.

1. Find the closing bracket of the "Profiles" section and type a single comma just before it:  ,
2. Copy the text below from the "Code" section, one "entry" for each additional profile, and paste it after that comma you just added but before the Profile section's closing square bracket ]
3. Change the Name value to match the automatic switching profile name you're adding
4. Change the DeviceType number to match your physical StreamDeck hardware device.  In my case, 0=classic.  If I recall, the other device type number codes are spelled out either directly in manifest.json, or in the pluginlog.log file (it's in the same folder as manifest.json).
5. If you're adding more than 1 additional profile, add a comma after the closing squiggly bracket for each entry you've added, EXCEPT for the last one.
6. Be careful not to delete or overwrite the closing square bracket of the "Profiles" section.
7. Repeat steps 2 through 6 for each additional automatic switching profile you created in Optional Step 2 under the Automatic Profile Switching Setup Instructions.


Profile Entry, 1 of these for each automatic switching profile you're adding:

```
{

      "Name”:”AdditionalProfileName1”,

      "ReadOnly":false,

      "DeviceType":YourDeviceHardwareNumber,

      "DontAutoSwitchWhenInstalled":false

    }
```

Your final Profile section inside manifest.json should look something like this:

```
"Profiles": [

    {

      "Name":"Elite Main",

      "ReadOnly":false,

      "DeviceType":2,

      "DontAutoSwitchWhenInstalled":false

    },

    {

      "Name":"Elite OnFoot",

      "ReadOnly":false,

      "DeviceType":2,

      "DontAutoSwitchWhenInstalled":false

    },

    {

      "Name":"Elite InSRV",

      "ReadOnly":false,

      "DeviceType":2,

      "DontAutoSwitchWhenInstalled":false

    },

    {

      "Name":"Elite InFighter",

      "ReadOnly":false,

      "DeviceType":2,

      "DontAutoSwitchWhenInstalled":false

    },

    {

      "Name":"YourAdditionalProfileName",

      "ReadOnly":false,

      "DeviceType":YourStreamDeckHardwareNumber,

      "DontAutoSwitchWhenInstalled":true

    }

  ]
```