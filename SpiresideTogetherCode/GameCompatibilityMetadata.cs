using MegaCrit.Sts2.Core.Debug;

namespace SpiresideTogether.SpiresideTogetherCode;

public static class GameCompatibilityMetadata
{
    public const string LobbyGameVersionKey = SteamLobbyMetadata.GameVersionKey;

    public static string CurrentGameVersion =>
        ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? GitHelper.ShortCommitId ?? "UNKNOWN";
}
