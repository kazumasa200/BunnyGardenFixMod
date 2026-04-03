using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace BunnyGardenFixMod.Patches;

/// <summary>
/// フルスクリーンの解像度計算を上書きするパッチ
/// </summary>
[HarmonyPatch]
public class CalcFullScreenResolutionPatch
{
    static MethodBase TargetMethod() =>
        AccessTools.Method("GB.GBSystem:CalcFullScreenResolution");

    private static bool Prefix(ref ValueTuple<int, int, bool> __result)
    {
        // コンフィグから値を取得
        int num = Plugin.ConfigWidth.Value;
        int num2 = Plugin.ConfigHeight.Value;
        bool flag = true;
        float num3 = (float)num / (float)num2;
        Resolution currentResolution = Screen.currentResolution;
        float num4 = (float)currentResolution.width / (float)currentResolution.height;

        if (num4 > num3)
        {
            num2 = Mathf.Min(num2, currentResolution.height);
            num = (int)((float)num2 * num3);
            flag = false;
        }
        else if (num4 < num3)
        {
            num = Mathf.Min(num, currentResolution.width);
            num2 = (int)((float)num / num3);
        }

        __result = new ValueTuple<int, int, bool>(num, num2, flag);

        return false;
    }
}
