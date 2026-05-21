namespace SpiresideTogether.SpiresideTogetherCode;

public static class SteamLobbyMetadata
{
    public const string ModMarkerKey = "spireside_together";
    public const string ModMarkerValue = "1";
    public const string NameKey = "name";
    public const string DescriptionKey = "description";
    public const string GameVersionKey = "sts2_game_version";

    public const int MaxHostNameLength = 16;
    public const int MaxDescriptionLength = 47;
    public const int MaxGameVersionLength = 16;

    public static string NormalizeHostName(string? value)
    {
        return Normalize(value, "Unnamed host", MaxHostNameLength);
    }

    public static string NormalizeDescription(string? value)
    {
        return Normalize(value, "Public Spireside Together lobby", MaxDescriptionLength);
    }

    public static string NormalizeGameVersion(string? value)
    {
        return Normalize(value, "unknown", MaxGameVersionLength);
    }

    private static string Normalize(string? value, string fallback, int maxLength)
    {
        string normalized = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        if (normalized.Length <= maxLength)
        {
            return normalized;
        }

        return normalized[..maxLength];
    }
}
