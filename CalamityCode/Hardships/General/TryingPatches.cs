using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Potions;
using MegaCrit.Sts2.Core.Rewards;

namespace Calamity.CalamityCode.Hardships.General;

/// <summary>
/// Leaky Flask: the potion bar only ever adds slots, because vanilla only removes them at run start (A4), before the bar
/// exists. When a slot is taken away mid-run the model already does the right thing (a potion in a removed slot moves to
/// a free slot, or the right-most one is discarded if there's none); this makes the bar match: extra holders go, and
/// each remaining holder shows the potion that's really in that slot. No hook covers UI for a shrinking belt.
/// </summary>
[HarmonyPatch(typeof(NPotionContainer), nameof(NPotionContainer.GrowPotionHolders))]
internal static class PotionBarShrinkPatch
{
    private const double RemoveDelay = 0.5; // lets the discard animation (0.4s) finish first

    private static void Postfix(NPotionContainer __instance, int newMaxPotionSlots)
    {
        List<NPotionHolder> holders = __instance._holders;
        if (holders.Count <= newMaxPotionSlots || __instance._player is not { } player)
        {
            return;
        }
        for (int i = 0; i < newMaxPotionSlots; i++)
        {
            PotionModel? model = i < player.PotionSlots.Count ? player.PotionSlots[i] : null;
            NPotionHolder holder = holders[i];
            if (holder.Potion?.Model == model)
            {
                continue;
            }
            if (holder.HasPotion)
            {
                holder.RemoveUsedPotion();
            }
            if (model != null && NPotion.Create(model) is { } node)
            {
                node.Position = new Vector2(-30f, -30f);
                holder.AddPotion(node);
            }
        }
        List<NPotionHolder> removed = holders.GetRange(newMaxPotionSlots, holders.Count - newMaxPotionSlots);
        holders.RemoveRange(newMaxPotionSlots, removed.Count);
        foreach (NPotionHolder holder in removed)
        {
            __instance.OnPotionHolderUnfocused(holder);
            if (holder.HasPotion)
            {
                holder.RemoveUsedPotion(); // a potion that moved left: shrink the old copy away
            }
            holder.FocusMode = Control.FocusModeEnum.None;
            holder.MouseFilter = Control.MouseFilterEnum.Ignore;
            if (holder.GetTree() is { } tree)
            {
                tree.CreateTimer(RemoveDelay).Timeout += () =>
                {
                    if (GodotObject.IsInstanceValid(holder))
                    {
                        holder.QueueFreeSafely();
                    }
                };
            }
            else
            {
                holder.QueueFreeSafely();
            }
        }
        __instance.UpdateNavigation();
    }
}

/// <summary>
/// Moth-Eaten Purse: the gold reward reads "25 Gold (18)", the second number being what you'll actually get, so the cut
/// isn't a surprise. Shown only when a hardship changes the amount; other gold modifiers keep the vanilla label.
/// </summary>
[HarmonyPatch(typeof(GoldReward), nameof(GoldReward.Description), MethodType.Getter)]
internal static class GoldRewardShowsHardshipCutPatch
{
    private static void Postfix(GoldReward __instance, ref LocString __result)
    {
        if (!__instance.IsPopulated)
        {
            return;
        }
        Player player = __instance.Player;
        decimal received = Hook.ModifyGoldGained(player.RunState, player.Creature.CombatState, __instance.Amount, player, out IEnumerable<AbstractModel> modifiers);
        if (!modifiers.Any(m => m is HardshipRelic))
        {
            return;
        }
        var label = new LocString("relics", "CALAMITY-MOTH_EATEN_PURSE.rewardLabel");
        label.Add("reward", __result);
        label.Add("received", Math.Max(0m, Math.Floor(received)));
        __result = label;
    }
}
