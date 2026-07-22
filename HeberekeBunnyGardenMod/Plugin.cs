using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
#if BIE6
using BepInEx.Unity.Mono;
#endif
using BunnyGardenFixMod.Utils;
using HarmonyLib;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BunnyGardenFixMod;

public enum AntiAliasingType
{
    Off,
    FXAA,
    TAA,
    MSAA2x,
    MSAA4x,
    MSAA8x,
}

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public static ConfigEntry<int> ConfigWidth;
    public static ConfigEntry<int> ConfigHeight;
    public static ConfigEntry<int> ConfigFrameRate;
    public static ConfigEntry<AntiAliasingType> ConfigAntiAliasing;
    public static ConfigEntry<Key> ConfigTimeStopKey;
    public static ConfigEntry<Key> ConfigSlowMotionKey;
    public static ConfigEntry<Key> ConfigFastForwardKey;
    public static ConfigEntry<float> ConfigSlowMotionScale;
    public static ConfigEntry<float> ConfigFastForwardSpeed;

    internal new static ManualLogSource Logger;

    private void Awake()
    {
        ConfigWidth = Config.Bind(
            "Resolution",              // セクション名
            "Width",                   // キー名
            1920,                      // デフォルト値
            "解像度の幅（横）を指定します"); // 説明

        ConfigHeight = Config.Bind(
            "Resolution",
            "Height",
            1080,
            "解像度の高さ（縦）を指定します");

        ConfigFrameRate = Config.Bind(
            "Resolution",
            "FrameRate",
            60,
            "フレームレート上限を指定します。-1にすると上限を撤廃します。");

        ConfigAntiAliasing = Config.Bind(
            "AntiAliasing",
            "AntiAliasingType",
            AntiAliasingType.MSAA8x,
            "アンチエイリアシングの種類を指定します。Off / FXAA / TAA / MSAA2x / MSAA4x / MSAA8x");

        ConfigTimeStopKey = Config.Bind(
            "Time", "ToggleTimeStopKey", Key.T,
            "時間停止をトグルするキー。");
        ConfigSlowMotionKey = Config.Bind(
            "Time", "ToggleSlowMotionKey", Key.Y,
            "スロー再生をトグルするキー。");
        ConfigFastForwardKey = Config.Bind(
            "Time", "FastForwardKey", Key.G,
            "押している間だけ早送りするキー。");
        ConfigSlowMotionScale = Config.Bind(
            "Time", "SlowMotionScale", 0.25f,
            "スロー再生の速度（1.0 が通常速度）。小さいほどゆっくりになります。");
        ConfigFastForwardSpeed = Config.Bind(
            "Time", "FastForwardSpeed", 2.0f,
            "早送りの速度（1.0 が通常速度）。大きいほど速くなります。");

        // Plugin startup logic
        Logger = base.Logger;
        PatchLogger.Initialize(Logger);
        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();
        Patches.TimeController.Initialize(gameObject);
        PatchLogger.LogInfo($"解像度パッチを適用しました: {Plugin.ConfigWidth.Value}x{Plugin.ConfigHeight.Value}");
        PatchLogger.LogInfo($"アンチエイリアシング設定: {Plugin.ConfigAntiAliasing.Value}");
    }
}
