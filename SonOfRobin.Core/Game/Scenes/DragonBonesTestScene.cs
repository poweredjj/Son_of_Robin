using DragonBonesMG;
using DragonBonesMG.Core;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SonOfRobin
{
    public class DragonBonesTestScene : Scene
    {
        public readonly struct DragonBonesAnim
        {
            private readonly TextureAtlas textureAtlas;
            private readonly DbArmature dbArmature;
            private readonly string[] animNames;

            public DragonBonesAnim(string atlasPath, string skeletonPath)
            {
                this.textureAtlas = TextureAtlas.FromJson(atlasPath);

                this.textureAtlas.LoadContent(SonOfRobinGame.ContentMgr);
                this.dbArmature = DragonBones.FromJson(skeletonPath, this.textureAtlas, SonOfRobinGame.GfxDev).Armature;
                this.animNames = this.dbArmature.Animations.Select(a => a.Name).ToArray();
                this.dbArmature.GotoAndPlay(animation: this.animNames[0], loop: false);
            }

            public void Update()
            {
                this.dbArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);

                if (this.dbArmature.IsDoneAnimating())
                {
                    string animName = this.animNames[SonOfRobinGame.random.Next(this.animNames.Length)];
                    this.dbArmature.GotoAndPlay(animation: animName, loop: false);
                }
            }

            public void Draw(Vector2 position, Vector2 scale)
            {
                this.dbArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: position, rotation: 0f, scale: scale, color: Color.White);
            }
        }

        private readonly DragonBonesAnim[] dragonBonesAnims;

        public DragonBonesTestScene(int priority = 0) : base(inputType: InputTypes.None, priority: priority, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            List<DragonBonesAnim> dragonBonesAnimsList = new();
            dragonBonesAnimsList.Add(new DragonBonesAnim(atlasPath: "Content/gfx/_DragonBones/DemonTexture.json", skeletonPath: "Content/gfx/_DragonBones/Demon.json"));
            dragonBonesAnimsList.Add(new DragonBonesAnim(atlasPath: "Content/gfx/_DragonBones/Sheep_tex.json", skeletonPath: "Content/gfx/_DragonBones/Sheep_ske.json"));
            dragonBonesAnimsList.Add(new DragonBonesAnim(atlasPath: "Content/gfx/_DragonBones/Dragon_tex.json", skeletonPath: "Content/gfx/_DragonBones/Dragon_ske.json"));
            dragonBonesAnimsList.Add(new DragonBonesAnim(atlasPath: "Content/gfx/_DragonBones/mecha_1004d_show_tex.json", skeletonPath: "Content/gfx/_DragonBones/mecha_1004d_show_ske.json"));
            //dragonBonesAnimsList.Add(new DragonBonesAnim(atlasPath: "Content/gfx/_DragonBones/Ubbie_tex.json", skeletonPath: "Content/gfx/_DragonBones/Ubbie_ske.json"));

            this.dragonBonesAnims = dragonBonesAnimsList.ToArray();
        }

        public override void Update()
        {
            foreach (DragonBonesAnim dragonBonesAnim in this.dragonBonesAnims)
            {
                dragonBonesAnim.Update();
            }
        }

        public override void Draw()
        {
            float scale = (float)SonOfRobinGame.VirtualHeight / 250f;

            int animNo = 0;
            foreach (DragonBonesAnim dragonBonesAnim in this.dragonBonesAnims)
            {
                dragonBonesAnim.Draw(position: new Vector2(125 * (animNo + 1), 150) * scale, scale: new Vector2(0.2f, 0.2f) * scale);
                animNo++;
            }
        }
    }
}