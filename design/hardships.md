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

## Hardship list (48)

Icons show the placeholder art once it's built (relic, then any power it applies). Feasibility: **Easy** = one hook on
the relic/power; **Medium** = custom card, saved counter, or small Harmony patch; **Hard** = new UI.

Every hardship that affects a fight shows up in that fight as a power, card or relic counter; see
[CONVENTIONS.md](../CONVENTIONS.md).

### General

#### Trying (you)

| Icon | Name | Effect | Visible as | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/nettle_shirt.png) ![](../Calamity/images/powers/sensitive_power.png) | **Nettle Shirt** | Whenever you lose HP, lose 1 more. Includes events and self-damage. | Sensitive power in combat; relic flash outside | Easy: `ModifyHpLostAfterOsty` runs in and out of combat |
| ![](../Calamity/images/relics/packed/cracked_femur.png) ![](../Calamity/images/powers/brittle_power.png) | **Cracked Femur** | Start each combat with 1 Brittle: take +1 damage from each attack hit. | Brittle power | Built (was Brittle Bones). **Overlaps Nettle Shirt; pending your call to keep or cut.** |
| ![](../Calamity/images/relics/packed/lumpy_cot.png) | **Lumpy Cot** | Restore 50% less HP at campfires, from Resting or being Mended. | campfire text, relic flash | Built (was Insomnia, 30%). Mend uses the same heal hook on whoever is healed, so trading Mends doesn't dodge it. |
| ![](../Calamity/images/relics/packed/moth_eaten_purse.png) | **Moth-Eaten Purse** | Gain 25% less Gold. Not offered in Act 3. | relic flash | Built |
| ![](../Calamity/images/relics/packed/heavy_pack.png) | **Heavy Pack** | Start each combat with 2 Dazed in your draw pile. | the cards | Easy |
| ![](../Calamity/images/relics/packed/leaky_flask.png) | **Leaky Flask** | One fewer potion slot. | potion bar | Easy: same call vanilla A4 uses |
| ![](../Calamity/images/relics/packed/flea_ridden_blanket.png) ![](../Calamity/images/powers/itchy_power.png) | **Flea-Ridden Blanket** | Whenever you shuffle your draw pile, lose 1 HP. | Itchy power | Easy: `AfterShuffle` |
| ![](../Calamity/images/relics/packed/rusted_hinge.png) ![](../Calamity/images/powers/seized_power.png) | **Rusted Hinge** | The first card you play each combat costs 1 more. | power, removed once used | Easy: `TryModifyEnergyCostInCombat` |
| ![](../Calamity/images/relics/packed/bloodied_signpost.png) | **Bloodied Signpost** | Whenever you enter a "?" room, lose 5 HP. | relic flash | Easy: `AfterRoomEntered` |

#### Crippling (enemies/world, party-wide)

| Icon | Name | Effect | Visible as | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/spiteful_totem.png) ![](../Calamity/images/powers/backlash_power.png) | **Spiteful Totem** | Enemies have Backlash: whenever they lose Strength, they gain 25% of it back as *permanent* Strength (rounded up). Piercing Wail+ (−8, temporary) leaves them +2 permanently once the −8 wears off. | Backlash power | Built (was Backlash). The −8 and +2 show as one merged Strength number (−6 that turn), like any Strength in the game. |
| ![](../Calamity/images/relics/packed/briar_crown.png) | **Briar Crown** | Enemies start combat with 1 Thorns. | Thorns | Easy |
| ![](../Calamity/images/relics/packed/war_drum.png) | **War Drum** | Enemies start combat with 1 Strength. | Strength | Easy |
| ![](../Calamity/images/relics/packed/mourning_bell.png) ![](../Calamity/images/powers/mourning_power.png) | **Mourning Bell** | When an enemy dies, the others gain 1 Strength. | Mourning power on each enemy, then Strength | Easy: `AfterDeath` |
| ![](../Calamity/images/relics/packed/martyrs_fuse.png) ![](../Calamity/images/powers/volatile_power.png) | **Martyr's Fuse** | When an enemy dies, it deals 3 damage to each player. | Volatile power on each enemy | Easy (moved here from Act 3) |
| ![](../Calamity/images/relics/packed/wardens_seal.png) | **Warden's Seal** | Elites and bosses start combat with 1 Artifact. | Artifact | Easy |
| ![](../Calamity/images/relics/packed/crooked_scale.png) | **Crooked Scale** | Merchant prices +25% for everyone. | shop prices | Easy: `ModifyMerchantPrice` |
| ![](../Calamity/images/relics/packed/bramble_signpost.png) | **Bramble Signpost** | "?" rooms are much more likely to be fights. | — | Easy: `ModifyUnknownMapPointRoomTypes` / odds |
| ![](../Calamity/images/relics/packed/vakuus_sextant.png) | **Vakuu's Sextant** | Vakuu casts a random ballot on every map vote: 50% hijack solo, 20% with 4 players. | relic flash; ideally a Vakuu marker on the map | Medium/Hard: votes already resolve by random ballot, so add one. Solo has no vote step and needs its own path. The marker is the hard part; ship without it first. |

