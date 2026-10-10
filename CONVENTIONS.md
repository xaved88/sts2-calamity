# Calamity conventions

Rules for designing and coding Calamity content. When something here conflicts with a quick fix, this wins.

## Design

- **Everything that affects a fight is visible.** If a hardship, trial or pact changes what happens in combat, it
  shows up *in* combat: a power (buff/debuff) on the creature it affects, a card, or a relic counter. A relic flash on
  its own is not enough, because in co-op the other players can't see your relics.
  - Effects on enemies (or that enemies cause) are a power on **every enemy**, including ones summoned mid-fight.
  - Effects on a player are a power on **that player**.
  - Effects outside combat (shops, rest sites, map, rewards) can live on the relic, shown through the relic counter,
    the screen they change (prices, missing options, campfire text) or a flash.
  - One-off moments (e.g. Trembling Gauntlet's single turn-1 discard) may use just a flash.
- **Every hardship is its own mechanic.** No weaker/stronger versions of the same idea. The tier (Trying / Crippling /
  Crushing) says how severe it is, not which version it is.
- **Names are items**, like vanilla relics ("Lumpy Cot", not "Insomnia"). Powers can be states ("Drowsy", "Volatile").
- **Flat numbers over small percentages.** The game floors damage, so "+10%" vanishes on small hits; "+1 per hit"
  doesn't.
- **Party decisions use vanilla voting** (one random ballot, like events and pathing), for consistency.
- **Never quietly change players' saves or unlocks.** Unlocks come from playing (A10 unlocks A11) or from the player
  running a dev console command.
- **Match community and official standards first.** Before inventing a mechanism (saves, config, content
  registration, packaging), check what BaseLib, Mega Crit or common mods already do.

## Code

- **The relic hands out a power; the power holds the logic.** Use the bases in `Hardships/PowerHardships.cs`:
  - `EnemyPowerHardship<TPower>`: applies `TPower` to every enemy at combat start and to anything summoned later.
  - `PlayerPowerHardship<TPower>`: applies `TPower` to the holder at combat start (before the first draw and energy).
  - Powers derive from `HardshipPower` (icon paths, permanent counter). Use a vanilla power when one already does the
    job (Thorns, Strength, Weak, Doom...).
- **Party-wide effects must work with one holder.** A hardship held by anyone is never offered again, so only one
  copy exists, but every player's game runs every relic's hooks: anything that changes shared state must be
  idempotent (check before applying) and deterministic.
- **Multiplayer determinism.** Anything that affects game state uses the run's seeded RNG or a seed derived from shared
  state (seed + act + floor), never `Random`. Only UI presentation (hand fog, banners, badges) may be local-only.
  Player choices go through `PlayerChoiceSynchronizer` the way vanilla does (see the Pact and Mend).
- **Timed debuffs on players skip their first tick.** Vanilla sets `SkipNextDurationTick` on every debuff applied to a
  player (enemies apply them in their own turn). A debuff a hardship applies at the start of the player's turn would
  last a turn longer than its number says; clear the flag after applying when the number should mean "this turn".
- **Prefer hooks over Harmony.** Override a model hook when one exists. When a Harmony patch is needed, say in its
  comment *why* no hook covers it, and patch the narrowest method.
- **Text lives in `Calamity/localization/eng/*.json`**, keyed `CALAMITY-<SNAKE_NAME>`. The build fails if a model has
  no text (BaseLib's analyzer). Numbers in text come from `DynamicVars`, so code and text can't drift apart.
- **Art**: add a drawing to `tools/art/make_placeholders.py` (or an SVG in `tools/art/src/`) named after the class in
  snake_case; `render.ps1` produces every size the game needs. No `<text>` in SVGs: the render container has no fonts,
  so draw letters as paths.
- **Don't commit** decompiled game or BaseLib code (`reference/`), build output, or anything from a player's save
  folder.

## Checklist for a new hardship

1. Pick the pool (`Hardships/<Pool>/`), tier and an item name.
2. Subclass `EnemyPowerHardship<T>` / `PlayerPowerHardship<T>` (combat effects) or `HardshipRelic` (out of combat).
3. Write the power with the logic; the relic only sets `PowerAmount`.
4. Add relic and power text (`relics.json`, `powers.json`); the relic description names the power.
5. Add art for the relic and the power.
6. Add it to `design/hardships.md` with its icon.
7. Test in game: `relic add calamity-<name>` in a run.
