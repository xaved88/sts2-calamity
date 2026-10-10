using Calamity.CalamityCode.Hardships;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode.Trials;

/// <summary>Shows the chosen trial's icon next to the boss node(s) on the map. Local presentation only.</summary>
internal static class TrialBadges
{
    private const string BadgeName = "CalamityTrialBadge";
    private const float BadgeSize = 96f;

    public static void Refresh()
    {
        if (NMapScreen.Instance is not { } screen || RunManager.Instance.State is not { } runState)
        {
            return;
        }
        Player me = LocalContext.GetMe(runState);
        TrialWrit? writ = me.Relics.OfType<TrialWrit>().LastOrDefault(w => w.Act == runState.CurrentActIndex);
        IReadOnlyList<TrialKind> assigned = writ?.Assigned ?? [];

        Place(screen._bossPointNode, assigned.Count > 0 ? assigned[0] : null);
        Place(screen._secondBossPointNode, assigned.Count > 1 ? assigned[1] : null);
    }

    private static void Place(Control? bossNode, TrialKind? kind)
    {
        if (bossNode == null || !GodotObject.IsInstanceValid(bossNode))
        {
            return;
        }
        bossNode.GetNodeOrNull(BadgeName)?.QueueFree();
        if (kind is not { } trial)
        {
            return;
        }
        var power = (HardshipPower)Trials.Power(trial);
        var badge = new TextureRect
        {
            Name = BadgeName,
            Texture = ResourceLoader.Load<Texture2D>(power.CustomBigIconPath, null, ResourceLoader.CacheMode.Reuse),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            Size = new Vector2(BadgeSize, BadgeSize),
            Position = new Vector2(bossNode.Size.X - BadgeSize * 0.4f, -BadgeSize * 0.3f),
            MouseFilter = Control.MouseFilterEnum.Pass,
            TooltipText = new MegaCrit.Sts2.Core.Localization.LocString("events", $"CALAMITY-TRIAL_EVENT.trials.{Trials.Key(trial)}.title").GetFormattedText(),
        };
        bossNode.AddChild(badge);
    }
}

/// <summary>Re-badge whenever the map screen is rebuilt (new act, loading a save).</summary>
[HarmonyPatch(typeof(NMapScreen), nameof(NMapScreen.SetMap))]
internal static class TrialBadgesSetMapPatch
{
    private static void Postfix() => TrialBadges.Refresh();
}
