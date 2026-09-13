#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using TMPro;

public static class TMPFallbackFontSetup
{
    [MenuItem("Game Debug/Setup TMP Fallback Fonts (Chinese, Japanese & Emoji)")]
    public static void SetupFonts()
    {
        Debug.Log("<color=cyan>=== MEMULAI KONFIGURASI TMP FALLBACK FONTS ===</color>");

        TMP_FontAsset cnFont = EnsureFontAsset("Assets/Fonts/MicrosoftYaHei.ttf", "Assets/Fonts/MicrosoftYaHei SDF.asset", "MicrosoftYaHei SDF");
        TMP_FontAsset jpFont = EnsureFontAsset("Assets/Fonts/MSGothic.ttf", "Assets/Fonts/MSGothic SDF.asset", "MSGothic SDF");
        TMP_FontAsset emojiFont = EnsureFontAsset("Assets/Fonts/SegoeUIEmoji.ttf", "Assets/Fonts/SegoeUIEmoji SDF.asset", "SegoeUIEmoji SDF");

        // 1. Hubungkan ke LiberationSans SDF fallback table
        TMP_FontAsset libSans = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (libSans != null)
        {
            if (libSans.fallbackFontAssetTable == null)
                libSans.fallbackFontAssetTable = new List<TMP_FontAsset>();

            if (cnFont != null && !libSans.fallbackFontAssetTable.Contains(cnFont))
            {
                libSans.fallbackFontAssetTable.Add(cnFont);
                EditorUtility.SetDirty(libSans);
                Debug.Log("<color=green>[TMP Fallback]</color> MicrosoftYaHei SDF ditambahkan ke LiberationSans SDF fallback table.");
            }

            if (jpFont != null && !libSans.fallbackFontAssetTable.Contains(jpFont))
            {
                libSans.fallbackFontAssetTable.Add(jpFont);
                EditorUtility.SetDirty(libSans);
                Debug.Log("<color=green>[TMP Fallback]</color> MSGothic SDF ditambahkan ke LiberationSans SDF fallback table.");
            }

            if (emojiFont != null && !libSans.fallbackFontAssetTable.Contains(emojiFont))
            {
                libSans.fallbackFontAssetTable.Add(emojiFont);
                EditorUtility.SetDirty(libSans);
                Debug.Log("<color=green>[TMP Fallback]</color> SegoeUIEmoji SDF ditambahkan ke LiberationSans SDF fallback table.");
            }
        }

        // 2. Hubungkan ke TMP_Settings global fallback list
        if (TMP_Settings.fallbackFontAssets != null)
        {
            if (cnFont != null && !TMP_Settings.fallbackFontAssets.Contains(cnFont))
            {
                TMP_Settings.fallbackFontAssets.Add(cnFont);
                EditorUtility.SetDirty(TMP_Settings.instance);
                Debug.Log("<color=green>[TMP Fallback]</color> MicrosoftYaHei SDF ditambahkan ke global TMP_Settings fallbackFontAssets.");
            }
            if (jpFont != null && !TMP_Settings.fallbackFontAssets.Contains(jpFont))
            {
                TMP_Settings.fallbackFontAssets.Add(jpFont);
                EditorUtility.SetDirty(TMP_Settings.instance);
                Debug.Log("<color=green>[TMP Fallback]</color> MSGothic SDF ditambahkan ke global TMP_Settings fallbackFontAssets.");
            }
            if (emojiFont != null && !TMP_Settings.fallbackFontAssets.Contains(emojiFont))
            {
                TMP_Settings.fallbackFontAssets.Add(emojiFont);
                EditorUtility.SetDirty(TMP_Settings.instance);
                Debug.Log("<color=green>[TMP Fallback]</color> SegoeUIEmoji SDF ditambahkan ke global TMP_Settings fallbackFontAssets.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("<color=green>=== KONFIGURASI TMP FALLBACK FONTS SELESAI ===</color>");
    }

    public static TMP_FontAsset EnsureFontAsset(string fontPath, string assetPath, string assetName)
    {
        TMP_FontAsset fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
        if (fontAsset != null) return fontAsset;

        Font font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        if (font == null)
        {
            AssetDatabase.ImportAsset(fontPath, ImportAssetOptions.ForceUpdate);
            font = AssetDatabase.LoadAssetAtPath<Font>(fontPath);
        }

        if (font == null)
        {
            Debug.LogWarning($"[TMP Fallback] Font tidak ditemukan di: {fontPath}");
            return null;
        }

        fontAsset = TMP_FontAsset.CreateFontAsset(font);
        if (fontAsset == null)
        {
            Debug.LogWarning($"[TMP Fallback] CreateFontAsset gagal untuk: {font.name}");
            return null;
        }

        fontAsset.name = assetName;
        AssetDatabase.CreateAsset(fontAsset, assetPath);

        if (fontAsset.material != null)
        {
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        }

        if (fontAsset.atlasTextures != null && fontAsset.atlasTextures.Length > 0 && fontAsset.atlasTextures[0] != null)
        {
            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[0], fontAsset);
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();

        Debug.Log($"<color=green>[TMP Fallback]</color> Berhasil membuat dynamic SDF font asset: {assetPath}");
        return fontAsset;
    }
}
#endif
