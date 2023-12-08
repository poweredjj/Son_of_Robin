using DragonBonesMG;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;

namespace SonOfRobin
{
    public class DragonBonesTest
    {
        private static DragonBonesMG.Core.DbArmature demonArmature;
        private static DragonBonesMG.Core.DbArmature sheepArmature;

        public static void Draw()
        {
            if (demonArmature == null)
            {
                var demonAtlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/DemonTexture.json");
                demonAtlas.LoadContent(SonOfRobinGame.ContentMgr);
                demonArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Demon.json", demonAtlas, SonOfRobinGame.GfxDev).Armature;
                demonArmature.GotoAndPlay("run");

            }

            if (sheepArmature == null)
            {
                var sheepAtlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/Sheep_Ani_tex.json");
                sheepAtlas.LoadContent(SonOfRobinGame.ContentMgr);
                sheepArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Sheep_Ani_ske.json", sheepAtlas, SonOfRobinGame.GfxDev).Armature;
                // sheepArmature.GotoAndPlay("goat_walk_anim");
            }

            demonArmature.Update(SonOfRobinGame.CurrentGameTime.TotalGameTime);
            // sheepArmature.Update(SonOfRobinGame.CurrentGameTime.TotalGameTime);

            //sheepArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(200f, 250f), rotation: 0f, scale: new Vector2(0.5f, 0.3f), color: Color.White);
            demonArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(300f, 220f), rotation: 0f, scale: new Vector2(0.3f, 0.3f), color: Color.White);
        }
    }
}