using DragonBonesMG;
using DragonBonesMG.Display;

namespace SonOfRobin
{
    public class DragonBonesTest
    {
        public static void Draw()
        {

            var atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/TestDemon/Demon.json");
            var _armature = DragonBones.FromJson("Content/gfx/_DragonBones/TestSheep/DemonTexture.json", atlas, SonOfRobinGame.GfxDev).Armature;



        }
    }
}