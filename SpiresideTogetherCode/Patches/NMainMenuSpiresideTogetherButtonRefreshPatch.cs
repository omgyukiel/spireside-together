using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

/// <summary>
/// Keeps the Spireside Together main-menu entry visually enabled after the native main menu refreshes
/// its button states.
/// </summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu.RefreshButtons))]
public static class NMainMenuSpiresideTogetherButtonRefreshPatch
{
    private static void Postfix(NMainMenu __instance)
    {
        NMainMenuSpiresideTogetherButtonPatch.EnableSpiresideButton(__instance);
    }
}
