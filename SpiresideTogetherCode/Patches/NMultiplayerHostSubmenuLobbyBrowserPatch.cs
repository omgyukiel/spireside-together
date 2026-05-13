using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using SpiresideTogether.SpiresideTogetherCode;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

[HarmonyPatch(typeof(NMultiplayerHostSubmenu), nameof(NMultiplayerHostSubmenu._Ready))]
public static class NMultiplayerHostSubmenuLobbyBrowserPatch
{
    private const string BrowserRootName = "SpiresideTogetherHostLobbyBrowserProbe";

    private static void Postfix(NMultiplayerHostSubmenu __instance)
    {
        Node submenuNode = __instance;

        if (submenuNode.GetNodeOrNull<Node>(BrowserRootName) != null)
        {
            return;
        }

        Control browser = SteamLobbyBrowserPanel.Create(
            BrowserRootName,
            "Steam Lobby Browser Probe",
            joinLobby: null);
        submenuNode.AddChild(browser);

        MainFile.Logger.Info("Added read-only Steam lobby browser probe to host submenu.");
    }
}
