using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace Calamity.CalamityCode.Patches;

/// <summary>
/// Adds "Calamity" under the main menu logo. It's parented to the logo node, so it fades with it when submenus open.
/// Horizontal placement comes from the logo's Spine skeleton bounds (x 611..2185 in logo space); the vertical
/// position was tuned in game so it sits between the bottom of "SPIRE" and the first menu button.
/// </summary>
[HarmonyPatch(typeof(NMainMenuBg), nameof(NMainMenuBg._Ready))]
internal static class MainMenuTitlePatch
{
    private const float LogoLeft = 611f;
    private const float LogoWidth = 1574f;
    private const float Top = 1190f;

    private static void Postfix(NMainMenuBg __instance)
    {
        Node2D logo = __instance._logo;
        if (logo.GetNodeOrNull("CalamityTitle") != null)
        {
            return;
        }
        var label = new Label
        {
            Name = "CalamityTitle",
            Text = "CALAMITY",
            HorizontalAlignment = HorizontalAlignment.Center,
            Position = new Vector2(LogoLeft, Top),
            Size = new Vector2(LogoWidth, 160f),
            MouseFilter = Control.MouseFilterEnum.Ignore,
        };
        if (ResourceLoader.Load<Font>("res://fonts/kreon_bold.ttf") is { } font)
        {
            label.AddThemeFontOverride("font", font);
        }
        label.AddThemeFontSizeOverride("font_size", 112);
        label.AddThemeConstantOverride("outline_size", 16);
        label.AddThemeColorOverride("font_color", new Color(0.78f, 0.1f, 0.14f));
        label.AddThemeColorOverride("font_outline_color", new Color(0.06f, 0.02f, 0.03f));
        logo.AddChild(label);
    }
}
