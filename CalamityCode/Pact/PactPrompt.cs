using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Platform;
using MegaCrit.Sts2.Core.Runs;

namespace Calamity.CalamityCode.Pact;

/// <summary>
/// The banner shown while a dead player chooses who bears their Pact. Built from plain Godot nodes in code (the PCK
/// packer can't pack .tscn scenes). Text lives in localization/eng/static_hover_tips.json.
/// </summary>
internal sealed class PactPrompt : IDisposable
{
    private readonly Control _root;

    private PactPrompt(string bbcode, Player? volunteer, Player dead)
    {
        _root = new VBoxContainer
        {
            Name = "CalamityPactPrompt",
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Alignment = BoxContainer.AlignmentMode.Center,
        };
        _root.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
        _root.Position = new Vector2(-360f, 140f);
        _root.CustomMinimumSize = new Vector2(720f, 0f);

        var panel = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        panel.AddThemeStyleboxOverride("panel", new StyleBoxFlat
        {
            BgColor = new Color(0.08f, 0.02f, 0.05f, 0.88f),
            BorderColor = new Color(0.55f, 0.08f, 0.12f),
            BorderWidthBottom = 3, BorderWidthTop = 3, BorderWidthLeft = 3, BorderWidthRight = 3,
            CornerRadiusBottomLeft = 10, CornerRadiusBottomRight = 10, CornerRadiusTopLeft = 10, CornerRadiusTopRight = 10,
            ContentMarginLeft = 24, ContentMarginRight = 24, ContentMarginTop = 16, ContentMarginBottom = 16,
        });
        var label = new RichTextLabel
        {
            BbcodeEnabled = true,
            FitContent = true,
            ScrollActive = false,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(672f, 0f),
            Text = $"[center]{bbcode}[/center]",
        };
        label.AddThemeFontSizeOverride("normal_font_size", 26);
        label.AddThemeFontSizeOverride("bold_font_size", 26);
        panel.AddChild(label);
        _root.AddChild(panel);

        if (volunteer != null)
        {
            var offer = new Button { Text = Loc("CALAMITY_PACT.offer").GetFormattedText(), FocusMode = Control.FocusModeEnum.None };
            offer.AddThemeFontSizeOverride("font_size", 26);
            offer.Pressed += () =>
            {
                offer.Disabled = true;
                CustomMessageWrapper.Send(new PactVolunteerMessage { deadPlayerId = dead.NetId });
            };
            _root.AddChild(offer);
        }

        NRun.Instance?.GlobalUi.AddChild(_root);
    }

    public static PactPrompt ShowForDead(Player dead)
    {
        return new PactPrompt(Loc("CALAMITY_PACT.dead").GetFormattedText(), null, dead);
    }

    public static PactPrompt ShowForAlly(Player dead, IReadOnlyList<Player> living)
    {
        LocString text = Loc("CALAMITY_PACT.ally");
        text.Add("Name", PlatformUtil.GetPlayerName(RunManager.Instance.NetService.Platform, dead.NetId));
        Player? me = living.FirstOrDefault(LocalContext.IsMe);
        return new PactPrompt(text.GetFormattedText(), me, dead);
    }

    private static LocString Loc(string key) => new("static_hover_tips", key);

    public void Dispose()
    {
        if (GodotObject.IsInstanceValid(_root))
        {
            _root.QueueFree();
        }
    }
}
