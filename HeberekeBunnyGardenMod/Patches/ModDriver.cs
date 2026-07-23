using System;
using System.Collections;
using BunnyGardenFixMod.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BunnyGardenFixMod.Patches;

/// <summary>
/// バニーガーデン1本体は起動時に「ゲーム外部(foreign)の GameObject」を破棄するため、
/// MOD の MonoBehaviour をプラグイン GO（BepInEx マネージャ）に載せると Update/OnGUI が
/// 一切回らない。そこでゲーム所有の永続 GO(GBSystem) に本コンポーネントを載せ、
/// オーバーレイ表示・ホットキー・スクリーンショット・起動時アップデート確認など、
/// Plugin 本体が担っていた毎フレーム処理を肩代わりして駆動する。
/// </summary>
public class ModDriver : MonoBehaviour
{
    private bool isOverlayVisible = true;
    private bool isCapturingScreenshot;

    private void Start()
    {
        // アップデート確認のコルーチンも永続 GO 上で回す（プラグイン GO では途中で破棄される）。
        if (Configs.UpdateCheck.Value)
            StartCoroutine(UpdateChecker.Check());
    }

    private void Update()
    {
        if (Keyboard.current?[Key.F4].wasPressedThisFrame == true)
            Plugin.ReloadConfig();

        if (Configs.OverlayToggle.IsTriggered())
        {
            isOverlayVisible = !isOverlayVisible;
            PatchLogger.LogInfo($"表示: {(isOverlayVisible ? "ON" : "OFF")}");
        }

        if (Configs.CaptureScreenshot.IsTriggered())
            StartCoroutine(CaptureScreenshotCoroutine());
    }

    private void OnGUI()
    {
        if (!isOverlayVisible || isCapturingScreenshot)
            return;

        GUILayout.BeginArea(new Rect(10, 10, Screen.width / 2, Screen.height - 10));
        Plugin.InvokeGUICallback();
        GUILayout.EndArea();
    }

    private IEnumerator CaptureScreenshotCoroutine()
    {
        Camera captureCam = Plugin.FindCurrentCamera();
        if (captureCam == null)
            yield break;

        isCapturingScreenshot = true;

        try
        {
            Plugin.SaveScreenshot();
        }
        catch (Exception ex)
        {
            PatchLogger.LogError($"スクリーンショット保存失敗: {ex.Message}");
        }

        // スクショがキャプチャされる前にオーバーレイを再表示しないよう 1 フレーム待機
        yield return null;
        isCapturingScreenshot = false;
    }
}
