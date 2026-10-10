using Calamity.CalamityCode.Map;
using Calamity.CalamityCode.Trials;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.sts2.Core.Nodes.TopBar;

namespace Calamity.CalamityCode.Patches;

// Vanilla switches over MapPointType in a few places and throws on values it doesn't know. BaseLib allocates our
// values ([CustomEnum]) but doesn't route custom map point types, so these patches do.

internal static class CalamityPointTypeExtensions
{
    // CustomEnum fields are 0 (Unassigned) until ModelDb.Init, so guard against matching Unassigned early.
    public static bool IsCalamity(this MapPointType type) =>
        CalamityMapPoint.Type != MapPointType.Unassigned && type == CalamityMapPoint.Type;

    public static bool IsTrial(this MapPointType type) =>
        CalamityMapPoint.Trial != MapPointType.Unassigned && type == CalamityMapPoint.Trial;
}

/// <summary>A calamity point opens a treasure room (CalamityRoomPatches fills it); a trial point opens an event.</summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.RollRoomTypeFor))]
internal static class RollRoomTypeForPatch
{
    private static bool Prefix(MapPointType pointType, ref RoomType __result)
    {
        if (pointType.IsCalamity())
        {
            __result = RoomType.Treasure;
            return false;
        }
        if (pointType.IsTrial())
        {
            __result = RoomType.Event;
            return false;
        }
        return true;
    }
}

/// <summary>The event behind a trial point is always the Trial event (vanilla would pull a random one).</summary>
[HarmonyPatch(typeof(RunManager), nameof(RunManager.CreateRoom))]
internal static class CreateTrialRoomPatch
{
    private static bool Prefix(RoomType roomType, MapPointType mapPointType, ref AbstractRoom __result)
    {
        if (roomType != RoomType.Event || !mapPointType.IsTrial())
        {
            return true;
        }
        __result = new EventRoom(ModelDb.Event<TrialEvent>());
        return false;
    }
}

/// <summary>
/// Run history records our floors as "?" floors. The history UI maps point types to loc keys inline
/// (NMapPointHistoryHoverTip._Ready) and would produce a missing key for ours.
/// </summary>
[HarmonyPatch(typeof(RunState), nameof(RunState.AppendToMapPointHistory))]
internal static class AppendToMapPointHistoryPatch
{
    private static void Prefix(ref MapPointType mapPointType)
    {
        if (mapPointType.IsCalamity() || mapPointType.IsTrial())
        {
            mapPointType = MapPointType.Unknown;
        }
    }
}

/// <summary>IconName throws on unknown types; give it any valid name, then UpdateIcon swaps in our textures.</summary>
[HarmonyPatch(typeof(NNormalMapPoint), nameof(NNormalMapPoint.IconName))]
internal static class MapPointIconNamePatch
{
    private static bool Prefix(MapPointType pointType, ref string __result)
    {
        if (!pointType.IsCalamity() && !pointType.IsTrial())
        {
            return true;
        }
        __result = "map_elite";
        return false;
    }
}

[HarmonyPatch(typeof(NNormalMapPoint), nameof(NNormalMapPoint.UpdateIcon))]
internal static class MapPointUpdateIconPatch
{
    private static void Postfix(NNormalMapPoint __instance)
    {
        MapPointType type = __instance.Point.PointType;
        string? name = type.IsCalamity() ? "calamity" : type.IsTrial() ? "trial" : null;
        if (name == null)
        {
            return;
        }
        __instance._icon.Texture = ResourceLoader.Load<Texture2D>($"res://Calamity/images/map/{name}.png", null, ResourceLoader.CacheMode.Reuse);
        __instance._outline.Texture = ResourceLoader.Load<Texture2D>($"res://Calamity/images/map/{name}_outline.png", null, ResourceLoader.CacheMode.Reuse);
    }
}

/// <summary>Top-bar room hover tip; same throwing-switch problem.</summary>
[HarmonyPatch(typeof(NTopBarRoomIcon), nameof(NTopBarRoomIcon.GetHoverTipPrefixForRoomType))]
internal static class TopBarHoverTipPatch
{
    private static bool Prefix(NTopBarRoomIcon __instance, ref string __result)
    {
        MapPointType type = __instance.GetCurrentMapPointType();
        if (type.IsCalamity())
        {
            __result = "ROOM_CALAMITY"; // localization/eng/static_hover_tips.json
            return false;
        }
        if (type.IsTrial())
        {
            __result = "ROOM_TRIAL";
            return false;
        }
        return true;
    }
}
