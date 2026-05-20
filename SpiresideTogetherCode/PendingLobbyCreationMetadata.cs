namespace SpiresideTogether.SpiresideTogetherCode;

public static class PendingLobbyCreationMetadata
{
    private static string? _description;

    public static void SetDescription(string? description)
    {
        _description = SteamLobbyMetadata.NormalizeDescription(description);
    }

    public static string ConsumeDescription()
    {
        string description = _description ?? SteamLobbyMetadata.NormalizeDescription(null);
        _description = null;
        return description;
    }
}
