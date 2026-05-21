namespace SpiresideTogether.SpiresideTogetherCode;

public static class LobbyIdParser
{
    public static bool TryParseLobbyId(string? rawValue, out ulong lobbyId)
    {
        lobbyId = 0;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        return ulong.TryParse(rawValue.Trim(), out lobbyId) && lobbyId != 0;
    }
}
