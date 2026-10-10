using Calamity.CalamityCode.Hardships;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode;

/// <summary>
/// What a calamity chest holds. A calamity room is a vanilla treasure room (see CalamityRoomPatches): it offers
/// players + 1 hardships, each player takes one through vanilla's treasure picking, and the leftover is discarded.
/// </summary>
public static class CalamityRoom
{
    /// <summary>Act-pool entries are this many times likelier than General ones, so act flavor shows up.</summary>
    private const int ActPoolWeight = 2;

    /// <summary>Per-slot tier odds (Trying, Crippling, Crushing), in percent.</summary>
    private static readonly int[] FirstCalamityOdds = [65, 30, 5];
    private static readonly int[] SecondCalamityOdds = [30, 40, 30];

    /// <summary>Guaranteed Crushing hardships at the calamity before the boss (a tunable; see design doc).</summary>
    private const int SecondCalamityGuaranteedCrushing = 1;

    /// <summary>The second calamity of an act sits on the last row, right before the boss.</summary>
    public static bool IsSecondCalamity(IRunState runState) =>
        runState.CurrentMapPoint is { } point && point.coord.row == runState.Map.GetRowCount() - 1;

    /// <summary>
    /// Deterministic from shared state (seed, floor, hardships already held), so every client in a multiplayer game
    /// fills the same chest without extra sync.
    /// </summary>
    public static List<RelicModel> PickHardships(IRunState runState)
    {
        int count = runState.Players.Count + 1;
        bool second = IsSecondCalamity(runState);
        int[] odds = second ? SecondCalamityOdds : FirstCalamityOdds;
        var rng = new Rng(runState.Rng.Seed, $"calamity_{runState.CurrentActIndex}_{runState.ActFloor}");

        HardshipAct act = CurrentAct(runState);
        HashSet<ModelId> held = runState.Players.SelectMany(p => p.Relics).Select(r => r.Id).ToHashSet();
        List<HardshipRelic> candidates = ModelDb.RelicPool<HardshipPool>().AllRelics
            .OfType<HardshipRelic>()
            .Where(h => (h.Act == HardshipAct.General || h.Act == act) && !held.Contains(h.Id) && h.IsOfferable(runState))
            .OrderBy(h => h.Id.Entry)
            .ToList();

        var picks = new List<HardshipRelic>();
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            HardshipRelic pick = TakeWeighted(candidates, RollTier(odds, rng), act, rng);
            picks.Add(pick);
        }

        if (second)
        {
            // Swap non-Crushing picks for Crushing ones until the guarantee is met (or there are none left).
            while (picks.Count(p => p.Tier == HardshipTier.Crushing) < SecondCalamityGuaranteedCrushing
                   && candidates.Any(c => c.Tier == HardshipTier.Crushing)
                   && picks.Any(p => p.Tier != HardshipTier.Crushing))
            {
                int slot = picks.FindLastIndex(p => p.Tier != HardshipTier.Crushing);
                candidates.Add(picks[slot]);
                picks[slot] = TakeWeighted(candidates, HardshipTier.Crushing, act, rng);
            }
        }

        // Shuffle so the guaranteed Crushing isn't always in the same spot.
        return picks.Cast<RelicModel>().ToList().UnstableShuffle(rng);
    }

    private static HardshipAct CurrentAct(IRunState runState) => runState.Act switch
    {
        Overgrowth => HardshipAct.Overgrowth,
        Underdocks => HardshipAct.Underdocks,
        _ when runState.CurrentActIndex == 1 => HardshipAct.Act2,
        _ when runState.CurrentActIndex == 2 => HardshipAct.Act3,
        _ => HardshipAct.General,
    };

    private static HardshipTier RollTier(int[] odds, Rng rng)
    {
        int roll = rng.NextInt(odds.Sum());
        for (int tier = 0; tier < odds.Length; tier++)
        {
            roll -= odds[tier];
            if (roll < 0)
            {
                return (HardshipTier)tier;
            }
        }
        return HardshipTier.Trying;
    }

    /// <summary>
    /// Removes and returns a weighted-random candidate of the given tier; if that tier is exhausted, falls back to the
    /// nearest milder tier, then harsher.
    /// </summary>
    private static HardshipRelic TakeWeighted(List<HardshipRelic> candidates, HardshipTier tier, HardshipAct act, Rng rng)
    {
        HardshipTier[] order = tier switch
        {
            HardshipTier.Crushing => [HardshipTier.Crushing, HardshipTier.Crippling, HardshipTier.Trying],
            HardshipTier.Crippling => [HardshipTier.Crippling, HardshipTier.Trying, HardshipTier.Crushing],
            _ => [HardshipTier.Trying, HardshipTier.Crippling, HardshipTier.Crushing],
        };
        List<HardshipRelic> pool = order.Select(t => candidates.Where(c => c.Tier == t).ToList()).First(l => l.Count > 0);

        int Weight(HardshipRelic h) => h.Act == act && act != HardshipAct.General ? ActPoolWeight : 1;
        int roll = rng.NextInt(pool.Sum(Weight));
        HardshipRelic chosen = pool.First(h => (roll -= Weight(h)) < 0);
        candidates.Remove(chosen);
        return chosen;
    }
}
