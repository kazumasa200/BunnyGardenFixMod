using BunnyGardenFixMod.Utils;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BunnyGardenFixMod.Patches;

/// <summary>
/// フレームレートを設定するパッチ
/// </summary>
[HarmonyPatch]
public class SetRefreshRatePatch
{
    static MethodBase TargetMethod() =>
        AccessTools.Method("GB.GBSystem:Setup");

    private static void Postfix()
    {
        if (Plugin.ConfigFrameRate.Value < 0)
        {
            // -1なら上限撤廃
            Application.targetFrameRate = -1;
            PatchLogger.LogInfo("フレームレートの上限を撤廃しました");
            return;
        }
        // 指定したフレームレートに設定
        Application.targetFrameRate = Plugin.ConfigFrameRate.Value;
        PatchLogger.LogInfo($"フレームレートを {Plugin.ConfigFrameRate.Value} FPS に設定しました");
    }
}
