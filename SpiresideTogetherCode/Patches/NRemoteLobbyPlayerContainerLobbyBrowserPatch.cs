using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using SpiresideTogether.SpiresideTogetherCode;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

[HarmonyPatch(typeof(NRemoteLobbyPlayerContainer), nameof(NRemoteLobbyPlayerContainer._Ready))]
public static class NRemoteLobbyPlayerContainerLobbyBrowserPatch
{
    private const string BrowserRootName = "SpiresideTogetherActiveHostLobbyBrowserProbe";

    private static void Postfix(NRemoteLobbyPlayerContainer __instance)
    {
        Node containerNode = __instance;

        if (containerNode.GetNodeOrNull<Node>(BrowserRootName) != null)
        {
            return;
        }

        Control browser = SteamLobbyBrowserPanel.Create(
            BrowserRootName,
            "Active Host Browser Probe",
            joinLobby: null);
        containerNode.AddChild(browser);

        MainFile.Logger.Info("Added read-only Steam lobby browser probe to active lobby player container.");
    }
}
