/********************************************************************************
** auth:  https://github.com/HushengStudent
** date:  2022/02/05 20:01:29
** desc:  Texture
*********************************************************************************/

using UnityEditor;
using UnityEngine;

public class TextureImporterHelper : AssetPostprocessor
{
    void OnPostprocessTexture(Texture2D texture)
    {
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.textureCompression = TextureImporterCompression.Compressed;

        importer.sRGBTexture = true;
        importer.alphaSource = TextureImporterAlphaSource.FromInput;
        importer.alphaIsTransparency = true;

        importer.isReadable = false;
        importer.mipmapEnabled = false;

        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
        {
            name = "Android",
            overridden = true,
            maxTextureSize = 2048,
            format = TextureImporterFormat.ASTC_6x6,
        });

        importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings()
        {
            name = "iPhone",
            overridden = true,
            maxTextureSize = 2048,
            format = TextureImporterFormat.ASTC_6x6,
        });

        importer.SaveAndReimport();

        //AssetDatabase.ImportAsset(assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
