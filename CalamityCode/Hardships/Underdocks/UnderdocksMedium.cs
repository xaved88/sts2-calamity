using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Merchant;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Potions;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode.Hardships.Underdocks;

/// <summary>You start each combat Soaked: no potions until your first turn ends (enforced in SoggyBandolierPatches).</summary>
public sealed class SoggyBandolier : PlayerPowerHardship<SoakedPower>
{
    public override HardshipTier Tier => HardshipTier.Trying;

    public override HardshipAct Act => HardshipAct.Underdocks;

    protected override decimal PowerAmount => 1m;
}

/// <summary>Can't use potions while this is on you. Removed at the end of your turn.</summary>
public sealed class SoakedPower : HardshipPower
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Player && participants.Contains(Owner))
        {
            await PowerCmd.Remove(this);
        }
    }
}

/// <summary>
/// Each time you enter a merchant, one random service is closed for everyone: cards, potions, relics, or card removal.
/// The shop UI already copes with missing sections (vanilla's Fake Merchant event sells only relics).
/// </summary>
public sealed class SmugglersPadlock : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;

    public override HardshipAct Act => HardshipAct.Underdocks;

    private enum Service
    {
        Cards,
        Potions,
        Relics,
        Removal,
    }

    internal static void Lock(MerchantInventory inventory)
    {
        IRunState runState = inventory.Player.RunState;
        if (!runState.Players.Any(p => p.GetRelic<SmugglersPadlock>() != null))
        {
            return;
        }
        // Seeded by floor, so every player's shop closes the same service.
        var rng = new Rng(runState.Rng.Seed, $"padlock_{runState.CurrentActIndex}_{runState.ActFloor}");
        switch ((Service)rng.NextInt(4))
        {
            case Service.Cards:
                inventory._characterCardEntries.Clear();
                inventory._colorlessCardEntries.Clear();
                break;
            case Service.Potions:
                inventory._potionEntries.Clear();
                break;
            case Service.Relics:
                inventory._relicEntries.Clear();
                break;
            case Service.Removal:
                inventory.CardRemovalEntry = null;
                break;
        }
        foreach (Player player in runState.Players)
        {
            player.GetRelic<SmugglersPadlock>()?.Flash();
        }
    }
}

[HarmonyPatch(typeof(MerchantInventory), nameof(MerchantInventory.CreateForNormalMerchant))]
internal static class SmugglersPadlockPatch
{
    private static void Postfix(MerchantInventory __result) => SmugglersPadlock.Lock(__result);
}

[HarmonyPatch]
internal static class SoggyBandolierPatches
{
    // Foul Potion overrides the check, so patch both getters.
    private static IEnumerable<System.Reflection.MethodBase> TargetMethods() =>
    [
        AccessTools.PropertyGetter(typeof(PotionModel), nameof(PotionModel.PassesCustomUsabilityCheck)),
        AccessTools.PropertyGetter(typeof(FoulPotion), nameof(FoulPotion.PassesCustomUsabilityCheck)),
    ];

    private static void Postfix(PotionModel __instance, ref bool __result)
    {
        if (__result && __instance.Owner?.Creature.GetPower<SoakedPower>() != null)
        {
            __result = false;
        }
    }
}
