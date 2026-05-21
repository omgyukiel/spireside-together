namespace SpiresideTogether.SpiresideTogetherCode;

public sealed class SteamLobbyBrowserEntry
{
    public required ulong LobbyId { get; init; }

    public required ulong OwnerId { get; init; }

    public required int MemberCount { get; init; }

    public required int MemberLimit { get; init; }

    public string Name { get; init; } = "";

    public string Description { get; init; } = "";

    public string GameVersion { get; init; } = "";
}
