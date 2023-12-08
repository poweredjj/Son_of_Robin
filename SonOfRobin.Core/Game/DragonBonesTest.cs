using DragonBonesMG;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;
using System.Linq;

namespace SonOfRobin
{
    public class DragonBonesTest
    {
        private static DragonBonesMG.Core.DbArmature demonArmature;
        private static string[] demonAnimNames;

        private static DragonBonesMG.Core.DbArmature sheepArmature;
        private static string[] sheepAnimNames;  
        
        private static DragonBonesMG.Core.DbArmature dragonArmature;
        private static string[] dragonAnimNames;


        public static void Draw()
        {
            if (demonArmature == null)
            {
                TextureAtlas atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/DemonTexture.json");
                atlas.LoadContent(SonOfRobinGame.ContentMgr);
                demonArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Demon.json", atlas, SonOfRobinGame.GfxDev).Armature;
                demonAnimNames = demonArmature.Animations.Select(a => a.Name).ToArray();
                demonArmature.GotoAndPlay(animation: demonAnimNames[0], loop: false);
            }

            if (sheepArmature == null)
            {
                TextureAtlas atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/Sheep_tex.json");
                atlas.LoadContent(SonOfRobinGame.ContentMgr);
                sheepArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Sheep_ske.json", atlas, SonOfRobinGame.GfxDev).Armature;
                sheepAnimNames = sheepArmature.Animations.Select(a => a.Name).ToArray();
                sheepArmature.GotoAndPlay(animation: sheepAnimNames[0], loop: false);
            }   
            
            if (dragonArmature == null)
            {
                TextureAtlas atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/Dragon_tex.json");
                atlas.LoadContent(SonOfRobinGame.ContentMgr);
                dragonArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Dragon_ske.json", atlas, SonOfRobinGame.GfxDev).Armature;
                dragonAnimNames = dragonArmature.Animations.Select(a => a.Name).ToArray();
                dragonArmature.GotoAndPlay(animation: dragonAnimNames[0], loop: false);
            }

            demonArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);
            sheepArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);
            dragonArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);

            if (demonArmature.IsDoneAnimating())
            {
                string animName = demonAnimNames[BoardPiece.Random.Next(demonAnimNames.Length)];
                demonArmature.GotoAndPlay(animation: animName, loop: false);
            }

            if (sheepArmature.IsDoneAnimating())
            {
                string animName = sheepAnimNames[BoardPiece.Random.Next(sheepAnimNames.Length)];
                sheepArmature.GotoAndPlay(animation: animName, loop: false);
            }  
            
            if (dragonArmature.IsDoneAnimating())
            {
                string animName = dragonAnimNames[BoardPiece.Random.Next(dragonAnimNames.Length)];
                dragonArmature.GotoAndPlay(animation: animName, loop: false);
            }

            demonArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(300f, 600f), rotation: 0f, scale: new Vector2(1.0f, 1.0f), color: Color.White);
            //sheepArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(700f, 550f), rotation: 0f, scale: new Vector2(0.8f, 0.8f), color: Color.White);
            dragonArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(1200f, 550f), rotation: 0f, scale: new Vector2(0.8f, 0.8f), color: Color.White);

        }
    }
}