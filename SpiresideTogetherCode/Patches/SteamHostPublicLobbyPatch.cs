using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using SpiresideTogether.SpiresideTogetherCode;
using Steamworks;
using System;
using System.Threading.Tasks;

namespace SpiresideTogether.SpiresideTogetherCode.Patches;

/*
   Change the lobby type from friends -> public. In MegaCrit.Sts2.Core.Multiplayer.Transport.Steam,
   the CSteamID is created and stored in the SteamHost instance only when the task result resolves
   to EResult.k_EResultOK. We use this status to implicitly infer the availability of the lobby id
   to modify the lobby type after StartHost 
*/
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
        // null result means successful lobby completion, so return if result has value
        if (result.HasValue || !host.LobbyId.HasValue) return result;
        MainFile.Logger.Info($"Steam lobby id is {host.GetRawLobbyIdentifier()}");
        string gameVersion = GameCompatibilityMetadata.CurrentGameVersion;
        string hostName = SteamLobbyMetadata.NormalizeHostName(GetHostPersonaName());
        string description = SteamLobbyMetadata.NormalizeDescription("Public Spireside Together lobby");
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
