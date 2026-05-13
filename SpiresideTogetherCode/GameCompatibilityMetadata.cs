using MegaCrit.Sts2.Core.Debug;

namespace SpiresideTogether.SpiresideTogetherCode;

public static class GameCompatibilityMetadata
{
    public const string LobbyGameVersionKey = "sts2_game_version";

    public static string CurrentGameVersion =>
        ReleaseInfoManager.Instance.ReleaseInfo?.Version ?? GitHelper.ShortCommitId ?? "UNKNOWN";
}
