using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;

namespace SpiresideTogether.SpiresideTogetherCode;

//You're recommended but not required to keep all your code in this package and all your assets in the SpiresideTogether folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "SpiresideTogether"; //At the moment, this is used only for the Logger and harmony names.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } = new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        harmony.PatchAll();
        Logger.Info("Initialized SpiresideTogether");
        HarmonyProbeTarget();
    }
    
    public static void HarmonyProbeTarget()
    {
        Logger.Info("harmony probe target original method ran");
    }
}

[HarmonyPatch(typeof(MainFile), nameof(MainFile.HarmonyProbeTarget))]
public static class HarmonyProbeTargetPatch
{
    public static void Prefix()
    {
        MainFile.Logger.Info("Harmony probe prefix ran");
    }
    
    public static void Postfix()
    {
        MainFile.Logger.Info("Harmony probe postfix ran");
    }
}
