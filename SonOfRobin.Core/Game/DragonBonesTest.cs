using DragonBonesMG;
using DragonBonesMG.Display;

namespace SonOfRobin
{
    public class DragonBonesTest
    {
        public static void Draw()
        {

            var _demonAtlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/DemonTexture.json");
            _demonAtlas.LoadContent(SonOfRobinGame.ContentMgr);
            var _demonArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Demon.json", _demonAtlas, SonOfRobinGame.GfxDev).Armature;




        }
    }
}