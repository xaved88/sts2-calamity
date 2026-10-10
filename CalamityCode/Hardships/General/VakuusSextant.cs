using HarmonyLib;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode.Hardships.General;

/// <summary>
/// Vakuu casts a random ballot on every map vote. Vanilla already resolves map votes by drawing one ballot at random
/// (on the host; singleplayer goes through the same path), so Vakuu's ballot hijacks the path 1 time in (players + 1).
/// </summary>
public sealed class VakuusSextant : HardshipRelic
{
    public override HardshipTier Tier => HardshipTier.Crippling;
}

[HarmonyPatch(typeof(MapSelectionSynchronizer), nameof(MapSelectionSynchronizer.MoveToMapCoord))]
internal static class VakuusSextantVotePatch
{
    private static void Prefix(MapSelectionSynchronizer __instance, out MapVote? __state)
    {
        __state = null;
        RunState runState = __instance._runState;
        VakuusSextant? sextant = runState.Players.Select(p => p.GetRelic<VakuusSextant>()).FirstOrDefault(r => r != null);
        MapPoint? from = runState.CurrentMapPoint ?? runState.Map?.StartingMapPoint;
        if (sextant == null || from == null)
        {
            return;
        }
        List<MapPoint> options = MapTravel.GetTravelablePointsFrom(runState, from).ToList();
        MapPoint? pick = __instance._multiplayerMapPointSelection.NextItem(options);
        if (pick == null)
        {
            return;
        }
        // Vakuu's ballot goes in the hat with everyone else's for this one draw, then comes back out.
        __state = new MapVote { coord = pick.coord, mapGenerationCount = __instance.MapGenerationCount };
        __instance._votes.Add(__state);
        sextant.Flash();
        MainFile.Logger.Info($"Vakuu votes for {pick.coord}.");
    }

    private static void Postfix(MapSelectionSynchronizer __instance, MapVote? __state)
    {
        if (__state.HasValue)
        {
            __instance._votes.RemoveAt(__instance._votes.Count - 1);
        }
    }
}
