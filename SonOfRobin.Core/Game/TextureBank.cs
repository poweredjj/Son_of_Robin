using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class TextureBank
    {
        private static ContentManager persistentTexturesManager;
        private static ContentManager temporaryTexturesManager;

        private static Dictionary<string, Texture2D> textureByNamePersistent = new Dictionary<string, Texture2D>();
        private static Dictionary<string, Texture2D> textureByNameTemporary = new Dictionary<string, Texture2D>();

        public static void AssignContentManagers(ContentManager persistentManager, ContentManager temporaryManager)
        {
            persistentTexturesManager = persistentManager;
            temporaryTexturesManager = temporaryManager;
        }

        public static Texture2D GetTexture(string textureName, bool persistent = true)
        {
            if (persistent) return GetTexturePersistent(textureName);
            else return GetTextureTemporary(textureName);
        }

        public static Texture2D GetTexturePersistent(string textureName)
        {
            if (!textureByNamePersistent.ContainsKey(textureName)) textureByNamePersistent[textureName] = persistentTexturesManager.Load<Texture2D>($"gfx/{textureName}");

            return textureByNamePersistent[textureName];
        }

        public static Texture2D GetTextureTemporary(string textureName)
        {
            if (!textureByNameTemporary.ContainsKey(textureName)) textureByNameTemporary[textureName] = temporaryTexturesManager.Load<Texture2D>($"gfx/{textureName}");

            return textureByNameTemporary[textureName];
        }

        public static void DisposeTexture(string textureName)
        {
            var textureDict = textureByNameTemporary.ContainsKey(textureName) ? textureByNameTemporary : textureByNamePersistent;

            textureDict[textureName].Dispose();
            textureDict.Remove(textureName);
        }

        public static void FlushTemporaryTextures()
        {
            temporaryTexturesManager.Unload();
        }
    }
}