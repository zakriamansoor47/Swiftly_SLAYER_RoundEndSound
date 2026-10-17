
<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" />
  <h2><strong>SLAYER_RoundEndSound</strong></h2>
  <h3>Plays a sound at the end of each round</h3>
</div>

<p align="center">
  <img src="https://img.shields.io/github/downloads/zakriamansoor47/Swiftly_SLAYER_RoundEndSound/total" alt="Downloads">
  <img src="https://img.shields.io/github/stars/zakriamansoor47/Swiftly_SLAYER_RoundEndSound?style=flat&logo=github" alt="Stars">
</p>

# Accepting Paid Requests!
Discord: Slayer47#7002

# Donation

If you like this project, consider supporting me:

<a href="https://www.buymeacoffee.com/slayer47" target="_blank">
  <img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;">
</a>

[![PayPal](https://www.paypalobjects.com/webstatic/mktg/logo/pp_cc_mark_37x23.jpg)](https://paypal.me/zakriamansoor)

## Description

SLAYER_RoundEndSound is a SwiftlyS2 plugin that plays a sound at the end of each round in Counter-Strike 2.

The plugin supports two different audio playback methods:

- **Workshop Sound Events + Volume API** — Plays sounds directly through the CS2 sound system using Workshop sound files. Players can adjust the volume using the global `!volume` command.
- **Swiftly Audio API** — Plays sounds through the Audio plugin. Players can adjust their individual RoundEndSound volume using `!res_vol` or `!res_volume`.

Players can enable or disable RoundEndSound using the `!res` command.

### Features

- Plays custom sounds at the end of each round.
- Supports Workshop Sound Events and Swiftly Audio API playback.
- Optional random sound selection.
- Individual player sound preferences.
- Optional sound notifications.
- Supports configurable default volume for Audio playback.
- Supports Volume API for Workshop sound volume control.
- Translatable chat messages.
- Configurable sound list.

## Requirements

- **[SwiftlyS2](https://github.com/swiftly-solution/swiftlys2)** — Required plugin framework.
- **[Volume API](https://github.com/a2Labs-cc/Volume.API)** — Required for Workshop sound volume control using `!volume`.
- **[Audio](https://github.com/SwiftlyS2-Plugins/Audio)** — Optional. Required only when using the Swiftly Audio playback method.

**Note:** The Audio plugin is not required when using Workshop Sound Events.

## Installation

1. Download the latest plugin release.
2. Upload the plugin files to your CS2 server.
3. Install the required dependencies for your selected audio playback method.
4. Edit the configuration file:

   `configs/plugins/SLAYER_RoundEndSound/SLAYER_RoundEndSound.jsonc`

5. Configure the sound files and playback settings.
6. Restart the server, change the map, or reload the plugin.

**Workshop users:** Make sure your sound files are available through the Workshop addon and your Sound Event file is configured correctly.

## Commands

| Command | Description |
|---------|-------------|
| `!res` | Enable or disable RoundEndSound for the player. |
| `!res_vol 0.5` | Set RoundEndSound volume to 0.5 when using the Audio plugin. |
| `!res_volume 1.0` | Set RoundEndSound volume to 1.0 when using the Audio plugin. |
| `!volume` | Open the global Volume API volume controls for Workshop sounds. |

### Volume Control

**When `RES_UseAudioPlugin` is `true`:**

- Sounds are played through the Swiftly Audio plugin.
- Players can use `!res_vol` or `!res_volume` to adjust their RoundEndSound volume.
- Volume values range from `0.0` to `1.0`.

**When `RES_UseAudioPlugin` is `false`:**

- Sounds are played through Workshop Sound Events.
- Workshop volume is controlled by Volume API using `!volume`.
- The plugin's own volume settings do not affect Workshop sound volume.
- If a player uses `!res_vol` or `!res_volume`, the plugin displays a translatable message directing them to `!volume`.

## Configuration

Configuration file:

`configs/plugins/SLAYER_RoundEndSound/SLAYER_RoundEndSound.jsonc`

```jsonc
{
  "Main": {
    "RES_DatabaseConnection": "default",
    "RES_UseAudioPlugin": false,
    "RES_SoundEventFile": "path/your.vsndevts",
    "RES_PlayInRandomOrder": true,
    "RES_EnableSoundNotification": true,
    "RES_DefaultVolume": 1,
    "RES_Sounds": [
      {
        "Name": "BO - Sirius",
        "FilePath": "BO - Sirius.mp3"
      },
      {
        "Name": "CJ - WHOOPTY (ERS Remix)",
        "FilePath": "CJ - WHOOPTY (ERS Remix).mp3"
      },
      {
        "Name": "CVRTOON - Izmir Marsi",
        "FilePath": "CVRTOON - Izmir Marsi.mp3"
      },
      {
        "Name": "Dravek - Katana (slowed + reverb)",
        "FilePath": "Dravek - Katana (slowed + reverb).mp3"
      },
      {
        "Name": "HOPEX - Paradise",
        "FilePath": "HOPEX - Paradise.mp3"
      },
      {
        "Name": "NOES - Afraid",
        "FilePath": "NOES - Afraid.mp3"
      },
      {
        "Name": "NOES - Paradise",
        "FilePath": "NOES - Paradise.mp3"
      },
      {
        "Name": "NOES & AKRA - And Die",
        "FilePath": "NOES & AKRA - And Die.mp3"
      },
      {
        "Name": "PashaMusic - Mafia",
        "FilePath": "PashaMusic - Mafia.mp3"
      },
      {
        "Name": "Suzume no Tojimari - Theme Song (AWO Remix)",
        "FilePath": "Suzume no Tojimari - Theme Song (AWO Remix).mp3"
      }
    ]
  }
}
```

### Configuration Options

| Setting | Description |
|---------|-------------|
| `RES_DatabaseConnection` | Database connection used by the plugin. |
| `RES_UseAudioPlugin` | `true` for Swiftly Audio playback, `false` for Workshop Sound Events. |
| `RES_SoundEventFile` | Path to the Workshop Sound Event file. |
| `RES_PlayInRandomOrder` | Play a randomly selected sound instead of sequential playback. |
| `RES_EnableSoundNotification` | Enable or disable sound notifications in chat. |
| `RES_DefaultVolume` | Default player volume for Audio playback. Does not control Workshop sound volume. |
| `RES_Sounds` | List of available round-end sounds. |

### Audio Playback Methods

#### 1. Workshop Sound Events (Recommended)

Set:

```jsonc
"RES_UseAudioPlugin": false
```

- Uses the CS2 native sound system.
- Does not require the Swiftly Audio plugin.
- Supports player volume adjustment through Volume API.
- Avoids server-side VoIP audio encoding.
- Recommended for better audio quality and lower server-side audio processing overhead.

**Important:** Workshop sound files must be correctly installed and referenced by the Sound Event configuration.

#### 2. Swiftly Audio Plugin

Set:

```jsonc
"RES_UseAudioPlugin": true
```

- Requires the Swiftly Audio plugin.
- Uses the Audio API for sound playback.
- Supports individual RoundEndSound volume adjustment using `!res_vol` or `!res_volume`.
- Uses `RES_DefaultVolume` as the default Audio playback volume.

## Translations

Chat messages can be customized in the plugin's translation files.

For example, the Workshop volume notification can be translated using:

```jsonc
"Chat.RES_WorkshopVolume": "[gold]To adjust Workshop sound volume, use [lime]!volume[gold]."
```

When Workshop playback is enabled, players using `!res_vol` or `!res_volume` will receive this message instead of changing the Workshop sound volume.

## Support

For questions, issues, or paid requests, contact:

**Discord:** Slayer47#7002
