#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Generator asset placeholder untuk karakter Edelweiss bergaya visual novel bust (720x1280)
/// dengan palet warna aksen biru es / lavender pastel dan label teks kontras "EDELWEISS (PLACEHOLDER)".
/// </summary>
public static class EdelweissPlaceholderGenerator
{
    public const string EDELWEISS_DIR = "Assets/Character/Edelweiss";
    public const string EDELWEISS_PNG_PATH = "Assets/Character/Edelweiss/Edelweiss.png";
    public const string EDELWEISS_RESOURCES_DIR = "Assets/Resources/Character/Edelweiss";
    public const string EDELWEISS_RESOURCES_PNG_PATH = "Assets/Resources/Character/Edelweiss/Edelweiss.png";

    [MenuItem("Tokimeki TA/Generate Edelweiss Placeholder")]
    public static void GenerateEdelweissPlaceholderMenu()
    {
        Debug.Log("<color=cyan>[EdelweissGenerator]</color> Memulai pembuatan placeholder sprite Edelweiss...");
        GeneratePlaceholderSprite();
        EditorUtility.DisplayDialog("Edelweiss Placeholder Generator", 
            "Aset sprite karakter Edelweiss berhasil dibuat dan dikonfigurasi di:\n" + EDELWEISS_PNG_PATH, "OK");
    }

    public static void GeneratePlaceholderSprite()
    {
        if (!Directory.Exists(EDELWEISS_DIR)) Directory.CreateDirectory(EDELWEISS_DIR);
        if (!Directory.Exists(EDELWEISS_RESOURCES_DIR)) Directory.CreateDirectory(EDELWEISS_RESOURCES_DIR);

        int width = 720;
        int height = 1280;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // 1. Inisialisasi latar transparan penuh
        Color32[] pixels = new Color32[width * height];
        Color32 clearCol = new Color32(0, 0, 0, 0);
        for (int i = 0; i < pixels.Length; i++) pixels[i] = clearCol;
        tex.SetPixels32(pixels);

        // Palet warna Edelweiss
        Color32 hairBase = new Color32(185, 210, 240, 255);    // Biru es pastel
        Color32 hairShadow = new Color32(150, 180, 220, 255);  // Ice blue shadow
        Color32 hairHighlight = new Color32(225, 240, 255, 255);
        Color32 skinBase = new Color32(252, 240, 235, 255);    // Soft fair skin
        Color32 skinShadow = new Color32(240, 215, 205, 255);
        Color32 clothBase = new Color32(95, 110, 150, 255);     // Navy lavender uniform
        Color32 clothCollar = new Color32(215, 228, 245, 255);  // Ice blue collar
        Color32 ribbonColor = new Color32(205, 155, 185, 255);  // Lavender pink bow
        Color32 outlineColor = new Color32(55, 65, 95, 255);    // Deep outline

        // 2. Gambar Siluet Karakter (Bust & Twin-Tails)
        // A. Torso & Bahu (Uniform)
        for (int y = 0; y < 760; y++)
        {
            float progress = (760f - y) / 760f; // 0 di leher, 1 di bawah
            int halfW = (int)Mathf.Lerp(110f, 320f, Mathf.Sqrt(progress));
            int cx = 360;

            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                if (x >= 0 && x < width)
                {
                    Color col = clothBase;
                    // Kerah V-Neck
                    if (y > 600 && Mathf.Abs(x - cx) < (y - 600) * 0.7f)
                    {
                        col = skinBase;
                    }
                    else if (y > 580 && Mathf.Abs(x - cx) < (y - 580) * 0.9f)
                    {
                        col = clothCollar;
                    }
                    tex.SetPixel(x, y, col);
                }
            }
        }

        // B. Pita / Dasi Lavender di Dada
        DrawRibbon(tex, 360, 580, ribbonColor, outlineColor);

        // C. Leher
        for (int y = 720; y < 850; y++)
        {
            for (int x = 325; x <= 395; x++)
            {
                Color col = (y < 760) ? skinShadow : skinBase;
                tex.SetPixel(x, y, col);
            }
        }

        // D. Kuncir Dua Bergelombang (Twin-Tails di Kiri & Kanan)
        DrawTwinTail(tex, 200, 780, -1, hairBase, hairShadow, outlineColor);
        DrawTwinTail(tex, 520, 780, 1, hairBase, hairShadow, outlineColor);

