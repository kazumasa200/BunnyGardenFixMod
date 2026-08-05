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
    private static MethodBase TargetMethod() =>
        AccessTools.Method("GB.GBSystem:Setup");

    private static void Postfix()
    {
        if (Configs.FrameRate.Value < 0)
        {
            // -1なら上限撤廃
            Application.targetFrameRate = -1;
            PatchLogger.LogInfo("フレームレートの上限を撤廃しました");
            return;
        }
        // 指定したフレームレートに設定
        Application.targetFrameRate = Configs.FrameRate.Value;
        PatchLogger.LogInfo($"フレームレートを {Configs.FrameRate.Value} FPS に設定しました");
    }
}
