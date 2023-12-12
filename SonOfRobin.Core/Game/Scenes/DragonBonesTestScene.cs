using DragonBonesMG;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;
using System.Linq;

namespace SonOfRobin
{
    public class DragonBonesTestScene : Scene
    {
        private readonly DragonBonesMG.Core.DbArmature demonArmature;
        private readonly string[] demonAnimNames;

        private readonly DragonBonesMG.Core.DbArmature sheepArmature;
        private readonly string[] sheepAnimNames;

        private readonly DragonBonesMG.Core.DbArmature dragonArmature;
        private readonly string[] dragonAnimNames;

        private readonly DragonBonesMG.Core.DbArmature mechaArmature;
        private readonly string[] mechaAnimNames;

        public DragonBonesTestScene(int priority = 0) : base(inputType: InputTypes.None, priority: priority, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            {
                TextureAtlas atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/DemonTexture.json");
                atlas.LoadContent(SonOfRobinGame.ContentMgr);
                this.demonArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Demon.json", atlas, SonOfRobinGame.GfxDev).Armature;
                this.demonAnimNames = this.demonArmature.Animations.Select(a => a.Name).ToArray();
                this.demonArmature.GotoAndPlay(animation: this.demonAnimNames[0], loop: false);
            }
            {
                TextureAtlas atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/Sheep_tex.json");
                atlas.LoadContent(SonOfRobinGame.ContentMgr);
                this.sheepArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Sheep_ske.json", atlas, SonOfRobinGame.GfxDev).Armature;
                this.sheepAnimNames = this.sheepArmature.Animations.Select(a => a.Name).ToArray();
                this.sheepArmature.GotoAndPlay(animation: this.sheepAnimNames[0], loop: false);
            }
            {
                TextureAtlas atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/Dragon_tex.json");
                atlas.LoadContent(SonOfRobinGame.ContentMgr);
                this.dragonArmature = DragonBones.FromJson("Content/gfx/_DragonBones/Dragon_ske.json", atlas, SonOfRobinGame.GfxDev).Armature;
                this.dragonAnimNames = this.dragonArmature.Animations.Select(a => a.Name).ToArray();
                this.dragonArmature.GotoAndPlay(animation: this.dragonAnimNames[0], loop: false);
            }
            {
                TextureAtlas atlas = TextureAtlas.FromJson("Content/gfx/_DragonBones/mecha_2903_tex.json");
                atlas.LoadContent(SonOfRobinGame.ContentMgr);
                this.mechaArmature = DragonBones.FromJson("Content/gfx/_DragonBones/mecha_2903_ske.json", atlas, SonOfRobinGame.GfxDev).Armature;
                this.mechaAnimNames = this.mechaArmature.Animations.Select(a => a.Name).ToArray();
                this.mechaArmature.GotoAndPlay(animation: this.mechaAnimNames[0], loop: false);
            }
        }

        public override void Update()
        {
            this.demonArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);
            this.sheepArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);
            this.dragonArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);
            this.mechaArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);

            if (demonArmature.IsDoneAnimating())
            {
                string animName = this.demonAnimNames[SonOfRobinGame.random.Next(demonAnimNames.Length)];
                demonArmature.GotoAndPlay(animation: animName, loop: false);
            }

            if (sheepArmature.IsDoneAnimating())
            {
                string animName = this.sheepAnimNames[SonOfRobinGame.random.Next(sheepAnimNames.Length)];
                sheepArmature.GotoAndPlay(animation: animName, loop: false);
            }

            if (dragonArmature.IsDoneAnimating())
            {
                string animName = this.dragonAnimNames[SonOfRobinGame.random.Next(dragonAnimNames.Length)];
                dragonArmature.GotoAndPlay(animation: animName, loop: false);
            }
        }

        public override void Draw()
        {
            float scale = (float)SonOfRobinGame.VirtualHeight / 250f;

            this.demonArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(120f, 220f) * scale, rotation: 0f, scale: new Vector2(-0.4f, 0.4f) * scale, color: Color.White);
            this.sheepArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(300f, 150f) * scale, rotation: 0f, scale: new Vector2(0.2f, 0.2f) * scale, color: Color.White);
            //this.dragonArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(450f, 150f) * scale, rotation: 0f, scale: new Vector2(0.2f, 0.2f) * scale, color: Color.White);
            this.mechaArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(480f, 120f) * scale, rotation: 0f, scale: new Vector2(0.4f, 0.4f) * scale, color: Color.White);
        }
    }
}