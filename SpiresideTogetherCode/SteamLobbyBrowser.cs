using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Multiplayer.Transport.Steam;
using MegaCrit.Sts2.Core.Platform.Steam;
using Steamworks;

namespace SpiresideTogether.SpiresideTogetherCode;

public static class SteamLobbyBrowser
{
    public static async Task<IReadOnlyList<SteamLobbyBrowserEntry>> RequestPublicLobbies()
    {
        if (!SteamInitializer.Initialized)
        {
            MainFile.Logger.Warn("Cannot request Steam lobby list because SteamInitializer is not initialized.");
            return new List<SteamLobbyBrowserEntry>();
        }

        SteamMatchmaking.AddRequestLobbyListResultCountFilter(50);

        SteamAPICall_t call = SteamMatchmaking.RequestLobbyList();
        using SteamCallResult<LobbyMatchList_t> callResult = new(call);
        LobbyMatchList_t result = await callResult.Task;

        List<SteamLobbyBrowserEntry> entries = new();
        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);
            CSteamID ownerId = SteamMatchmaking.GetLobbyOwner(lobbyId);
            string name = SteamMatchmaking.GetLobbyData(lobbyId, "name");
            string gameVersion = SteamMatchmaking.GetLobbyData(lobbyId, GameCompatibilityMetadata.LobbyGameVersionKey);

            entries.Add(new SteamLobbyBrowserEntry
            {
                LobbyId = lobbyId.m_SteamID,
                OwnerId = ownerId.m_SteamID,
                MemberCount = SteamMatchmaking.GetNumLobbyMembers(lobbyId),
                MemberLimit = SteamMatchmaking.GetLobbyMemberLimit(lobbyId),
                Name = name,
                GameVersion = gameVersion
            });
        }

        MainFile.Logger.Info($"Steam lobby browser found {entries.Count} lobbies.");
        return entries;
    }
}
