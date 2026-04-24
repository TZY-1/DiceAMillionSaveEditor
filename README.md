# Dice A Million - Save Editor

## What this tool does

This is a save game editor for *Dice A Million*.

- It reads `data_1.sav` from your local save folder.
- It decodes the base64 payload into JSON data.
- It lets you edit values in a table.
- It can sync unlocked Steam achievements into your save.
- It writes the updated save back after creating a timestamped backup.

 
## How to use

1. Load save
   Click `Load Save File` to decode and show all save properties.
2. Optional Steam sync
   Enter your public Steam profile URL, then click `Load Steam Achievements and patch save`.
3. Optional manual edits
   Edit values directly in the Savegame table.
4. Save
   Click `Save Changes` to write the file. A backup is created automatically.

## Steam profile URL notes

- Example format: `https://steamcommunity.com/id/YourName/`
- Profile visibility must be public for Steam stats to load.

## Achievement mapping notes

Most achievement groups map by prefix:


| Achievement Group | Savestate Prefix  |
| ----------------- | ----------------- |
| `dice_*`          | `dicelist_*`      |
| `ring_*`          | `ringlist_*`      |
| `card_*`          | `cardlist_*`      |
| `char_*`          | `charlist_*`      |
| `challenge_*`     | `challengelist_*` |
| `boss_*`          | `bosslist_*`      |


Phone-state special mapping currently supported:

- `misc_piece1` -> `jphonestate` = `2`
- `misc_piece2` -> `jphonestate` = `3`
- `none` -> `jphonestate` = `0`


If an achievement has no mapping yet, the log will show a warning and skip that entry.
