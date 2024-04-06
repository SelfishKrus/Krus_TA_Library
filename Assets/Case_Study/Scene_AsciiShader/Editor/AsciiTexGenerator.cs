using System.Collections;
using System.Collections.Generic;
using SD = System.Drawing;
using System.Drawing.Imaging;
using UnityEditor;
using UnityEngine;
using System.Drawing;

public class AsciiTexGenerator : EditorWindow
{
    string characters = "A";
    float fontScale = 0.5f;
    int tileSize = 32;
    string fontType = "Courier";

    string texSavePath = "Assets/Case_Study/Scene_AsciiShader/Texture";
    string texName = "Tex_AsciiTex.png";

    [MenuItem("Tools/Ascii Texture Generator")]
    public static void ShowWindow()
    {
        GetWindow<AsciiTexGenerator>("Ascii Texture Generator");
    }

    private void OnGUI()
    {
        characters = EditorGUILayout.TextField("Characters", characters);
        fontScale = EditorGUILayout.FloatField("Font Scale", fontScale);
        fontType = EditorGUILayout.TextField("Font Type", fontType);
        tileSize = EditorGUILayout.IntField("Tile Size", tileSize);

        EditorGUILayout.Space(10);

        texName = EditorGUILayout.TextField("Texture Name", texName);
        texSavePath = EditorGUILayout.TextField("Texture Save Path", texSavePath);

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Generate Texture"))
        {
            Texture2D texture = GenerateTexture(characters, tileSize);
            string filePath = texSavePath + "/" + texName;
            // Save the texture to a file, replace "path_to_save" with your desired path
            System.IO.File.WriteAllBytes(filePath, texture.EncodeToPNG());
        }
    }

    Texture2D GenerateTexture(string characters, int tileSize)
    {
        Texture2D texture = new Texture2D(tileSize * characters.Length, tileSize);
        SD.Bitmap bitmap = new SD.Bitmap(tileSize * characters.Length, tileSize);

        // Draw characters to the bitmap
        using (SD.Graphics g = SD.Graphics.FromImage(bitmap))
        {
            g.Clear(SD.Color.Transparent);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            SD.Font font = new SD.Font(fontType, tileSize * fontScale);
            SD.Brush brush = new SD.SolidBrush(SD.Color.Black);

            for (int i = 0; i < characters.Length; i++)
            {
                g.DrawString(characters[i].ToString(), font, brush, i * tileSize, 0);
            }
        }

        // Blit the bitmap to the texture
        for (int y = 0; y < bitmap.Height; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                System.Drawing.Color color = bitmap.GetPixel(x, y);
                texture.SetPixel(x, bitmap.Height - 1 - y, new Color32(color.R, color.G, color.B, color.A));
            }
        }
        texture.Apply();
        Debug.Log("Save " + texName + " to " + texSavePath);

        return texture;
    }
}