#### Crushing (you)

| Icon | Name | Effect | Visible as | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/guttering_brazier.png) | **Guttering Brazier** | Every 4 combats, choose one of your relics to melt (the wax-relic mechanic). | counter on relic; melted relic greys out | Easy/Medium: vanilla `RelicCmd.Melt` + choose-a-relic screen. Hardships can't be chosen. |
| ![](../Calamity/images/relics/packed/ballast_box.png) | **Ballast Box** | A counter that carries between fights: every 3 turns, add a **Weight** to your deck. Weight: Status, costs 1, removed from your deck when played. | counter on relic; the cards | Medium: custom card; removes its own deck copy. Pace: ~1.5 per fight, maybe every 4. |
| ![](../Calamity/images/relics/packed/bound_locket.png) | **Bound Locket** | When an ally adds a curse to their deck, you get a copy, *including Pacts*. Multiplayer only. | relic flash; the cards | Medium: patch card-added-to-deck for all players |
| ![](../Calamity/images/relics/packed/broken_glasses.png) ![](../Calamity/images/powers/blurred_vision_power.png) | **Broken Glasses** | Every enemy-targeted card and potion hits a random enemy. | Blurred Vision power | Medium: Harmony on the card-play target (set once when the play is created) |
| ![](../Calamity/images/relics/packed/hungry_jar.png) | **Hungry Jar** | Add **Gnawing** (Curse, Unplayable, **Eternal**): at the end of your turn, if it's in your hand, lose 3 HP. | the card | Medium: custom curse |
| ![](../Calamity/images/relics/packed/lead_bookmark.png) ![](../Calamity/images/powers/drowsy_power.png) | **Lead Bookmark** | Draw 1 fewer card every turn. | Drowsy power | Easy |

### Act 1: Overgrowth (spores, thorns, growth). Long-term damage.

| Icon | Tier | Name | Effect | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/pollen_sachet.png) | Trying | **Pollen Sachet** | Start each combat with 1 Weak. | Easy |
| ![](../Calamity/images/relics/packed/thistle_grip.png) ![](../Calamity/images/powers/thorny_grip_power.png) | Trying | **Thistle Grip** | The first Attack you play each turn deals 1 damage to you. | Easy |
| ![](../Calamity/images/relics/packed/puffball_spore.png) ![](../Calamity/images/powers/spore_cloud_power.png) | Crippling | **Puffball Spore** | When an enemy dies, every player gains 1 Weak. | Easy |
| ![](../Calamity/images/relics/packed/ironbark_sap.png) | Crippling | **Ironbark Sap** | Enemies start combat with 3 Plating. | Easy |
| ![](../Calamity/images/relics/packed/blighted_seeds.png) | Crushing | **Blighted Seeds** | Card rewards offer 1 fewer card. | Easy: `TryModifyCardRewardOptions` |
| ![](../Calamity/images/relics/packed/kudzu_cutting.png) | Crushing | **Kudzu Cutting** | Every 4th combat, add a **Kudzu** (Curse, Unplayable) to your deck. Nothing on pickup. | Medium: custom curse + counter |

### Act 1: Underdocks (water, rot, smugglers). Long-term damage.

