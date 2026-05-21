namespace SpiresideTogether.SpiresideTogetherCode;

/// <summary>
/// Holds the lobby id between Steam host creation and the host lobby screen initialization.
/// Steam exposes the id before the native submenu is pushed, so the screen patch consumes this
/// value when there is a real UI parent for the lobby id widget.
/// </summary>
public static class PendingHostLobbyIdDisplay
{
    private static string? _lobbyId;

    public static void Set(string lobbyId)
    {
        _lobbyId = lobbyId;
    }

    public static bool TryConsume(out string lobbyId)
    {
        if (_lobbyId == null)
        {
            lobbyId = "";
            return false;
        }

        lobbyId = _lobbyId;
        _lobbyId = null;
        return true;
    }
}
