using BunnyGardenFixMod.Utils;
using HarmonyLib;
using System.Reflection;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace BunnyGardenFixMod.Patches;

/// <summary>
/// カメラのアンチエイリアシング設定を上書きするパッチ
/// </summary>
[HarmonyPatch(
    typeof(UniversalAdditionalCameraData),
    nameof(UniversalAdditionalCameraData.antialiasing),
    MethodType.Setter
)]
public static class AntiAliasingSetterPatch
{
    private static void Prefix(ref AntialiasingMode value)
    {
        value = Configs.AntiAliasing.Value switch
        {
            AntiAliasingType.Off => AntialiasingMode.None,
            AntiAliasingType.FXAA => AntialiasingMode.FastApproximateAntialiasing,
            AntiAliasingType.TAA => AntialiasingMode.TemporalAntiAliasing,
            // MSAA はポストプロセスAAをオフにして別途 msaaSampleCount で設定
            _ => AntialiasingMode.None,
        };
    }
}

/// <summary>
/// MSAA のサンプル数を設定するパッチ
/// </summary>
[HarmonyPatch]
public static class MsaaSetupPatch
{
    private static MethodBase TargetMethod() =>
        AccessTools.Method("GB.GBSystem:Setup");

    private static void Postfix()
    {
        int msaaSamples = Configs.AntiAliasing.Value switch
        {
            AntiAliasingType.MSAA2x => 2,
            AntiAliasingType.MSAA4x => 4,
            AntiAliasingType.MSAA8x => 8,
            _ => 1,
        };

        if (msaaSamples > 1 && GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset urpAsset)
        {
            urpAsset.msaaSampleCount = msaaSamples;
            PatchLogger.LogInfo($"MSAA を {msaaSamples}x に設定しました");
        }
    }
}