| Icon | Tier | Name | Effect | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/soggy_bandolier.png) ![](../Calamity/images/powers/soaked_power.png) | Trying | **Soggy Bandolier** | You can't use potions on your first turn of each combat. | Medium: patch the potion-use check |
| ![](../Calamity/images/relics/packed/damp_powder_horn.png) ![](../Calamity/images/powers/damp_power.png) | Trying | **Damp Powder Horn** | Your first Attack each combat deals 3 less damage. | Easy: power removed after the first attack |
| ![](../Calamity/images/relics/packed/bilge_bucket.png) ![](../Calamity/images/powers/slimy_power.png) | Crippling | **Bilge Bucket** | Whenever an enemy deals unblocked attack damage to a player, add a Slimed to that player's discard pile. | Easy: `AfterDamageGiven` + vanilla Slimed |
| ![](../Calamity/images/relics/packed/smugglers_padlock.png) | Crippling | **Smuggler's Padlock** | Each time you enter a merchant, one random service is closed: cards, potions, relics, or card removal. | Medium: empty one merchant section on entry |
| ![](../Calamity/images/relics/packed/rotten_crate.png) | Crushing | **Rotten Crate** | Cards from rewards are never pre-upgraded. | Easy: `ModifyCardRewardUpgradeOdds` |
| ![](../Calamity/images/relics/packed/cracked_hull_plank.png) | Crushing | **Cracked Hull Plank** | Lose 1 Max HP after every combat. | Easy |

### Act 2 (any act at index 1). Attrition and denial.

| Icon | Tier | Name | Effect | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/rusted_buckler.png) ![](../Calamity/images/powers/rusted_power.png) | Trying | **Rusted Buckler** | Block from cards is reduced by 1. | Easy: `ModifyBlockAdditive` |
| ![](../Calamity/images/relics/packed/tiny_pouch.png) ![](../Calamity/images/powers/cramped_power.png) | Trying | **Tiny Pouch** | Your max hand size is 2 smaller. | Easy: BaseLib `IMaxHandSizeModifier` |
| ![](../Calamity/images/relics/packed/hive_nectar.png) ![](../Calamity/images/powers/shared_lifeblood_power.png) | Crippling | **Hive Nectar** | When an enemy dies, the others heal half of its Max HP. | Easy |
| ![](../Calamity/images/relics/packed/fogbound_lantern.png) ![](../Calamity/images/powers/fogbound_power.png) | Crippling | **Fogbound Lantern** | On each player's first turn of a combat, their hand is face-down until they play a card. The deck can't be opened then (the draw pile can). Innate cards are known, so they're the counterplay. | **Hard**: the game can't draw full cards face-down, so we overlay a card back on each hand card and block the deck view |
| ![](../Calamity/images/relics/packed/dull_whetstone.png) | Crushing | **Dull Whetstone** | You can't Smith at rest sites. | Easy: `TryModifyRestSiteOptions` |
| ![](../Calamity/images/relics/packed/clouded_orb.png) ![](../Calamity/images/powers/clouded_power.png) | Crushing | **Clouded Orb** | Your Powers cost 1 more. | Easy: `TryModifyEnergyCostInCombat` |

### Act 3 (any act at index 2). Immediate pain.

| Icon | Tier | Name | Effect | Feasibility |
| --- | --- | --- | --- | --- |
| ![](../Calamity/images/relics/packed/trembling_gauntlet.png) | Trying | **Trembling Gauntlet** | Discard 1 random card at the start of your first turn. | Easy |
| ![](../Calamity/images/relics/packed/blue_skull.png) ![](../Calamity/images/powers/rage_threshold_power.png) | Crippling | **Blue Skull** | Enemies have **Rage Threshold**: the first time each drops to half HP or below, it gains 3 Strength (Red Skull, for them). | Easy |
| ![](../Calamity/images/relics/packed/shieldbreaker_spike.png) ![](../Calamity/images/powers/shieldbreaker_power.png) | Crippling | **Shieldbreaker Spike** | Enemy attacks deal 3 extra damage to Block. | Medium: extra damage that only lands on Block |
| ![](../Calamity/images/relics/packed/iron_manacle.png) ![](../Calamity/images/powers/manacled_power.png) | Crushing | **Iron Manacle** | You can play only 1 Attack per turn. | Easy: `ShouldPlay`. Moved up from Trying: every turn is crushing for attack decks. |
| ![](../Calamity/images/relics/packed/frozen_ember.png) ![](../Calamity/images/powers/frozen_power.png) | Crushing | **Frozen Ember** | 1 less Energy on your first two turns. | Easy |
| ![](../Calamity/images/relics/packed/doom_clock.png) ![](../Calamity/images/powers/ticking_power.png) | Crushing | **Doom Clock** | From turn 3, gain 3 Doom at the start of each of your turns (vanilla Doom: at end of turn, if Doom ≥ your HP, you die). | Easy: vanilla `DoomPower` works on players |

