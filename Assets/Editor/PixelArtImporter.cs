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
        if (name.StartsWith("player"))      importer.spritePixelsPerUnit = 24;
        else if (name.StartsWith("sack"))   importer.spritePixelsPerUnit = 18;  // tool bag
        else if (name.StartsWith("toilet")) importer.spritePixelsPerUnit = 9;   // toilet map is drawn smaller
        else if (name.StartsWith("duck"))   importer.spritePixelsPerUnit = 9;   // rubber duck is 2 units wide
        else                               importer.spritePixelsPerUnit = 36;

        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        settings.spriteMeshType = SpriteMeshType.FullRect;  // needed for tiled sprites
        // island edges stay the same and the middle repeats
        if (name.StartsWith("island")) settings.spriteBorder = new Vector4(18, 0, 18, 0);
        if (name.StartsWith("button")) settings.spriteBorder = new Vector4(4, 4, 4, 4);  // menu button corners
        importer.SetTextureSettings(settings);
    }
}
