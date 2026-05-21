using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using SpiresideTogether.SpiresideTogetherCode;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

/// <summary>
/// Attaches the host lobby id widget to the active multiplayer host lobby screen. Keeping this
/// scene under the submenu makes the native menu lifecycle remove it when the player backs out.
/// </summary>
public static class NHostLobbyIdScenePatchHelper
{
    public static void AttachPendingLobbyId(Node hostLobbyScreen)
    {
        if (!PendingHostLobbyIdDisplay.TryConsume(out string lobbyId))
        {
            return;
        }

        SpiresideLobbyUiScenes.ShowHostLobbyId(hostLobbyScreen, lobbyId);
    }
}

[HarmonyPatch(typeof(NCharacterSelectScreen), nameof(NCharacterSelectScreen.InitializeMultiplayerAsHost))]
public static class NCharacterSelectScreenHostLobbyIdPatch
{
    private static void Postfix(NCharacterSelectScreen __instance)
    {
        NHostLobbyIdScenePatchHelper.AttachPendingLobbyId(__instance);
    }
}

[HarmonyPatch(typeof(NCustomRunScreen), nameof(NCustomRunScreen.InitializeMultiplayerAsHost))]
public static class NCustomRunScreenHostLobbyIdPatch
{
    private static void Postfix(NCustomRunScreen __instance)
    {
        NHostLobbyIdScenePatchHelper.AttachPendingLobbyId(__instance);
    }
}

[HarmonyPatch(typeof(NDailyRunScreen), nameof(NDailyRunScreen.InitializeMultiplayerAsHost))]
public static class NDailyRunScreenHostLobbyIdPatch
{
    private static void Postfix(NDailyRunScreen __instance)
    {
        NHostLobbyIdScenePatchHelper.AttachPendingLobbyId(__instance);
    }
}
