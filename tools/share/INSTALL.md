# Calamity (playtest build)

Ascension 11 for Slay the Spire 2 (v0.107.1): every act gets a **Trial** (the party votes on a modifier for the
boss) and two **Calamity** floors (everyone takes a hardship). In co-op, being revived costs someone an
**Unholy Pact**.

## Install

1. Close the game.
2. Extract this zip anywhere and double-click **Install-Calamity.bat**. It copies the two mods into your game folder
   and doesn't touch your saves.
   (By hand instead: copy the `mods` folder into your game folder. Steam → right-click Slay the Spire 2 → Manage →
   Browse local files.)
3. Launch. The first time, the game asks to enable mods and then **closes itself**; launch it again.

Everyone in the lobby needs this same build. If you already have BaseLib from the Workshop, delete
`mods\BaseLib` from your game folder after installing (two copies conflict).

## Unlock everything (once, from the main menu)

With mods on, the game uses a separate, fresh **modded** profile, so everything starts locked. Your normal save is
untouched and comes back whenever you disable mods.

Open the dev console with the **`** key (left of `1`) and type:

```
unlock all
```

That unlocks all characters, cards, relics, potions, events, ancients (via the timeline), acts, and every Ascension
up to **A11**. It only affects the modded profile. If a character still shows as locked, restart the game.

To unlock just some of it instead: `unlock ascensions`, `unlock epochs`, `unlock cards`, `unlock relics`,
`unlock potions`, `unlock events`, `unlock monsters`, `unlock acts`.

The co-op lobby only offers ascensions that **every** player has unlocked, so everyone needs to do this.

## Uninstall

Delete `mods\Calamity` (and `mods\BaseLib` if nothing else needs it).