        // E. Wajah & Dagu
        int headCx = 360;
        int headCy = 940;
        int headRx = 115;
        int headRy = 145;
        for (int y = headCy - headRy; y <= headCy + headRy; y++)
        {
            for (int x = headCx - headRx; x <= headCx + headRx; x++)
            {
                float dx = (x - headCx) / (float)headRx;
                float dy = (y - headCy) / (float)headRy;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    // Dagu meruncing halus
                    if (y < headCy - 60)
                    {
                        float taper = (y - (headCy - headRy)) / (float)(headRy - 60);
                        if (Mathf.Abs(x - headCx) > headRx * taper * 0.85f) continue;
                    }
                    tex.SetPixel(x, y, skinBase);
                }
            }
        }

        // F. Rambut Atas (Kubah Kepala & Poni Bertekstur)
        for (int y = headCy - 20; y <= headCy + headRy + 45; y++)
        {
            for (int x = headCx - headRx - 25; x <= headCx + headRx + 25; x++)
            {
                float dx = (x - headCx) / (float)(headRx + 25);
                float dy = (y - (headCy + 15)) / (float)(headRy + 30);
                if (dx * dx + dy * dy <= 1.0f)
                {
                    Color col = hairBase;
                    if (y > headCy + 90) col = hairHighlight;
                    else if (y < headCy + 40) col = hairShadow;
                    tex.SetPixel(x, y, col);
                }
            }
        }

        // Poni depan (Bangs)
        DrawBangs(tex, headCx, headCy + 30, hairBase, hairShadow, outlineColor);

        // G. Mata & Ekspresi Halus (Anime Eyes)
        DrawAnimeEye(tex, 310, 935, true);
        DrawAnimeEye(tex, 410, 935, false);
        DrawSoftMouth(tex, 360, 860);

        // 3. Aplikasikan Garis Tepi Outline Halus pada Batas Siluet
        ApplySmoothOutline(tex, outlineColor);

        // 4. Banner Badge Teks Kontras: "EDELWEISS (PLACEHOLDER)" di Bagian Dada
        DrawContrastBadge(tex, 360, 380, "EDELWEISS (PLACEHOLDER)");

        tex.Apply();

        // 5. Simpan berkas PNG
        byte[] pngBytes = tex.EncodeToPNG();
        Object.DestroyImmediate(tex);

        File.WriteAllBytes(EDELWEISS_PNG_PATH, pngBytes);
        File.WriteAllBytes(EDELWEISS_RESOURCES_PNG_PATH, pngBytes);
        Debug.Log($"<color=green>[EdelweissGenerator]</color> Berhasil menyimpan PNG ke:\n• {EDELWEISS_PNG_PATH}\n• {EDELWEISS_RESOURCES_PNG_PATH}");

        // 6. Konfigurasi Texture Importer via AssetDatabase
        AssetDatabase.Refresh();
        ConfigureTextureImporter(EDELWEISS_PNG_PATH);
        ConfigureTextureImporter(EDELWEISS_RESOURCES_PNG_PATH);
    }

    private static void ConfigureTextureImporter(string path)
    {
        TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;

        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.mipmapEnabled = false;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.alphaIsTransparency = true;
        ti.filterMode = FilterMode.Bilinear;

        EditorUtility.SetDirty(ti);
        ti.SaveAndReimport();
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
        Debug.Log($"<color=cyan>[EdelweissImporter]</color> Parameter Sprite (2D/UI, Mipmaps Off, Clamp) berhasil diterapkan: {path}");
    }

    // =========================================================================
    // PROCEDURAL DRAWING HELPERS
    // =========================================================================

    private static void DrawTwinTail(Texture2D tex, int rootX, int rootY, int dir, Color32 baseCol, Color32 shadowCol, Color32 outline)
    {
        // Pita ikat kuncir
        for (int py = rootY + 120; py <= rootY + 160; py++)
        {
            for (int px = rootX - 25; px <= rootX + 25; px++)
            {
                tex.SetPixel(px, py, new Color32(205, 155, 185, 255));
            }
        }

        // Aliran kuncir dua menjuntai
        for (int y = rootY - 380; y <= rootY + 140; y++)
        {
            float prog = (rootY + 140 - y) / 520f; // 0..1
            int cx = rootX + (int)(dir * Mathf.Sin(prog * 3.1415f) * 60f) + (int)(dir * prog * 40f);
            int halfW = (int)(Mathf.Sin(prog * 3.1415f) * 55f + 25f);

            for (int x = cx - halfW; x <= cx + halfW; x++)
            {
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                {
                    Color c = (dir * (x - cx) > 5) ? baseCol : shadowCol;
                    tex.SetPixel(x, y, c);
                }
            }
        }
    }

    private static void DrawBangs(Texture2D tex, int cx, int cy, Color32 baseCol, Color32 shadowCol, Color32 outline)
    {
        // Helai poni samping & tengah
        int[] bangOffsets = { -80, -45, -15, 20, 55, 85 };
        foreach (int off in bangOffsets)
        {
            int bx = cx + off;
            int length = 60 + Mathf.Abs(off) / 2;
            for (int dy = 0; dy < length; dy++)
            {
                int y = cy - dy;
                int w = Mathf.Max(2, 14 - (dy / 4));
                for (int x = bx - w; x <= bx + w; x++)
                {
                    if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                    {
                        tex.SetPixel(x, y, (dy > length - 10) ? shadowCol : baseCol);
                    }
                }
            }
        }
    }

    private static void DrawRibbon(Texture2D tex, int cx, int cy, Color32 ribCol, Color32 outCol)
    {
        // Simpul tengah
        for (int y = cy - 15; y <= cy + 15; y++)
        {
            for (int x = cx - 18; x <= cx + 18; x++)
            {
                tex.SetPixel(x, y, ribCol);
            }
        }
        // Sayap pita kiri & kanan
        for (int dx = 18; dx <= 60; dx++)
        {
            int h = (int)((60 - dx) * 0.7f);
            for (int dy = -h; dy <= h; dy++)
            {
                tex.SetPixel(cx - dx, cy + dy, ribCol);
                tex.SetPixel(cx + dx, cy + dy, ribCol);
            }
        }
    }

    private static void DrawAnimeEye(Texture2D tex, int cx, int cy, bool isLeft)
    {
        Color32 pupilCol = new Color32(50, 75, 120, 255);
        Color32 irisCol = new Color32(110, 155, 225, 255);
        Color32 whiteCol = new Color32(255, 255, 255, 255);

        for (int y = cy - 20; y <= cy + 20; y++)
        {
            for (int x = cx - 18; x <= cx + 18; x++)
            {
                float dx = (x - cx) / 18f;
                float dy = (y - cy) / 20f;
                if (dx * dx + dy * dy <= 1.0f)
                {
                    tex.SetPixel(x, y, (y > cy) ? pupilCol : irisCol);
                }
            }
        }
        // Kilau mata (Catchlight)
        for (int y = cy + 5; y <= cy + 12; y++)
        {
            for (int x = cx - 8; x <= cx - 2; x++)
            {
                tex.SetPixel(x, y, whiteCol);
            }
        }
        // Alis tipis
        for (int x = cx - 22; x <= cx + 22; x++)
        {
            tex.SetPixel(x, cy + 32, new Color32(95, 120, 160, 255));
            tex.SetPixel(x, cy + 33, new Color32(95, 120, 160, 255));
        }
    }

    private static void DrawSoftMouth(Texture2D tex, int cx, int cy)
    {
        Color32 lipCol = new Color32(220, 130, 130, 255);
        for (int x = cx - 14; x <= cx + 14; x++)
        {
            tex.SetPixel(x, cy, lipCol);
            tex.SetPixel(x, cy + 1, lipCol);
        }
    }

    private static void ApplySmoothOutline(Texture2D tex, Color32 outCol)
    {
        int w = tex.width;
        int h = tex.height;
        Color32[] src = tex.GetPixels32();
        Color32[] dst = (Color32[])src.Clone();

        for (int y = 2; y < h - 2; y++)
        {
            for (int x = 2; x < w - 2; x++)
            {
                int idx = y * w + x;
                if (src[idx].a == 0)
                {
                    // Cek tetangga
                    bool hasNeighbor = src[(y + 1) * w + x].a > 0 ||
                                       src[(y - 1) * w + x].a > 0 ||
                                       src[y * w + (x + 1)].a > 0 ||
                                       src[y * w + (x - 1)].a > 0;
                    if (hasNeighbor)
                    {
                        dst[idx] = outCol;
                    }
                }
            }
        }
        tex.SetPixels32(dst);
    }

    // =========================================================================
    // CONTRAST BADGE & BITMAP FONT RASTERIZER
    // =========================================================================

    private static void DrawContrastBadge(Texture2D tex, int cx, int cy, string text)
    {
        int badgeW = 600;
        int badgeH = 100;
        int minX = cx - badgeW / 2;
        int maxX = cx + badgeW / 2;
        int minY = cy - badgeH / 2;
        int maxY = cy + badgeH / 2;

        Color32 badgeBg = new Color32(18, 24, 38, 240);       // Dark glass
        Color32 badgeBorder = new Color32(110, 180, 245, 255); // Cyan ice glow
        Color32 textCol = new Color32(255, 255, 255, 255);
        Color32 textShadow = new Color32(0, 0, 0, 220);

        // Draw badge box with rounded border
        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                if (x >= 0 && x < tex.width && y >= 0 && y < tex.height)
                {
                    bool isBorder = (x <= minX + 3 || x >= maxX - 3 || y <= minY + 3 || y >= maxY - 3);
                    tex.SetPixel(x, y, isBorder ? badgeBorder : badgeBg);
                }
            }
        }

        // Render text
        int scale = 3;
        int charW = 6 * scale;
        int totalTextW = text.Length * charW;
        int startX = cx - totalTextW / 2;
        int startY = cy - (8 * scale) / 2;

        // Shadow first
        DrawString(tex, text, startX + 2, startY - 2, scale, textShadow);
        // Main text
        DrawString(tex, text, startX, startY, scale, textCol);
    }

    private static void DrawString(Texture2D tex, string str, int x, int y, int scale, Color32 color)
    {
        int curX = x;
        foreach (char c in str.ToUpper())
        {
            DrawChar(tex, c, curX, y, scale, color);
            curX += 6 * scale;
        }
    }

    private static void DrawChar(Texture2D tex, char c, int startX, int startY, int scale, Color32 color)
    {
        byte[] glyph = GetGlyph(c);
        for (int row = 0; row < 7; row++)
        {
            byte line = glyph[6 - row]; // Top to bottom
            for (int col = 0; col < 5; col++)
            {
                if ((line & (1 << (4 - col))) != 0)
                {
                    for (int dy = 0; dy < scale; dy++)
                    {
                        for (int dx = 0; dx < scale; dx++)
                        {
                            int px = startX + col * scale + dx;
                            int py = startY + row * scale + dy;
                            if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                            {
                                tex.SetPixel(px, py, color);
                            }
                        }
                    }
                }
            }
        }
    }

    private static byte[] GetGlyph(char c)
    {
        return c switch
        {
            'E' => new byte[] { 0x1F, 0x10, 0x10, 0x1E, 0x10, 0x10, 0x1F },
            'D' => new byte[] { 0x1E, 0x11, 0x11, 0x11, 0x11, 0x11, 0x1E },
            'L' => new byte[] { 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x1F },
            'W' => new byte[] { 0x11, 0x11, 0x11, 0x15, 0x15, 0x1B, 0x11 },
            'I' => new byte[] { 0x0E, 0x04, 0x04, 0x04, 0x04, 0x04, 0x0E },
            'S' => new byte[] { 0x0F, 0x10, 0x10, 0x0E, 0x01, 0x01, 0x1E },
            'P' => new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x10, 0x10, 0x10 },
            'A' => new byte[] { 0x0E, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 },
            'C' => new byte[] { 0x0E, 0x11, 0x10, 0x10, 0x10, 0x11, 0x0E },
            'H' => new byte[] { 0x11, 0x11, 0x11, 0x1F, 0x11, 0x11, 0x11 },
            'O' => new byte[] { 0x0E, 0x11, 0x11, 0x11, 0x11, 0x11, 0x0E },
            'R' => new byte[] { 0x1E, 0x11, 0x11, 0x1E, 0x14, 0x12, 0x11 },
            '(' => new byte[] { 0x06, 0x08, 0x10, 0x10, 0x10, 0x08, 0x06 },
            ')' => new byte[] { 0x0C, 0x02, 0x01, 0x01, 0x01, 0x02, 0x0C },
            '-' => new byte[] { 0x00, 0x00, 0x00, 0x1F, 0x00, 0x00, 0x00 },
            _ => new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }
        };
    }
}
#endif
