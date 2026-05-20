using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using SpiresideTogether.SpiresideTogetherCode;
using Steamworks;
using System;
using System.Threading.Tasks;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

/// <summary>
/// Converts newly-created Steam lobbies from friends-only to public and publishes the metadata used by
/// the Spireside Together browser. SteamHost only exposes a lobby id after StartHost completes
/// successfully, so this patch wraps the returned task and updates lobby state after native creation.
/// </summary>
[HarmonyPatch(typeof(SteamHost), nameof(SteamHost.StartHost))]
public class SteamHostPublicLobbyPatch
{
    private static void Postfix(SteamHost __instance, ref Task<NetErrorInfo?> __result)
    {
        __result = MakeLobbyPublicAfterStart(__instance, __result);
    }
    private static async Task<NetErrorInfo?> MakeLobbyPublicAfterStart(
        SteamHost host,
        Task<NetErrorInfo?> startHostTask)
    {
        // Wait for original startHost task to complete before checking LobbyId
        var result = await startHostTask;
        MainFile.Logger.Info($"Steam host startup complete. Error: {result.HasValue.ToString()}");
        string description = PendingLobbyCreationMetadata.ConsumeDescription();
        // null result means successful lobby completion, so return if result has value
        if (result.HasValue || !host.LobbyId.HasValue) return result;
        MainFile.Logger.Info($"Steam lobby id is {host.GetRawLobbyIdentifier()}");
        string gameVersion = GameCompatibilityMetadata.CurrentGameVersion;
        string hostName = SteamLobbyMetadata.NormalizeHostName(GetHostPersonaName());
        SteamMatchmaking.SetLobbyType(host.LobbyId.Value, ELobbyType.k_ELobbyTypePublic);
        SteamMatchmaking.SetLobbyData(host.LobbyId.Value, SteamLobbyMetadata.ModMarkerKey, SteamLobbyMetadata.ModMarkerValue);
        SteamMatchmaking.SetLobbyData(host.LobbyId.Value, SteamLobbyMetadata.NameKey, hostName);
        SteamMatchmaking.SetLobbyData(host.LobbyId.Value, SteamLobbyMetadata.DescriptionKey, description);
        SteamMatchmaking.SetLobbyData(host.LobbyId.Value, SteamLobbyMetadata.GameVersionKey, gameVersion);
        MainFile.Logger.Info($"Steam lobby set to public with game version metadata {gameVersion}.");

        return result;
    }

    private static string GetHostPersonaName()
    {
        try
        {
            return SteamFriends.GetPersonaName();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Could not read Steam persona name for lobby metadata: {ex}");
            return "";
        }
    }
}
