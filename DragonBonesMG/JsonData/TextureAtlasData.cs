using System.IO;
using Microsoft.Xna.Framework.Content;
using Newtonsoft.Json;

namespace DragonBonesMG.JsonData {
    internal class TextureAtlasData {
        public string Name;
        public string ImagePath;

        [JsonProperty(PropertyName = "SubTexture")]
        public SubTextureData[] SubTextures;

        public static TextureAtlasData FromJson(string path)
        {           
            var data = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<TextureAtlasData>(data);
        }

        public static TextureAtlasData FromJsonData(string data)
        {
            return JsonConvert.DeserializeObject<TextureAtlasData>(data);
        }
    }
}