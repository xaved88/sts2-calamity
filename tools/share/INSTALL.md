# Calamity (playtest build)

Ascension 11 for Slay the Spire 2 (v0.107.1): every act gets a **Trial** (the party votes on a modifier for the
boss) and two **Calamity** floors (everyone takes a hardship). In co-op, being revived costs someone an
**Unholy Pact**.

## Install (2 minutes)

1. Close the game.
2. Extract this zip anywhere and double-click **Install-Calamity.bat**. It:
   - copies the mods into your game folder,
   - gives you a ready-to-play **modded** profile with everything unlocked, including **Ascension 11**
     (it's the mod author's profile, minus run history and settings),
   - backs up any modded profile you already had. Your normal, unmodded save is never touched.
3. **Turn off Steam Cloud for the game before launching:** Steam → right-click Slay the Spire 2 → Properties →
   General → untick "Keep game saves in the Steam Cloud". Otherwise Steam replaces the installed profile.
   You can turn it back on after your first session (if Steam asks, keep the files on this computer).
4. Launch. The first time, the game asks to enable mods and then **closes itself**; launch it again.

Everyone in the lobby needs this same build. If you already have BaseLib from the Workshop, delete
`mods\BaseLib` from your game folder after installing (two copies conflict).

Doing it by hand instead: copy `mods\*` into the game folder, and `profile\saves\progress.save` into
`%APPDATA%\SlayTheSpire2\steam\<your id>\modded\profile1\saves\`.

## Want your own unlocks instead?

With mods on, the game uses a separate "modded" profile. To use your real progress there: with Steam Cloud off, copy
`%APPDATA%\SlayTheSpire2\steam\<your id>\profile1` over `...\<your id>\modded\profile1`. Then in game, open the dev
console (`` ` `` key) and type `unlock ascensions` to reach A11. (Game version 0.108, coming soon, copies your
save over automatically.)

## Uninstall

Delete `mods\Calamity` (and `mods\BaseLib` if nothing else needs it). Your backups are in
`%APPDATA%\SlayTheSpire2\calamity-backups`.
