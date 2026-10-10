using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;

namespace Calamity.CalamityCode.Trials;

/// <summary>
/// Dev console: <c>trial awakened</c> (Act 3: <c>trial awakened champion</c>, one per boss) gives you a Trial Writ for
/// the current act, exactly as the Trial event would. Then <c>travel</c> to the boss to test the writ applying it. The
/// game picks up console commands from mods by reflection, as BaseLib's own do.
/// </summary>
public sealed class TrialConsoleCmd : AbstractConsoleCmd
{
    public override string CmdName => "trial";

    public override string Args => "<trial:string> [trial2:string]";

    public override string Description =>
        "Gives a Trial Writ for this act's boss (two trials in Act 3, one per boss). Trials: " +
        string.Join(", ", Enum.GetNames<TrialKind>()).ToLowerInvariant();

    // Changes run state, so every client runs it (like the vanilla relic command).
    public override bool IsNetworked => true;

    public override CmdResult Process(Player? issuingPlayer, string[] args)
    {
        if (issuingPlayer == null)
        {
            return new CmdResult(success: false, "A run is currently not in progress!");
        }
        if (args.Length == 0)
        {
            return new CmdResult(success: false, "Name a trial: " + Description);
        }
        List<TrialKind> trials = [];
        foreach (string arg in args.Take(2))
        {
            if (!Enum.TryParse(arg, ignoreCase: true, out TrialKind kind) || !Enum.IsDefined(kind))
            {
                return new CmdResult(success: false, $"Unknown trial '{arg}'. " + Description);
            }
            trials.Add(kind);
        }
        var writ = (TrialWrit)ModelDb.Relic<TrialWrit>().ToMutable();
        writ.Record(issuingPlayer.RunState.CurrentActIndex, trials);
        TaskHelper.RunSafely(RelicCmd.Obtain(writ, issuingPlayer));
        return new CmdResult(success: true, $"Trial Writ for act {issuingPlayer.RunState.CurrentActIndex + 1}: {string.Join(", ", trials)}. Use 'travel' to reach the boss.");
    }

    public override CompletionResult GetArgumentCompletions(Player? player, string[] args)
    {
        List<string> candidates = Enum.GetNames<TrialKind>().Select(n => n.ToLowerInvariant()).ToList();
        return CompleteArgument(candidates, Array.Empty<string>(), args.LastOrDefault() ?? "");
    }
}
