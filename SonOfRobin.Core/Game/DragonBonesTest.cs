using DragonBonesMG.Display;
using Microsoft.Xna.Framework.Graphics;

namespace SonOfRobin
{
    public class DragonBonesTest
    {
        public static void Draw()
        {
            // "Sheep_Ani_ske.json"
            // "Sheep_Ani_tex.json"
            // "Sheep_Ani_tex.png"


            Texture2D atlasTexture = SonOfRobinGame.ContentMgr.Load<Texture2D>("gfx/_DragonBones/TestSheep/Sheep_Ani_tex");

            var atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/TestSheep/Sheep_Ani_tex.json");
            var _armature = DragonBones.FromJson("Content/gfx/_DragonBones/TestSheep/Sheep_Ani_ske.json", atlas, SonOfRobinGame.GfxDev).Armature;


            //byte[] dbDataBin = FileReaderWriter.LoadBytes("Content/gfx/_DragonBones/TestSheep/Sheep_Ani_ske.dbbin");
            //byte[] texJson = FileReaderWriter.LoadBytes("Content/gfx/_DragonBones/TestSheep/Sheep_Ani_tex.json");




        }
    }
}