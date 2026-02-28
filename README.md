<div align="center">
  <img src="https://pan.samyyc.dev/s/VYmMXE" />
  <h2><strong>SLAYER_RoundEndSound</strong></h2>
  <h3>Plays a sound at the end of each round</h3>
</div>

<p align="center">
  <img src="https://img.shields.io/github/downloads/zakriamansoor47/Swiftly_SLAYER_RoundEndSound/total" alt="Downloads">
  <img src="https://img.shields.io/github/stars/zakriamansoor47/Swiftly_SLAYER_RoundEndSound?style=flat&logo=github" alt="Stars">
</p>

# Accepting Paid Request! Discord: Slayer47#7002
# Donation
If you like this project, consider supporting me:

<a href="https://www.buymeacoffee.com/slayer47" target="_blank"><img src="https://www.buymeacoffee.com/assets/img/custom_images/orange_img.png" alt="Buy Me A Coffee" style="height: 41px !important;width: 174px !important;box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;-webkit-box-shadow: 0px 3px 2px 0px rgba(190, 190, 190, 0.5) !important;" ></a>
[![PayPal](https://www.paypalobjects.com/webstatic/mktg/logo/pp_cc_mark_37x23.jpg)](https://paypal.me/zakriamansoor)


## Description:
This plugin plays a Sound at every round end. Players can enable/disable RoundEndSound with `!res` command. And can change volume by `!res_vol <0.1 - 1.0>` command.

## Required:
**[Audio](https://github.com/SwiftlyS2-Plugins/Audio)** - To Use Sounds

## Installation:
**1.** Upload files to your server.

**2.** Edit **configs/plugins/SLAYER_RoundEndSound/SLAYER_RoundEndSound.jsonc**.

**3.** Change the Map **or** Restart the Server **or** Load the Plugin.


## Commands:
`!res` - To Enable/Disable Round End Sound
`!res_volume 1.0` - To Change Round End Sound volume from 0.0 to 1.0
`!res_vol 0.5` - To Change Round End Sound volume from 0.0 to 1.0


## Configuration:
```jsonc
{
  "Main": {
    "RES_DatabaseConnection": "default",
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