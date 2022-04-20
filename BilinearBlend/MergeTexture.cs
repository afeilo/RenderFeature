using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MergeTexturee : MonoBehaviour
{
    [UnityEditor.MenuItem("Create/MergeTexture")]
    static void ExportHeight()
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Clipmap/split.mat");
        var t1 = mat.GetTexture("_Splat1") as Texture2D;
        var t2 = mat.GetTexture("_Splat2") as Texture2D;
        var t3 = mat.GetTexture("_Splat3") as Texture2D;
        var ai1 = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t1)) as TextureImporter;
        var ai2 = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t2)) as TextureImporter;
        var ai3 = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(t3)) as TextureImporter;
        ai1.isReadable = true;
        ai2.isReadable = true;
        ai3.isReadable = true;
        ai1.SaveAndReimport();
        ai2.SaveAndReimport();
        ai3.SaveAndReimport();
        //
        // var width = t2.width;
        // var height = t2.height;
        //
        // Texture2D texture2D = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Debug.Log(width + "-" + height);
        // var c2 = t2.GetPixels32();
        // for (int i = 0; i < width; i++)
        // {
        //     for (int j = 0; j < height; j++)
        //     {
        //         texture2D.SetPixel(i, j, GetColor(i, j, c2, width, height));
        //     }
        // }
        //
        // var bytes = texture2D.EncodeToPNG();
        // FileStream file = null;
        //
        // file = File.Open("Assets/Clipmap/splitmix22222.png", FileMode.Create);
        //
        // var binary = new BinaryWriter(file);
        // binary.Write(bytes);
        // file.Close();

        // Texture2D texture2D = new Texture2D(t1.width * 2, t1.height * 2, TextureFormat.RGB24, t1.mipmapCount, true);
        
        var temp_c1 = t1.GetPixels32();
        var temp_c2 = t2.GetPixels32();
        var temp_c3 = t3.GetPixels32();
        
        for (int lod = 0; lod < t1.mipmapCount; lod++)
        {
            var width = t1.width >> lod;
            var height = t1.height >> lod;
            Texture2D texture2D = new Texture2D(width * 2, height * 2, TextureFormat.RGB24, false);
            Debug.Log(width + "-" + height);
            var c1 = temp_c1;
            var c2 = temp_c2;
            var c3 = temp_c3;
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    texture2D.SetPixel(i * 2, j * 2, GetColor(i, j, c1, width, height));
                    texture2D.SetPixel(i * 2 + 1, j * 2, GetColor(i, j, c2, width, height));
                    texture2D.SetPixel(i * 2, j * 2 + 1, GetColor(i, j, c3, width, height));
                    texture2D.SetPixel(i * 2 + 1, j * 2 + 1, GetColor(i, j, c3, width, height));
                }
            }
        
            var bytes = texture2D.EncodeToPNG();
            FileStream file = null;
            if (lod > 0)
            {
                file = File.Open("Assets/Clipmap/splitmix.mip" + lod + ".png", FileMode.Create);
            }
            else
            {
                file = File.Open("Assets/Clipmap/splitmix.png", FileMode.Create);
            }
        
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();
        
            if (lod != t1.mipmapCount - 1)
            {
                var _width = width / 2;
                var _height = height / 2;
                temp_c1 = new Color32[(_width) * (_height)];
                temp_c2 = new Color32[(_width) * (_height)];
                temp_c3 = new Color32[(_width) * (_height)];
                for (int i = 0; i < _width; i++)
                {
                    for (int j = 0; j < _height; j++)
                    {
                        var _c1 = Bilinear(i * 2, j * 2, c1, width, height);
                        SetColor(i, j, temp_c1, _c1, _width, _height);
                        var _c2 = Bilinear(i * 2, j * 2, c2, width, height);
                        SetColor(i, j, temp_c2, _c2, _width, _height);
                        var _c3 = Bilinear(i * 2, j * 2, c3, width, height);
                        SetColor(i, j, temp_c3, _c3, _width, _height);
                    }
                }
            }
        }
        
        
        ai1.isReadable = false;
        ai2.isReadable = false;
        ai3.isReadable = false;
        ai1.SaveAndReimport();
        ai2.SaveAndReimport();
        ai3.SaveAndReimport();

        UnityEditor.AssetDatabase.Refresh();
    }

    static Color32 GetColor(int i, int j, Color32[] colors, int width, int height)
    {
        return colors[i + j * height];
    }
    
    static void SetColor(int i, int j, Color32[] colors, Color32 c, int width, int height)
    {
        colors[i + j* height] = c;
    }

    static Color32 Bilinear(int i, int j, Color32[] colors, int width, int height)
    {
        var _c1 = GetColor(i, j, colors, width, height);
        var _c2 = GetColor(i, j + 1, colors, width, height);
        var _c3 = GetColor(i + 1, j + 1, colors, width, height);
        var _c4 = GetColor(i + 1, j, colors, width, height);
        var _r = (byte)(((double) _c1.r + (double) _c2.r + (double) _c3.r + (double) _c4.r) / 4.0f);
        var _g = (byte)(((double) _c1.g + (double) _c2.g + (double) _c3.g + (double) _c4.g) / 4.0f);
        var _b = (byte)(((double) _c1.b + (double) _c2.b + (double) _c3.b + (double) _c4.b) / 4.0f);
        var _a = (byte)(((double) _c1.a + (double) _c2.a + (double) _c3.a + (double) _c4.a) / 4.0f);
        return new Color32(_r, _g, _b, _a);
    }
}