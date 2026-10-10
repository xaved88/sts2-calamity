# Calamity design: hardships & the Pact

Status: **built, untested in co-op.** All 48 hardships, the Pact and the Trials are in. Placeholder art throughout.

## Rules

### Picking

- A calamity offers **players + 1** hardships (solo: 2, 4 players: 5).
- Every player must take exactly one; the leftover is discarded, so you can dodge the worst.
- This is vanilla's treasure rule as-is: each player clicks one; if two want the same one they play rock-paper-scissors and
  the loser is handed an unclaimed one.
- Crippling (enemy/world) hardships hit the whole party in multiplayer, because enemies are shared. Taking one is a social
  choice.

### Tiers (severity only)

| Tier | Label | Who it hits |
| --- | --- | --- |
| Common | **Trying** | you |
| Uncommon | **Crippling** | enemies / the world (party-wide) |
| Rare | **Crushing** | you |

**Rule:** every hardship is its own mechanic. No two entries are the same idea at different strengths; the tier says how
severe a hardship is, not which version of it you got. Names are **items**, like vanilla relics.

### Rarity odds (per offered slot)

| Calamity | Trying | Crippling | Crushing | Guarantee |
| --- | --- | --- | --- | --- |
| I (before the chest) | 65% | 30% | 5% | — |
| II (before the boss) | 30% | 40% | 30% | at least 1 Crushing |

**Open balance question:** one leftover always exists, so a single guaranteed Crushing can always be dodged (solo: take
the other). Shipping with 1 as a tunable constant; revisit after playtests.

### Pools

- Each slot rolls a tier, then draws from **General ∪ current act pool** for that tier, with act entries weighted ×2. If
  the act pool has none of that tier, General alone.
- Act pools: **Act 1** keyed by act (Overgrowth, Underdocks); **Act 2 / Act 3** keyed by act index, so whatever is at
  index 1/2 later inherits them (today: Hive, Glory).
- A hardship already held by anyone is excluded. Hardships can restrict themselves (`IsAllowed`): e.g. long-term
  economy ones skip Act 3, multiplayer-only ones skip solo.
- Act themes: **Act 1** Crushing hurts the *rest of the run*; **Act 2** is attrition and denial; **Act 3** is immediate
  pain.
- Combat getting harder pushes players toward "?" rooms, so General includes two hardships that make "?" rooms costly
  (Bramble Signpost, Bloodied Signpost).

## Testing in game

