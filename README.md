# Voicemeeter Integration
VoiceMeeter integration and live feedback for the Elgato Stream Deck device.

**Author's website and contact information:** [https://barraider.com](https://barraider.com)

## New in v1.9
- :new: **MacroButton support!** Toggle VoiceMeeter Macro Buttons from the Stream Deck
    - Supports both "Toggle" mode and "PTT" mode.
- `Mute/Unmute` action now supports sending hotkeys
- Core improvements and bug fixes

### Download:
https://github.com/BarRaider/streamdeck-voicemeeter/releases/

## Current functionality
### 6 Plugins built into one:
#### VoiceMeeter Mute/Unmute
- Allows you to easily connect to one of VoiceMeeter's Strips or Buses
- 3 different modes: Toggle/Push-To-Talk/Single Setting (on/off)
- See a live indication of the current status on Stream Deck (never forget your microphone on again!)
- Can also be used to mute/unmute different Strips/Buses such as Spotify/Background music/etc.
- Choose from 4 different icons to display the mute/unmute settings
- Choose your own images to display, instead of the 4 pre-defined icons

#### VoiceMeeter Modify Setting
- Allows you to easily modify various VoiceMeeter settings
- Supports a whole list of options for each Strip/Bus
	* Options include modifying the: Gain slider, gate, comp,  mono button, solo button, audibility, color_x, color_y, eqgain1, eqgain2, eqgain3, fx_x, fx_y, mc,pan_x, pan_y
	(Valid values can be found starting on page 9 of VoiceMeeter API PDF: https://download.vb-audio.com/Download_CABLE/VoicemeeterRemoteAPI.pdf )
- Live feedback on the current value of that setting
- Supports both Press and Long Press (allows you to toggle between two preset values for this setting)
- Option to turn off the Live feedback and set the title to whatever you want (including a prefix using the `TitlePrefix` parameter)

#### VoiceMeeter Advanced Press/Long-Press
- **Note:** This is for advanced users but there are explanations under the ***Fields explained*** section below
- Allows you to directly modify a whole set of settings
- Example: `Strip[0].mono=1;Strip[1].Mute=1;Bus[2].Gain=-20;`
	* Additional examples can be found on the VoiceMeeter forum: https://forum.vb-audio.com/viewtopic.php?f=8&t=346&sid=a773394396c10847fd6fd7e332a55e49#p495
	and the VoiceMeeter API PDF: https://download.vb-audio.com/Download_CABLE/VoicemeeterRemoteAPI.pdf
- Supports both Press and Long Press (allows you to change between two preset list of settings)
- Live feedback on whatever setting you choose
- Option to turn off the Live feedback and set the title to whatever you want (including a prefix using the `TitlePrefix` parameter)

#### VoiceMeeter Advanced Toogle
- **Note:** This is for advanced users but there are explanations under the ***Fields explained*** section below
- Focused on toggling between two modes (versus press and long press in the VoiceMeeter Advanced Press/Long-Press)
- Mode1 should always turn things ON and Mode2 should turn things OFF
- Example: `Strip[0].mono=1;Strip[1].Mute=1;Bus[2].Gain=-20;`
	* Additional examples can be found on the VoiceMeeter forum: https://forum.vb-audio.com/viewtopic.php?f=8&t=346&sid=a773394396c10847fd6fd7e332a55e49#p495
	and the VoiceMeeter API PDF: https://download.vb-audio.com/Download_CABLE/VoicemeeterRemoteAPI.pdf
- Supports toggling between two preset list of settings
- Supports different user-defined icons for each preset
- Live feedback on whatever setting you choose
- Option to turn off the Live feedback and set the title to whatever you want (including a prefix using the `TitlePrefix` parameter)

#### VoiceMeeter Advanced PTT
The `Advanced PTT` action allows you to set a bunch of settings until you release the key. 

#### MacroButton Toggle
Allows running VoiceMeeter macros from the Stream Deck. Supports both Toggle and Push modes. The `Logical ID` number is shown at the top-center of every VoiceMeeter macro.

### Fields explained:
- Mode1 Key Press - The configuration to set when we're toggling into Mode1 -> Use this to turn ON the setting e.g. `Strip[0].Mute=1`
- Mode1 Check - True/False value to determine if we're in Mode1. Example: If you input: `Strip[0].Mute` the plugin will determine you're in Mode1 every time Strip0 is muted.
- Mode1 Image - Customizable image that will be shown when you're in Mode1
- Mode2 Key Press - The configuration to set when we're toggling into Mode2 -> Use this to turn OFF the setting e.g. `Strip[0].Mute=0`
- Mode2 Key Press - Customizable image that will be shown when you're in Mode1
- Title - Used to determine if you want a dynamic title (Based on the "Title Value" field) or a static title (Based on the "Title field" at the very top)
- Title Prefix - Text to add before displaying the `Title Value`. ProTip: Type `\n` to make the title multi-line
- Title Value - Value to display in the title. Example: If you input: `Strip[0].Mono` it will display `1` when Mono is enabled on Strip0 and `0` otherwise.


### Midi Usage
You can trigger Midi functions using the SendMidi command from the Advanced Actions.
Syntax: `SendMidi(DEVICE_NAME, COMMAND, CHANNEL, KEY_ID, VALUE);`

**DEVICE_NAME:** Name of your device. Start of the name is good too (i.e. nano instead of nanoKORG).
Name can be found in VoiceMeeter Macro under `MIDI OUT1 device:
<IMG>

**COMMAND:** One of 3 options:
	- note-on
	- note-of
	- ctrl-change
	
**CHANNEL:** Integer value between 1 to 16

**KEY_ID:** The id of the Midi key to turn on/off. This can be found using the LEARN feature inside VoiceMeeter Macro:
<IMG>

### FAQ
Q: Can I use this plugin to Restart VoiceMeeter?  
A: Yes! Choose one of the "VoiceMeeter Advanced" plugins and use the following command: `Command.Restart = 1;`

Q: Stream Deck shows a big VoiceMeeter logo and nothing works  
A: This means that VoiceMeeter is either not running or not properly installed. Try reloading VoiceMeeter, if that doesn't work - try reinstalling.

Q: What are the valid values for each setting?  
A: Valid values can be found starting on page 9 of VoiceMeeter API PDF: https://download.vb-audio.com/Download_CABLE/VoicemeeterRemoteAPI.pdf

Q: Can I make the title multi-line?
A: Yes, write `\n` in the `Title Prefix` parameter to add lines

Q: Is there AND/OR support for Mode1 Check?
A: Yes, there is now `AND/OR` support on the Mode1 Check in the `Advanced Toggle`! You can now do things like `Strip[0].Mute AND Strip[1].B2` or `Strip[0].Solo OR Strip[0].B2 OR Strip[1].B1`

Q: Where can I find the Macro `Logical ID`?
A: The `Logical ID` number is shown at the top-center of every VoiceMeeter macro.
### Download

* [Download plugin](https://github.com/BarRaider/streamdeck-voicemeeter/releases/)

## I found a bug, who do I contact?
For support please contact the developer. Contact information is available at https://barraider.com

## I have a feature request, who do I contact?
Please contact the developer. Contact information is available at https://barraider.com

## Dependencies
* Uses StreamDeck-Tools by BarRaider: [![NuGet](https://img.shields.io/nuget/v/streamdeck-tools.svg?style=flat)](https://www.nuget.org/packages/streamdeck-tools)
* Uses [Easy-PI](https://github.com/BarRaider/streamdeck-easypi) by BarRaider - Provides seamless integration with the Stream Deck PI (Property Inspector) 

## Change Log

## What's new in v1.8
- `AND/OR` support on the Mode1 Check in the `Advanced Toggle`! You can now do things like `Strip[0].Mute AND Strip[1].B2` or `Strip[0].Solo OR Strip[0].B2 OR Strip[1].B1`
- Customizable `Long Press` length on the Press/Long-Press action
- **Hotkey Support** :new: - All advanced actions now support sending hotkeys to integrate with the :voicemeeter: Macros
- **Midi Support** :new: - All advanced actions now support the :voicemeeter: `SendMidi` commands
- Support for `\n` (new line) in the Enabled/Disabled Texts

## What's new in 1.7
- VM Advanced actions now support renaming values that show 1 / 0 to a user-defined text (On/Off or Enabled/Disabled, etc.)
- Bugfix in which the second image was not stored correctly in the VM Advanced Toggle action
- Moved actions to a "VoiceMeeter" category in the Stream Deck app
