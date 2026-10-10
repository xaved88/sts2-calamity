using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Shops;

namespace Calamity.CalamityCode.Hardships.Underdocks;

/// <summary>
/// Smuggler's Padlock empties a shop section's entries, but the shop scene has a fixed set of slots and shows the empty
/// ones with their placeholder price ("99") and sale tags. Blank out every slot left without an entry (transparent and
/// non-interactive, but still taking its space so the shop layout doesn't shift) and put a padlock over each closed
/// section. Only touches shops while someone holds the padlock.
/// </summary>
[HarmonyPatch(typeof(NMerchantInventory), nameof(NMerchantInventory.Initialize))]
internal static class SmugglersPadlockShopUiPatch
{
    private const string PadlockTexture = "res://Calamity/images/relics/smugglers_padlock.png";
    private const string PadlockName = "CalamityPadlock";
    private const float MaxPadlockSize = 180f;

    private static void Postfix(NMerchantInventory __instance)
    {
        if (__instance.Inventory?.Player.RunState.Players.Any(p => p.GetRelic<SmugglersPadlock>() != null) != true)
        {
            return;
        }
        var closedSections = new List<Control>();
        foreach (Control? section in new Control?[] { __instance._characterCardContainer, __instance._colorlessCardContainer, __instance._relicContainer, __instance._potionContainer })
        {
            List<NMerchantSlot> slots = section?.GetChildren().OfType<NMerchantSlot>().ToList() ?? [];
            if (slots.Count > 0 && slots.All(s => s.Entry == null))
            {
                slots.ForEach(Blank);
                closedSections.Add(section!);
            }
        }
        if (__instance._cardRemovalNode is { } removal && removal.Entry == null)
        {
            Blank(removal);
            closedSections.Add(removal);
        }
        if (closedSections.Count == 0)
        {
            return;
        }
        // After layout, so the sections have their real size and position.
        Callable.From(() => closedSections.ForEach(AddPadlock)).CallDeferred();
    }

    private static void Blank(NMerchantSlot slot)
    {
        slot.Modulate = Colors.Transparent;
        slot.MouseFilter = Control.MouseFilterEnum.Ignore;
        slot.FocusMode = Control.FocusModeEnum.None;
        if (slot._hitbox is { } hitbox)
        {
            hitbox.MouseFilter = Control.MouseFilterEnum.Ignore;
            hitbox.FocusMode = Control.FocusModeEnum.None;
        }
    }

    private static void AddPadlock(Control section)
    {
        // Containers re-arrange their children, so hang the padlock off the nearest ancestor that isn't one. That
        // ancestor sits inside the shop's sliding panel, so the padlock moves with it.
        Node? host = section.GetParent();
        while (host is Container)
        {
            host = host.GetParent();
        }
        if (host is not Control anchor || anchor.GetNodeOrNull(PadlockName + section.Name) != null)
        {
            return;
        }
        Rect2 rect = anchor.GetGlobalTransform().AffineInverse() * section.GetGlobalRect();
        float size = Mathf.Min(MaxPadlockSize, Mathf.Min(rect.Size.X, rect.Size.Y) * 0.8f);
        if (size <= 0f)
        {
            size = MaxPadlockSize;
        }
        var padlock = new TextureRect
        {
            Name = PadlockName + section.Name,
            Texture = ResourceLoader.Load<Texture2D>(PadlockTexture, null, ResourceLoader.CacheMode.Reuse),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            Size = new Vector2(size, size),
            Position = rect.GetCenter() - new Vector2(size, size) / 2f,
            Modulate = new Color(1f, 1f, 1f, 0.9f),
        };
        anchor.AddChild(padlock);
    }
}

/// <summary>
/// Vanilla's default focus (used by controllers) reads Entry.IsStocked on every slot, which throws on the slots the
/// padlock left empty. Same logic, null-safe.
/// </summary>
[HarmonyPatch(typeof(NMerchantInventory), nameof(NMerchantInventory.DefaultFocusedControl), MethodType.Getter)]
internal static class MerchantDefaultFocusNullSafePatch
{
    private static bool Prefix(NMerchantInventory __instance, ref Control? __result)
    {
        if (__instance._isInputBlocked)
        {
            return true;
        }
        __result = __instance._lastSlot is { Entry.IsStocked: true } last
            ? last
            : __instance.GetAllSlots().FirstOrDefault(s => s.Entry is { IsStocked: true });
        return false;
    }
}
