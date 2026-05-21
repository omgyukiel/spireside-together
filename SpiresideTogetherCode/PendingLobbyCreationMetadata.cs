namespace SpiresideTogether.SpiresideTogetherCode;

public static class PendingLobbyCreationMetadata
{
    private static string? _description;

    public static void SetDescription(string? description)
    {
        _description = SteamLobbyMetadata.NormalizeDescription(description);
    }

    public static bool TryConsumePublicLobbyDescription(out string description)
    {
        if (_description == null)
        {
            description = "";
            return false;
        }

        description = _description;
        _description = null;
        return true;
    }
}