Open the dev console with `` ` `` during an A11 run. IDs are case-insensitive; Tab autocompletes.

| What | Command |
| --- | --- |
| Get a hardship | `relic add CALAMITY-NETTLE_SHIRT` (IDs are under each name below) |
| Remove it again | `relic remove CALAMITY-NETTLE_SHIRT` |
| Get a Trial Writ for this act's boss | `trial awakened` (Act 3: `trial awakened champion`, one per boss), then `travel` and click the boss. `fight X_BOSS` won't trigger the writ: it starts a normal fight. |
| The real Trial vote | `event CALAMITY-TRIAL_EVENT` (3 random trials, fixed per act) |
| Put a power straight on a creature | `power CALAMITY-REBIRTH_POWER 25 1` (0 = you, 1 = first enemy) |
| Calamity cards | `card CALAMITY-UNHOLY_PACT`, `CALAMITY-GNAWING`, `CALAMITY-KUDZU`, `CALAMITY-WEIGHT` (add `Deck` or `Draw` for another pile) |
| Fights | `fight <ENCOUNTER>` (Tab lists them), `win`, `kill 1` (normal kill: death-preventing powers still trigger), `damage 50 1` |
| Moving around | `travel` (jump to any map node), `act 2`, `room <id>` |
| Staying alive | `godmode`, `heal 50`, `energy 5`, `draw 5`, `gold 500` |

## Hardship list (48)

Icons show the placeholder art once it's built (relic, then any power it applies). Feasibility: **Easy** = one hook on
the relic/power; **Medium** = custom card, saved counter, or small Harmony patch; **Hard** = new UI.

Every hardship that affects a fight shows up in that fight as a power, card or relic counter; see
[CONVENTIONS.md](../CONVENTIONS.md).

### General

#### Trying (you)

| Icon | Name | Effect | Visible as | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/nettle_shirt.png) ![](../Calamity/images/powers/sensitive_power.png) | **Nettle Shirt**<br>`CALAMITY-NETTLE_SHIRT` | Whenever you lose HP, lose 1 more. Includes events and self-damage. | Sensitive power in combat; relic flash outside | Easy: `ModifyHpLostAfterOsty` runs in and out of combat |
| ![](../Calamity/images/relics/packed/cracked_femur.png) ![](../Calamity/images/powers/brittle_power.png) | **Cracked Femur**<br>`CALAMITY-CRACKED_FEMUR` | Start each combat with 1 Brittle: take +1 damage from each attack hit. | Brittle power | Built (was Brittle Bones). **Overlaps Nettle Shirt; pending your call to keep or cut.** |
| ![](../Calamity/images/relics/packed/lumpy_cot.png) | **Lumpy Cot**<br>`CALAMITY-LUMPY_COT` | Restore 50% less HP at campfires, from Resting or being Mended. | campfire text, relic flash | Built (was Insomnia, 30%). Mend uses the same heal hook on whoever is healed, so trading Mends doesn't dodge it. |
| ![](../Calamity/images/relics/packed/moth_eaten_purse.png) | **Moth-Eaten Purse**<br>`CALAMITY-MOTH_EATEN_PURSE` | Gain 25% less Gold. Not offered in Act 3. | relic flash | Built |
| ![](../Calamity/images/relics/packed/heavy_pack.png) | **Heavy Pack**<br>`CALAMITY-HEAVY_PACK` | Start each combat with 2 Dazed in your draw pile. | the cards | Easy |
| ![](../Calamity/images/relics/packed/leaky_flask.png) | **Leaky Flask**<br>`CALAMITY-LEAKY_FLASK` | One fewer potion slot. | potion bar | Easy: same call vanilla A4 uses, plus a potion-bar patch (the bar never shrank mid-run). With a full belt the right-most potion is discarded first (vanilla's removal throws on a full belt). |
| ![](../Calamity/images/relics/packed/flea_ridden_blanket.png) ![](../Calamity/images/powers/itchy_power.png) | **Flea-Ridden Blanket**<br>`CALAMITY-FLEA_RIDDEN_BLANKET` | Whenever you shuffle your draw pile, lose 1 HP. | Itchy power | Easy: `AfterShuffle` |
| ![](../Calamity/images/relics/packed/rusted_hinge.png) ![](../Calamity/images/powers/seized_power.png) | **Rusted Hinge**<br>`CALAMITY-RUSTED_HINGE` | The first card you play each combat costs 1 more. | power, removed once used | Easy: `TryModifyEnergyCostInCombat` |
| ![](../Calamity/images/relics/packed/bloodied_signpost.png) | **Bloodied Signpost**<br>`CALAMITY-BLOODIED_SIGNPOST` | Whenever you enter a "?" room, lose 5 HP. | relic flash | Easy: `AfterRoomEntered` |

#### Crippling (enemies/world, party-wide)

| Icon | Name | Effect | Visible as | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/spiteful_totem.png) ![](../Calamity/images/powers/backlash_power.png) | **Spiteful Totem**<br>`CALAMITY-SPITEFUL_TOTEM` | Enemies have Backlash: whenever they lose Strength, they gain 25% of it back as *permanent* Strength (rounded up). Piercing Wail+ (−8, temporary) leaves them +2 permanently once the −8 wears off. | Backlash power | Built (was Backlash). The −8 and +2 show as one merged Strength number (−6 that turn), like any Strength in the game. |
| ![](../Calamity/images/relics/packed/briar_crown.png) | **Briar Crown**<br>`CALAMITY-BRIAR_CROWN` | Enemies start combat with 1 Thorns. | Thorns | Easy |
| ![](../Calamity/images/relics/packed/war_drum.png) | **War Drum**<br>`CALAMITY-WAR_DRUM` | Enemies start combat with 1 Strength. | Strength | Easy |
| ![](../Calamity/images/relics/packed/mourning_bell.png) ![](../Calamity/images/powers/mourning_power.png) | **Mourning Bell**<br>`CALAMITY-MOURNING_BELL` | When an enemy dies, the others gain 1 Strength. | Mourning power on each enemy, then Strength | Easy: `AfterDeath` |
| ![](../Calamity/images/relics/packed/martyrs_fuse.png) ![](../Calamity/images/powers/volatile_power.png) | **Martyr's Fuse**<br>`CALAMITY-MARTYRS_FUSE` | When an enemy dies, it deals 3 damage to each player. | Volatile power on each enemy | Easy (moved here from Act 3) |
| ![](../Calamity/images/relics/packed/wardens_seal.png) | **Warden's Seal**<br>`CALAMITY-WARDENS_SEAL` | Elites and bosses start combat with 1 Artifact. | Artifact | Easy |
| ![](../Calamity/images/relics/packed/crooked_scale.png) | **Crooked Scale**<br>`CALAMITY-CROOKED_SCALE` | Merchant prices +25% for everyone. | shop prices | Easy: `ModifyMerchantPrice` |
| ![](../Calamity/images/relics/packed/bramble_signpost.png) | **Bramble Signpost**<br>`CALAMITY-BRAMBLE_SIGNPOST` | "?" rooms are much more likely to be fights. | — | Easy: `ModifyUnknownMapPointRoomTypes` / odds |
| ![](../Calamity/images/relics/packed/vakuus_sextant.png) | **Vakuu's Sextant**<br>`CALAMITY-VAKUUS_SEXTANT` | Vakuu casts a random ballot on every map vote: 50% hijack solo, 20% with 4 players. | relic flash; ideally a Vakuu marker on the map | Medium/Hard: votes already resolve by random ballot, so add one. Solo has no vote step and needs its own path. The marker is the hard part; ship without it first. |

#### Crushing (you)

| Icon | Name | Effect | Visible as | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/guttering_brazier.png) | **Guttering Brazier**<br>`CALAMITY-GUTTERING_BRAZIER` | Every 4 combats, choose one of your relics to melt (the wax-relic mechanic). | counter on relic; melted relic greys out | Easy/Medium: vanilla `RelicCmd.Melt` (mark the relic wax first: vanilla only melts wax) + choose-a-relic screen. Calamity's own relics (hardships, Trial Writs) and used-up relics can't be chosen. |
| ![](../Calamity/images/relics/packed/ballast_box.png) | **Ballast Box**<br>`CALAMITY-BALLAST_BOX` | A counter that carries between fights: every 3 turns, add a **Weight** to your deck. Weight: Status, costs 1, removed from your deck when played. | counter on relic; the cards | Medium: custom card; removes its own deck copy. Pace: ~1.5 per fight, maybe every 4. |
| ![](../Calamity/images/relics/packed/bound_locket.png) | **Bound Locket**<br>`CALAMITY-BOUND_LOCKET` | When an ally adds a curse to their deck, you get a copy, *including Pacts*. Multiplayer only. | relic flash; the cards | Medium: patch card-added-to-deck for all players |
| ![](../Calamity/images/relics/packed/broken_glasses.png) ![](../Calamity/images/powers/blurred_vision_power.png) | **Broken Glasses**<br>`CALAMITY-BROKEN_GLASSES` | Every enemy-targeted card and potion hits a random enemy. | Blurred Vision power | Medium: Harmony on the card-play target (set once when the play is created) |
| ![](../Calamity/images/relics/packed/hungry_jar.png) | **Hungry Jar**<br>`CALAMITY-HUNGRY_JAR` | Add **Gnawing** (Curse, Unplayable, **Eternal**): at the end of your turn, if it's in your hand, lose 3 HP. | the card | Medium: custom curse |
| ![](../Calamity/images/relics/packed/lead_bookmark.png) ![](../Calamity/images/powers/drowsy_power.png) | **Lead Bookmark**<br>`CALAMITY-LEAD_BOOKMARK` | Draw 1 fewer card every turn. | Drowsy power | Easy |

### Act 1: Overgrowth (spores, thorns, growth). Long-term damage.

| Icon | Tier | Name | Effect | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/pollen_sachet.png) | Trying | **Pollen Sachet**<br>`CALAMITY-POLLEN_SACHET` | Start each combat with 1 Weak. | Easy |
| ![](../Calamity/images/relics/packed/thistle_grip.png) ![](../Calamity/images/powers/thorny_grip_power.png) | Trying | **Thistle Grip**<br>`CALAMITY-THISTLE_GRIP` | The first Attack you play each turn deals 1 damage to you. | Easy |
| ![](../Calamity/images/relics/packed/puffball_spore.png) ![](../Calamity/images/powers/spore_cloud_power.png) | Crippling | **Puffball Spore**<br>`CALAMITY-PUFFBALL_SPORE` | When an enemy dies, every player gains 1 Weak. | Easy |
| ![](../Calamity/images/relics/packed/ironbark_sap.png) | Crippling | **Ironbark Sap**<br>`CALAMITY-IRONBARK_SAP` | Enemies start combat with 3 Plating. | Easy |
| ![](../Calamity/images/relics/packed/blighted_seeds.png) | Crushing | **Blighted Seeds**<br>`CALAMITY-BLIGHTED_SEEDS` | Card rewards offer 1 fewer card. | Easy: `TryModifyCardRewardOptions` |
| ![](../Calamity/images/relics/packed/kudzu_cutting.png) | Crushing | **Kudzu Cutting**<br>`CALAMITY-KUDZU_CUTTING` | Every 4th combat, add a **Kudzu** (Curse, Unplayable) to your deck. Nothing on pickup. | Medium: custom curse + counter |

### Act 1: Underdocks (water, rot, smugglers). Long-term damage.

| Icon | Tier | Name | Effect | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/soggy_bandolier.png) ![](../Calamity/images/powers/soaked_power.png) | Trying | **Soggy Bandolier**<br>`CALAMITY-SOGGY_BANDOLIER` | You can't use potions on your first turn of each combat. | Medium: patch the potion-use check |
| ![](../Calamity/images/relics/packed/damp_powder_horn.png) ![](../Calamity/images/powers/damp_power.png) | Trying | **Damp Powder Horn**<br>`CALAMITY-DAMP_POWDER_HORN` | Your first Attack each combat deals 3 less damage. | Easy: power removed after the first attack |
| ![](../Calamity/images/relics/packed/bilge_bucket.png) ![](../Calamity/images/powers/slimy_power.png) | Crippling | **Bilge Bucket**<br>`CALAMITY-BILGE_BUCKET` | Whenever an enemy deals unblocked attack damage to a player, add a Slimed to that player's discard pile. | Easy: `AfterDamageGiven` + vanilla Slimed |
| ![](../Calamity/images/relics/packed/smugglers_padlock.png) | Crippling | **Smuggler's Padlock**<br>`CALAMITY-SMUGGLERS_PADLOCK` | Each time you enter a merchant, one random service is closed: cards, potions, relics, or card removal. | Medium: empty one merchant section on entry |
| ![](../Calamity/images/relics/packed/rotten_crate.png) | Crushing | **Rotten Crate**<br>`CALAMITY-ROTTEN_CRATE` | Cards from rewards are never pre-upgraded. | Easy: `ModifyCardRewardUpgradeOdds` |
| ![](../Calamity/images/relics/packed/cracked_hull_plank.png) | Crushing | **Cracked Hull Plank**<br>`CALAMITY-CRACKED_HULL_PLANK` | Lose 1 Max HP after every combat. | Easy |

### Act 2 (any act at index 1). Attrition and denial.

| Icon | Tier | Name | Effect | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/rusted_buckler.png) ![](../Calamity/images/powers/rusted_power.png) | Trying | **Rusted Buckler**<br>`CALAMITY-RUSTED_BUCKLER` | Block from cards is reduced by 1. | Easy: `ModifyBlockAdditive` |
| ![](../Calamity/images/relics/packed/tiny_pouch.png) ![](../Calamity/images/powers/cramped_power.png) | Trying | **Tiny Pouch**<br>`CALAMITY-TINY_POUCH` | Your max hand size is 2 smaller. | Easy: BaseLib `IMaxHandSizeModifier` |
| ![](../Calamity/images/relics/packed/hive_nectar.png) ![](../Calamity/images/powers/shared_lifeblood_power.png) | Crippling | **Hive Nectar**<br>`CALAMITY-HIVE_NECTAR` | When an enemy dies, the others heal half of its Max HP. | Easy |
| ![](../Calamity/images/relics/packed/fogbound_lantern.png) ![](../Calamity/images/powers/fogbound_power.png) | Crippling | **Fogbound Lantern**<br>`CALAMITY-FOGBOUND_LANTERN` | On each player's first turn of a combat, their hand is face-down until they play a card. The deck can't be opened then (the draw pile can). Innate cards are known, so they're the counterplay. | **Hard**: the game can't draw full cards face-down, so we overlay a card back on each hand card and block the deck view |
| ![](../Calamity/images/relics/packed/dull_whetstone.png) | Crushing | **Dull Whetstone**<br>`CALAMITY-DULL_WHETSTONE` | You can't Smith at rest sites. | Easy: `TryModifyRestSiteOptions` |
| ![](../Calamity/images/relics/packed/clouded_orb.png) ![](../Calamity/images/powers/clouded_power.png) | Crushing | **Clouded Orb**<br>`CALAMITY-CLOUDED_ORB` | Your Powers cost 1 more. | Easy: `TryModifyEnergyCostInCombat` |

### Act 3 (any act at index 2). Immediate pain.

| Icon | Tier | Name | Effect | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/trembling_gauntlet.png) | Trying | **Trembling Gauntlet**<br>`CALAMITY-TREMBLING_GAUNTLET` | Discard 1 random card at the start of your first turn. | Easy |
| ![](../Calamity/images/relics/packed/blue_skull.png) ![](../Calamity/images/powers/rage_threshold_power.png) | Crippling | **Blue Skull**<br>`CALAMITY-BLUE_SKULL` | Enemies have **Rage Threshold**: the first time each drops to half HP or below, it gains 3 Strength (Red Skull, for them). | Easy |
| ![](../Calamity/images/relics/packed/shieldbreaker_spike.png) ![](../Calamity/images/powers/shieldbreaker_power.png) | Crippling | **Shieldbreaker Spike**<br>`CALAMITY-SHIELDBREAKER_SPIKE` | Enemy attacks deal 3 extra damage to Block. | Medium: extra damage that only lands on Block |
| ![](../Calamity/images/relics/packed/iron_manacle.png) ![](../Calamity/images/powers/manacled_power.png) | Crushing | **Iron Manacle**<br>`CALAMITY-IRON_MANACLE` | You can play only 1 Attack per turn. | Easy: `ShouldPlay`. Moved up from Trying: every turn is crushing for attack decks. |
| ![](../Calamity/images/relics/packed/frozen_ember.png) ![](../Calamity/images/powers/frozen_power.png) | Crushing | **Frozen Ember**<br>`CALAMITY-FROZEN_EMBER` | 1 less Energy on your first two turns. | Easy |
| ![](../Calamity/images/relics/packed/doom_clock.png) ![](../Calamity/images/powers/ticking_power.png) | Crushing | **Doom Clock**<br>`CALAMITY-DOOM_CLOCK` | From turn 3, gain 3 Doom at the start of each of your turns (vanilla Doom: at end of turn, if Doom ≥ your HP, you die). | Easy: vanilla `DoomPower` works on players |

**Totals:** 48 hardships (4 built, being renamed). 37 Easy, 9 Medium, 1 Medium/Hard (Vakuu's marker), 1 Hard
(Fogbound Lantern). Custom cards: Weight, Gnawing, Kudzu, Unholy Pact.

## The Pact (death punishment, multiplayer, A11)

**What vanilla does:** a dead player is revived at 1 HP when the combat ends, for free (`Player.ReviveBeforeCombatEnd`).
Solo death ends the run, and stays that way. The Pact puts a price on the existing revive.

**The card:** *Unholy Pact* (`CALAMITY-UNHOLY_PACT`): Curse. Unplayable. Eternal. "When drawn, take X damage." X lives on the card.
Reviving someone raises X by 1 on every Pact in your deck, or adds one with X = 1 if you have none. A Pact copied by Bound
Locket keeps its X. Damage for now; HP loss (ignores Block) is the alternative if damage feels toothless.

**UX:**

1. Combat is won with someone dead. Before the rewards screen, a **Pact** overlay opens for everyone: the dead player's
   portrait in the middle, each living player's portrait around it with their current Pact X.
2. **The dead player** can click a living player: "Grasp for [name]".
   **Any living player** can click "Offer your soul" on themselves.
3. The first choice to reach the host wins and is shown to everyone (a hand reaching out to the chosen player, the card
   flying into their deck). With several dead players, one Pact per dead player, resolved one after another.
4. No timer, so nobody is rushed; the dead player can always resolve it.

**Build notes:** the revive itself stays where vanilla does it (before AfterCombatEnd, which relics rely on); only the
payment is deferred to the overlay. The overlay is built in code (the PCK packer can't pack `.tscn` scenes) and synced
with BaseLib's custom network messages. This is the largest single piece of work here.

## Trials (boss modifiers, A11)

**Map:** a full **Trial** row is inserted right after the Ancient in every act (so every path crosses it; acts get 3 extra
floors in total with the two Calamity rows). It has its own map icon.

**The room:** a shared event: everyone sees the same three trials, drawn at random from the nine below, and votes.

- **Acts 1–2:** vote for the trial to **include**; that act's boss gets it.
- **Act 3:** vote for the trial to **exclude**; the other two are randomly assigned to the two Act 3 bosses (A11
  includes vanilla A10's Double Boss, so Act 3 always has two).
- **Vote resolution:** vanilla's: one random ballot is drawn, the same as events and pathing (decided for consistency).

**Visibility:**

- Every player gets a **Trial Writ** relic naming this act's trial(s). It's saved with the run automatically and removed
  after the boss.
- The boss node on the map gets a small trial badge with a hover tip.
- In the fight, each trial is a visible power on the boss.

**Targets:** a trial applies to every enemy present when the boss fight starts. Summons (including Trial of the Slime's)
don't get it.

### The nine trials

Numbers are Act 1 / Act 2 / Act 3.

| Trial | Boss gains | 1 / 2 / 3 | Feasibility |
| --- | --- | --- | --- |
| ![](../Calamity/images/powers/split_power.png) **Trial of the Slime**<br>`trial slime`<br>`CALAMITY-SPLIT_POWER` | **Split:** each time it loses 25% of its Max HP, a slime splits off and fights alongside it. *Changed:* summoning instead of Slimed-on-hit, because the Bilge Bucket hardship already adds Slimed on hits. | small slimes / medium / medium | Medium: vanilla summoning (`CreatureCmd.Add`, as the Fabricator uses). **Fallback:** if the boss encounter has no free slot, every player gets 2 Slimed instead. Needs a test. |
| ![](../Calamity/images/powers/inferno_power.png) **Trial of the Ghost**<br>`trial ghost`<br>`CALAMITY-INFERNO_POWER` | **Inferno:** every 3rd turn, shuffle Burns into each player's draw pile. *Dropped* the extra multi-attack: changing intents means rewriting each boss's move set. | 1 Burn / 2 Burns / 2 upgraded Burns | Easy: vanilla Burn |
| ![](../Calamity/images/powers/mode_shift_power.png) **Trial of the Guardian**<br>`trial guardian`<br>`CALAMITY-MODE_SHIFT_POWER` | **Mode Shift:** after taking X damage, it gains Y Block and Z Thorns for 2 turns. The threshold rises by 10 each time, like the original. | X 30, Y 15, Z 2 / 45, 20, 3 / 60, 30, 4 | Medium: damage-counter power |
| ![](../Calamity/images/powers/second_wind_power.png) **Trial of the Champion**<br>`trial champion`<br>`CALAMITY-SECOND_WIND_POWER` | **Second Wind:** the first time it drops to half HP, it removes its debuffs and gains Strength and Plating. | +3 Str, 5 Plating / +4, 8 / +5, 10 | Easy. Stacks with Blue Skull's Rage Threshold (decided). |
| ![](../Calamity/images/powers/stasis_power.png) **Trial of the Automaton**<br>`trial automaton`<br>`CALAMITY-STASIS_POWER` | **Stasis:** starts with Artifact; at the start of combat and every 4 turns, it takes the highest-cost card from each player's hand. Taken cards return when it drops below half HP or dies. *Changed:* no orb minions (there's no orb monster to summon); the boss holds the cards itself. | Artifact 1 / 2 / 3 | Medium/Hard: pull cards out of combat and restore them, plus a power that lists what it's holding |
| ![](../Calamity/images/powers/mega_debuff_power.png) **Trial of the Collector**<br>`trial collector`<br>`CALAMITY-MEGA_DEBUFF_POWER` | **Mega Debuff:** on its 3rd turn, gives every player Vulnerable, Weak and Frail. | 2 turns / 3 / 5 | Easy |
| ![](../Calamity/images/powers/square_and_circle_power.png) **Trial of the Shapes**<br>`trial shapes`<br>`CALAMITY-SQUARE_AND_CIRCLE_POWER` | **Square and Circle:** every 2nd turn, gains Strength and shuffles Dazed into each player's draw pile (Donu's Strength, Deca's Dazed). | +1 Str, 1 Dazed / +2, 1 / +2, 2 | Easy |
| ![](../Calamity/images/powers/time_warp_power.png) **Trial of the Time Eater**<br>`trial timeeater`<br>`CALAMITY-TIME_WARP_POWER` | **Time Warp:** counts every card any player plays; every Nth card, it gains Strength and the player who played that card ends their turn. | N 12, +1 Str / 12, +2 / 10, +2 | Medium: party-wide counter power. Force-ending one player's turn in multiplayer needs checking; fallback is Strength only. |
| ![](../Calamity/images/powers/rebirth_power.png) **Trial of the Awakened One**<br>`trial awakened`<br>`CALAMITY-REBIRTH_POWER` | **Rebirth:** the first time it would die, it instead revives at X% of Max HP with its debuffs removed, then spends its next turn Stunned (no intent) and Intangible, so overkill damage that turn barely lands. *Rebalanced* from 20/40/60%: 60% on both Act 3 bosses on top of Double Boss is a lot. | 25% / 35% / 50% | Medium: vanilla `ShouldDie` hook (Lizard Tail uses it) on a boss power. Must hold combat open (`ShouldStopCombatFromEnding`) while at 0 HP, or the heal is skipped and it dies anyway. |

**New art:** a Trial map icon, 9 trial icons (badge, relic and power can share one), and the Trial Writ relic.