**Totals:** 48 hardships (4 built, being renamed). 37 Easy, 9 Medium, 1 Medium/Hard (Vakuu's marker), 1 Hard
(Fogbound Lantern). Custom cards: Weight, Gnawing, Kudzu, Unholy Pact.

## The Pact (death punishment, multiplayer, A11)

**What vanilla does:** a dead player is revived at 1 HP when the combat ends, for free (`Player.ReviveBeforeCombatEnd`).
Solo death ends the run, and stays that way. The Pact puts a price on the existing revive.

**The card:** *Unholy Pact*: Curse. Unplayable. Eternal. "When drawn, take X damage." X lives on the card.
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
| ![](../Calamity/images/powers/split_power.png) **Trial of the Slime** | **Split:** each time it loses 25% of its Max HP, a slime splits off and fights alongside it. *Changed:* summoning instead of Slimed-on-hit, because the Bilge Bucket hardship already adds Slimed on hits. | small slimes / medium / medium | Medium: vanilla summoning (`CreatureCmd.Add`, as the Fabricator uses). **Fallback:** if the boss encounter has no free slot, every player gets 2 Slimed instead. Needs a test. |
| ![](../Calamity/images/powers/inferno_power.png) **Trial of the Ghost** | **Inferno:** every 3rd turn, shuffle Burns into each player's draw pile. *Dropped* the extra multi-attack: changing intents means rewriting each boss's move set. | 1 Burn / 2 Burns / 2 upgraded Burns | Easy: vanilla Burn |
| ![](../Calamity/images/powers/mode_shift_power.png) **Trial of the Guardian** | **Mode Shift:** after taking X damage, it gains Y Block and Z Thorns for 2 turns. The threshold rises by 10 each time, like the original. | X 30, Y 15, Z 2 / 45, 20, 3 / 60, 30, 4 | Medium: damage-counter power |
| ![](../Calamity/images/powers/second_wind_power.png) **Trial of the Champion** | **Second Wind:** the first time it drops to half HP, it removes its debuffs and gains Strength and Plating. | +3 Str, 5 Plating / +4, 8 / +5, 10 | Easy. Stacks with Blue Skull's Rage Threshold (decided). |
| ![](../Calamity/images/powers/stasis_power.png) **Trial of the Automaton** | **Stasis:** starts with Artifact; at the start of combat and every 4 turns, it takes the highest-cost card from each player's hand. Taken cards return when it drops below half HP or dies. *Changed:* no orb minions (there's no orb monster to summon); the boss holds the cards itself. | Artifact 1 / 2 / 3 | Medium/Hard: pull cards out of combat and restore them, plus a power that lists what it's holding |
| ![](../Calamity/images/powers/mega_debuff_power.png) **Trial of the Collector** | **Mega Debuff:** on its 3rd turn, gives every player Vulnerable, Weak and Frail. | 2 turns / 3 / 5 | Easy |
| ![](../Calamity/images/powers/square_and_circle_power.png) **Trial of the Shapes** | **Square and Circle:** every 2nd turn, gains Strength and shuffles Dazed into each player's draw pile (Donu's Strength, Deca's Dazed). | +1 Str, 1 Dazed / +2, 1 / +2, 2 | Easy |
| ![](../Calamity/images/powers/time_warp_power.png) **Trial of the Time Eater** | **Time Warp:** counts every card any player plays; every Nth card, it gains Strength and the player who played that card ends their turn. | N 12, +1 Str / 12, +2 / 10, +2 | Medium: party-wide counter power. Force-ending one player's turn in multiplayer needs checking; fallback is Strength only. |
| ![](../Calamity/images/powers/rebirth_power.png) **Trial of the Awakened One** | **Rebirth:** the first time it would die, it instead revives at X% of Max HP with its debuffs removed, then spends its next turn Stunned (no intent) and Intangible, so overkill damage that turn barely lands. *Rebalanced* from 20/40/60%: 60% on both Act 3 bosses on top of Double Boss is a lot. | 25% / 35% / 50% | Medium: vanilla `ShouldDie` hook (Lizard Tail uses it) on a boss power. Must hold combat open (`ShouldStopCombatFromEnding`) while at 0 HP, or the heal is skipped and it dies anyway. |

**New art:** a Trial map icon, 9 trial icons (badge, relic and power can share one), and the Trial Writ relic.
