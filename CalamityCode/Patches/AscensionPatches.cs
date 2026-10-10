using HarmonyLib;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace Calamity.CalamityCode.Patches;

// Raises the ascension cap from 10 to CalamityAscension.Max. Vanilla's cap is a const (inlined at every use), so
// each place that enforces it is patched individually. Level names/descriptions live in localization/eng/ascension.json.
// Without the mod, vanilla clamps saved values back to 10 on load, which is harmless.

/// <summary>Winning a singleplayer run on your max ascension unlocks the next one, past 10.</summary>
[HarmonyPatch(typeof(ProgressSaveManager), nameof(ProgressSaveManager.IncrementSingleplayerAscension))]
internal static class IncrementSingleplayerAscensionPatch
{
    private static void Postfix(SerializableRun run, CharacterStats charStats)
    {
        // Vanilla already handled < 10; it no-ops at exactly 10.
        if (run.Ascension == charStats.MaxAscension && charStats.MaxAscension >= CalamityAscension.VanillaMax && charStats.MaxAscension < CalamityAscension.Max)
        {
            charStats.MaxAscension++;
            charStats.PreferredAscension = charStats.MaxAscension;
        }
    }
}

[HarmonyPatch(typeof(ProgressSaveManager), nameof(ProgressSaveManager.IncrementMultiplayerAscension))]
internal static class IncrementMultiplayerAscensionPatch
{
    private static void Postfix(ProgressSaveManager __instance, SerializableRun run)
    {
        ProgressState progress = __instance.Progress;
        if (run.Ascension == progress.MaxMultiplayerAscension && progress.MaxMultiplayerAscension >= CalamityAscension.VanillaMax && progress.MaxMultiplayerAscension < CalamityAscension.Max)
        {
            progress.MaxMultiplayerAscension++;
            progress.PreferredMultiplayerAscension = progress.MaxMultiplayerAscension;
        }
    }
}

/// <summary>
/// The dev console's "unlock ascensions" sets everything to 10; extend it to our max. Only runs when typed, so the
/// mod itself never unlocks anything.
/// </summary>
[HarmonyPatch(typeof(UnlockConsoleCmd), nameof(UnlockConsoleCmd.UnlockAscensions))]
internal static class UnlockAscensionsConsolePatch
{
    private static void Postfix()
    {
        // Mirrors vanilla's body with our cap.
        ProgressState progress = SaveManager.Instance.Progress;
        progress.MaxMultiplayerAscension = CalamityAscension.Max;
        foreach (CharacterModel character in ModelDb.AllCharacters)
        {
            progress.GetOrCreateCharacterStats(character.Id).MaxAscension = CalamityAscension.Max;
        }
    }
}

/// <summary>Load-time validation clamps character ascension to 10; keep values up to our max.</summary>
[HarmonyPatch(typeof(ProgressState), nameof(ProgressState.ClampCharacterStatsFields))]
internal static class ClampCharacterStatsFieldsPatch
{
    private static void Prefix(CharacterStats stats, out (int max, int preferred) __state)
    {
        __state = (stats.MaxAscension, stats.PreferredAscension);
    }

    private static void Postfix(CharacterStats stats, (int max, int preferred) __state)
    {
        if (__state.max is > CalamityAscension.VanillaMax and <= CalamityAscension.Max)
        {
            stats.MaxAscension = __state.max;
        }
        if (__state.preferred is > CalamityAscension.VanillaMax and <= CalamityAscension.Max)
        {
            stats.PreferredAscension = __state.preferred;
        }
    }
}

/// <summary>Same clamp for the multiplayer ascension fields.</summary>
[HarmonyPatch(typeof(ProgressState), nameof(ProgressState.ClampAscension))]
internal static class ClampAscensionPatch
{
    private static bool Prefix(int value, ref int __result)
    {
        if (value is > CalamityAscension.VanillaMax and <= CalamityAscension.Max)
        {
            __result = value;
            return false;
        }
        return true;
    }
}
