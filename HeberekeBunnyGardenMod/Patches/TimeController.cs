using BunnyGardenFixMod.Utils;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BunnyGardenFixMod.Patches;

/// <summary>
/// 時間停止・スロー再生・早送りを <see cref="Time.timeScale"/> で制御する。
/// 停止／スローはトグル、早送りは押している間のみ有効。優先度は 早送り &gt; 停止 &gt; スロー。
/// キーは Plugin の Config（Time セクション）で変更できる。
/// </summary>
public class TimeController : MonoBehaviour
{
    private bool stop;
    private bool slow;
    private bool fastForward;
    private bool wasControlling;

    public static TimeController Initialize(GameObject parent)
        => parent.AddComponent<TimeController>();

    private void OnDisable()
    {
        Time.timeScale = 1f;
        stop = false;
        slow = false;
    }

    private void Update()
    {
        var kb = Keyboard.current;
        if (kb == null)
            return;

        fastForward = IsHeld(kb, Plugin.ConfigFastForwardKey.Value);

        if (WasPressed(kb, Plugin.ConfigTimeStopKey.Value))
        {
            stop = !stop;
            if (stop) slow = false; // 停止に入るときスローを解除
            PatchLogger.LogInfo($"時間停止: {(stop ? "ON" : "OFF")}");
        }

        if (WasPressed(kb, Plugin.ConfigSlowMotionKey.Value))
        {
            slow = !slow;
            if (slow) stop = false; // スローに入るとき停止を解除
            PatchLogger.LogInfo($"スロー再生: {(slow ? "ON" : "OFF")}");
        }
    }

    private void LateUpdate()
    {
        bool controlling = stop || slow || fastForward;

        if (fastForward)
            Time.timeScale = Plugin.ConfigFastForwardSpeed.Value;
        else if (stop)
            Time.timeScale = 0f;
        else if (slow)
            Time.timeScale = Plugin.ConfigSlowMotionScale.Value;
        else if (wasControlling)
            // MOD の制御から抜けた直後の 1 フレームだけ 1f に戻す。
            Time.timeScale = 1f;

        wasControlling = controlling;
    }

    private void OnGUI()
    {
        if (!stop && !slow)
            return;

        GUI.color = Color.cyan;
        int y = 10;
        if (stop)
        {
            GUI.Label(new Rect(10, y, 500, 24), $"Time Stop: ON ({Plugin.ConfigTimeStopKey.Value}=OFF)");
            y += 24;
        }
        if (slow)
        {
            GUI.Label(new Rect(10, y, 500, 24),
                $"Slow Motion: {Plugin.ConfigSlowMotionScale.Value:F2}x ({Plugin.ConfigSlowMotionKey.Value}=OFF)");
        }
        GUI.color = Color.white;
    }

    private static bool WasPressed(Keyboard kb, Key key)
        => key != Key.None && kb[key].wasPressedThisFrame;

    private static bool IsHeld(Keyboard kb, Key key)
        => key != Key.None && kb[key].isPressed;
}
