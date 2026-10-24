using UnityEditor;
using UnityEngine;

// Import settings for everything in Assets/Art so the pixel art stays sharp
public class PixelArtImporter : AssetPostprocessor {
    void OnPreprocessTexture () {
        if (!assetPath.StartsWith("Assets/Art/")) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType         = TextureImporterType.Sprite;
        importer.spriteImportMode    = SpriteImportMode.Single;
        importer.filterMode          = FilterMode.Point;
        importer.textureCompression  = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled       = false;
        importer.alphaIsTransparency = true;
        importer.wrapMode            = TextureWrapMode.Clamp;

        // pixels per unit: tiles are 18px = half a unit, characters are 24px = 1 unit
        string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        if (name.StartsWith("player"))     importer.spritePixelsPerUnit = 24;
        else if (name == "sack")           importer.spritePixelsPerUnit = 18;
        else if (name.StartsWith("cloud")) importer.spritePixelsPerUnit = 18;
        else if (name == "horizon")        importer.spritePixelsPerUnit = 12;
        else if (name.StartsWith("tree"))  importer.spritePixelsPerUnit = 18;  // trees are 1 unit wide
        else                               importer.spritePixelsPerUnit = 36;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;  // needed for tiled sprites
        // island edges stay the same and the middle repeats
        if (name.StartsWith("island")) settings.spriteBorder = new Vector4(18, 0, 18, 0);
        importer.SetTextureSettings(settings);
    }
}
