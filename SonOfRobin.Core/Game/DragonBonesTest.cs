using DragonBonesMG;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;
using System.Linq;

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
                TextureAtlas demonAtlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/DemonTexture.json");
                demonAtlas.LoadContent(SonOfRobinGame.ContentMgr);
                demonArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Demon.json", demonAtlas, SonOfRobinGame.GfxDev).Armature;
                demonArmature.GotoAndPlay(animation: "run", loop: false);
            }

            if (sheepArmature == null)
            {
                TextureAtlas sheepAtlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/Sheep_Ani_tex.json");
                sheepAtlas.LoadContent(SonOfRobinGame.ContentMgr);
                sheepArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Sheep_Ani_ske.json", sheepAtlas, SonOfRobinGame.GfxDev).Armature;
            }

            demonArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);

            if (demonArmature.IsDoneAnimating()) {
                string[] animationNames = demonArmature.Animations.Select(a => a.Name).ToArray();
                string animName = animationNames[BoardPiece.Random.Next(animationNames.Length)];
                demonArmature.GotoAndPlay(animation: animName, loop: false);
            }

            // sheepArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);

            sheepArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(600f, 800f), rotation: 0f, scale: new Vector2(1.0f, 1.0f), color: Color.White);
            demonArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(400f, 600f), rotation: 0f, scale: new Vector2(1.0f, 1.0f), color: Color.White);
        }
    }
}