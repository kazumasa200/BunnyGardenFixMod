#if BIE6
using BepInEx.Unity.Mono;
#endif

using System;
using System.IO;
using System.Linq;
using BepInEx;
using BepInEx.Logging;
using GB;
using HarmonyLib;
using BunnyGardenFixMod.Utils;
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
    private static Plugin s_instance;

    internal static event Action GUICallback;

    internal new static ManualLogSource Logger;

    private void Awake()
    {
        s_instance = this;
        Logger = base.Logger;
        PatchLogger.Initialize(Logger);
        ConfigMigration.Migrate(Config);

        // YAML 駆動 Config（source of truth: Configs.yaml → Generated/Configs.g.cs）。
        Configs.BindAll(Config);

        Patches.Settings.SettingsCollapseState.Init(Config);

        var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        // 注意: バニーガーデン1本体は起動時に MOD 外部の GameObject を破棄するため、
        // MOD コンポーネントの生成はここではなく、ゲーム永続 GO(GBSystem) が用意された後
        // （GBSystem.Setup の Postfix = ModInitPatch）に InitializeGameObjects で行う。

        PatchLogger.LogInfo($"プラグイン起動: {MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION}");
        PatchLogger.LogInfo($"解像度パッチを適用しました: {Configs.Width.Value}x{Configs.Height.Value}");
        PatchLogger.LogInfo($"アンチエイリアシング設定: {Configs.AntiAliasing.Value}");
    }

    private void OnDestroy()
    {
        if (s_instance == this)
            s_instance = null;
    }

    /// <summary>
    /// MOD の毎フレーム処理コンポーネントを、ゲーム所有の永続 GameObject に載せる。
    /// プラグイン GO は本体に破棄されるため、必ずゲーム側 GO を host に渡すこと。
    /// GBSystem.Setup の Postfix から呼ばれる。二重付与は host 上の ModDriver 有無で防ぐ。
    /// </summary>
    internal static void InitializeGameObjects(GameObject host)
    {
        if (host == null || host.GetComponent<Patches.ModDriver>() != null)
            return;

        // 注意: プラグイン GO は本体に破棄され Plugin.Instance は null になっている
        // 可能性が高い。ここでは Plugin.Instance に依存せず初期化すること
        // （依存すると FreeCameraManager だけ生成されないバグが再発する）。
        host.AddComponent<Patches.ModDriver>();
        Patches.Settings.SettingsController.Initialize(host);
        Patches.FreeCamera.FreeCameraManager.Initialize(host);
        Patches.TimeController.Initialize(host);

        PatchLogger.LogInfo($"MOD コンポーネントをゲーム永続GO({host.name})へ初期化しました");
    }

    internal static void ReloadConfig() => s_instance?.Config.Reload();

    internal static void InvokeGUICallback() => GUICallback?.Invoke();

    internal static void SaveScreenshot()
    {
        var dir = Path.Combine(Paths.BepInExRootPath, "screenshots", MyPluginInfo.PLUGIN_GUID);
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, $"bg1_{DateTime.Now:yyyyMMdd_HHmmss_fff}.png");
        ScreenCapture.CaptureScreenshot(path, Configs.ScreenshotScale.Value);
        PatchLogger.LogInfo($"スクリーンショットを保存しました: {path}");
    }

    internal static void ReleaseFreeCameraFor(string uiName)
    {
        // Plugin.Instance はプラグイン GO ごと破棄されるため、FreeCameraManager 側の
        // static Instance を経由する。
        var freeCam = Patches.FreeCamera.FreeCameraManager.Instance;
        if (freeCam == null || !Patches.FreeCamera.FreeCameraManager.IsActive)
            return;

        freeCam.Deactivate();
        PatchLogger.LogInfo($"システム UI が開いたためフリーカメラを終了します: {uiName}");
    }


    /// <summary>
    /// バニーガーデン1は Camera.main（MainCamera タグ）を使わないため、
    /// アクティブな環境シーン配下のカメラを優先して取得する。見つからなければ
    /// 有効なカメラのうち depth 最大のものを代替として使う。
    /// </summary>
    internal static Camera FindCurrentCamera()
    {
        try
        {
            var env = GBSystem.Instance != null ? GBSystem.Instance.GetActiveEnvScene() : null;
            if (env != null)
            {
                var envCam = env.GetComponentInChildren<Camera>();
                if (envCam != null)
                {
                    Logger.LogInfo($"EnvScene カメラを使用: {envCam.name}");
                    return envCam;
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning($"EnvScene カメラの取得に失敗: {ex.Message}");
        }

        var cam = Camera.allCameras.OrderByDescending(c => c.depth).FirstOrDefault();
        if (cam == null)
        {
            Logger.LogError("有効なカメラが見つかりません。");
            return null;
        }
        Logger.LogInfo($"代替カメラを使用: {cam.name}");
        return cam;
    }
}

/// <summary>
/// ゲーム永続 GO(GBSystem) の準備完了後に MOD コンポーネントを載せる。
/// プラグイン GO は本体に破棄されるため、ゲーム側 GO へ載せ替えるのが要。
/// </summary>
[HarmonyPatch(typeof(GBSystem), "Setup")]
public static class ModInitPatch
{
    private static void Postfix()
    {
        var host = GBSystem.Instance != null ? GBSystem.Instance.gameObject : null;
        if (host != null)
            Plugin.InitializeGameObjects(host);
    }
}

/// <summary>フリーカメラ中（非固定）はゲーム本体の入力を無効化する。</summary>
[HarmonyPatch(typeof(GBSystem), "IsInputDisabled")]
public static class FreeCamGameInputDisablePatch
{
    private static void Postfix(ref bool __result)
    {
        if (Patches.FreeCamera.FreeCameraManager.IsActive && !Patches.FreeCamera.FreeCameraManager.IsFixed)
            __result = true;
    }
}

/// <summary>終了確認ダイアログが出たらフリーカメラを解除してカーソルを操作可能に戻す。</summary>
[HarmonyPatch(typeof(GBSystem), "confirmQuit")]
public static class QuitConfirmFreeCamReleasePatch
{
    private static void Prefix()
        => Plugin.ReleaseFreeCameraFor("終了確認");
}

/// <summary>
/// F9 設定パネル上（ポインタがパネル矩形内）またはキーバインドキャプチャ中は、
/// マウスクリックがゲーム側に貫通しないよう GBInput.isMouseTriggered を false に差し替える。
/// </summary>
[HarmonyPatch(typeof(GBInput), "isMouseTriggered")]
public class SuppressClickOverPanelPatch
{
    private static bool Prefix(ref bool __result)
    {
        if (Patches.Settings.SettingsController.IsAnyCapturing ||
            Patches.Settings.SettingsController.ShouldSuppressMouseInput())
        {
            __result = false;
            return false;
        }
        return true;
    }
}

/// <summary>
/// F9 設定パネル上またはキャプチャ中は、マウスホイールがゲーム側操作に流れないよう
/// GBInput.ScrollAxis を 0 に差し替える（UI Toolkit 内のスクロールは影響を受けない）。
/// </summary>
[HarmonyPatch]
public class SuppressScrollOverPanelPatch
{
    private static System.Reflection.MethodBase TargetMethod()
        => AccessTools.PropertyGetter(typeof(GBInput), nameof(GBInput.ScrollAxis));

    private static bool Prefix(ref float __result)
    {
        if (Patches.Settings.SettingsController.IsAnyCapturing ||
            Patches.Settings.SettingsController.ShouldSuppressMouseInput())
        {
            __result = 0f;
            return false;
        }
        return true;
    }
}
