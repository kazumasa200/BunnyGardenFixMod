using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace UITKit;

/// <summary>
/// PanelSettings / UIDocument のランタイム生成。MOD は asset を同梱できないため、
/// ゲームがロード済みの PanelSettings からテーマを借用するのが最も確実。
/// themeStyleSheet の解決に失敗した場合は PanelSettings の ThemeStyleSheet が null
/// のままになり、Unity が "UI will not render properly" 警告を出す。
/// 呼び出し側（View）は返却値の themeStyleSheet を確認して log する責務を持つ。
/// </summary>
public static class UITRuntime
{
    public static PanelSettings CreatePanelSettings(int sortingOrder = 999)
    {
        var settings = ScriptableObject.CreateInstance<PanelSettings>();
        settings.scaleMode = PanelScaleMode.ScaleWithScreenSize;
        settings.referenceResolution = new Vector2Int(1920, 1080);
        settings.match = 0.5f;
        // ScaleWithScreenSize の上に乗る追加倍率。Configs.UIScale はユーザー設定（既定 1.0）。
        settings.scale = global::BunnyGardenFixMod.Configs.UIScale.Value;
        settings.sortingOrder = sortingOrder;

        var existing = Resources.FindObjectsOfTypeAll<PanelSettings>()
            .FirstOrDefault(p => p != null && p != settings && p.themeStyleSheet != null);
        if (existing != null)
        {
            settings.themeStyleSheet = existing.themeStyleSheet;
            return settings;
        }

        var theme = Resources.FindObjectsOfTypeAll<ThemeStyleSheet>().FirstOrDefault();
        if (theme != null)
        {
            settings.themeStyleSheet = theme;
            return settings;
        }

        // uGUI 専用ゲーム（UI Toolkit 未使用）ではロード済み ThemeStyleSheet が存在しない。
        // 注意: ScriptableObject.CreateInstance<ThemeStyleSheet>() で「空テーマ」を作って割り当てると、
        // 内部のセレクタ参照テーブルが未初期化のため、スタイル適用パス
        // (VisualTreeStyleUpdater.ApplyStyles → StyleSelectorHelper.FastLookup) が毎フレーム
        // NullReferenceException で失敗し、インラインスタイルすら反映されなくなる（実測）。
        // UITKit は全スタイルをインラインで指定しているため、テーマは null のままにするのが正しい
        // （Unity が "No Theme Style Sheet" 警告を 1 度出すだけで描画は正常に行われる）。
        Debug.Log("[UITRuntime] ロード済みの ThemeStyleSheet が見つからないため無テーマで動作します（uGUI ゲーム想定）");

        return settings;
    }

    public static UIDocument AttachDocument(GameObject host, PanelSettings settings)
    {
        var doc = host.AddComponent<UIDocument>();
        doc.panelSettings = settings;
        doc.visualTreeAsset = null;
        return doc;
    }

    /// <summary>
    /// ゲームがロード済みの Font から日本語対応っぽいものを 1 つ選ぶ。
    /// 優先: Noto Sans JP / NotoSans / JP / Japanese / CJK を名前に含む Font。
    /// 見つからなければ LegacyRuntime.ttf (fallback — 実測では UI Toolkit の
    /// dynamic font fallback で日本語描画に成功している)。
    /// </summary>
    public static Font ResolveJapaneseFont(out IReadOnlyList<string> allFontNames)
    {
        var all = Resources.FindObjectsOfTypeAll<Font>();
        allFontNames = all.Select(f => f != null ? f.name : "<null>").ToList();

        string[] prefer = { "NotoSansJP", "NotoSans JP", "NotoSans-JP", "Noto Sans JP", "Japanese", "CJK", " JP", "-JP", "_JP" };
        foreach (var p in prefer)
        {
            var hit = all.FirstOrDefault(f => f != null && f.name.IndexOf(p, System.StringComparison.OrdinalIgnoreCase) >= 0);
            if (hit != null) return hit;
        }

        var loose = all.FirstOrDefault(f => f != null && f.name.IndexOf("Noto", System.StringComparison.OrdinalIgnoreCase) >= 0);
        if (loose != null) return loose;

        return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
    }

    /// <summary>
    /// Unity 2022.3 の UI Toolkit ランタイムでは、style.unityFont（レガシー Font）指定は
    /// unityFontDefinition より優先度が低く、動的 OS Font の内部変換も安定しない
    /// （日本語どころか全文字が豆腐になり得る）。TextCore の FontAsset を OS フォント
    /// ファミリー名から直接生成し、root の unityFontDefinition に設定するのが確実
    /// （FontDefinition は unityFont に優先し、子要素へ継承される）。
    /// </summary>
    public static bool TryGetJapaneseFontDefinition(out FontDefinition def)
    {
        string[] families = { "Yu Gothic UI", "Yu Gothic", "Meiryo", "MS Gothic", "Noto Sans CJK JP", "Noto Sans JP" };
        foreach (var family in families)
        {
            try
            {
                var fa = UnityEngine.TextCore.Text.FontAsset.CreateFontAsset(family, "Regular", 90);
                if (fa != null)
                {
                    Debug.Log($"[UITRuntime] OS フォントから FontAsset を生成: {family}");
                    def = FontDefinition.FromSDFFont(fa);
                    return true;
                }
            }
            catch { /* 次の候補へ */ }
        }
        def = default;
        return false;
    }

    /// <summary>
    /// 他の PanelSettings 名と sortingOrder 一覧を返す（sortingOrder 衝突調査用）。
    /// </summary>
    public static IReadOnlyList<string> DumpOtherPanelSettings(PanelSettings exclude)
    {
        return Resources.FindObjectsOfTypeAll<PanelSettings>()
            .Where(p => p != null && p != exclude)
            .Select(p => $"{p.name}(sort={p.sortingOrder})")
            .ToList();
    }
}
