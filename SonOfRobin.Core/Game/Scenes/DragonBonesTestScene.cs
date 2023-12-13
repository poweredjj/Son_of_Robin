using DragonBonesMG;
using DragonBonesMG.Core;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SonOfRobin
{
    public class DragonBonesTestScene : Scene
    {
        private static readonly string contentDirPath = Path.Combine(SonOfRobinGame.ContentMgr.RootDirectory, "gfx", "_DragonBones");

        public readonly struct DragonBonesAnim
        {
            private readonly TextureAtlas textureAtlas;
            private readonly DbArmature dbArmature;
            private readonly string[] animNames;

            public DragonBonesAnim(string atlasName, string skeletonName)
            {
                string atlasPath = Path.Combine(contentDirPath, atlasName);
                string skeletonPath = Path.Combine(contentDirPath, skeletonName);

                string atlasJsonData, skeletonJsonData;

                using (var stream = TitleContainer.OpenStream(atlasPath))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        atlasJsonData = reader.ReadToEnd();
                    }
                }

                using (var stream = TitleContainer.OpenStream(skeletonPath))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        skeletonJsonData = reader.ReadToEnd();
                    }
                }

                this.textureAtlas = TextureAtlas.FromJsonData(atlasJsonData);

                this.textureAtlas.LoadContent(SonOfRobinGame.ContentMgr);
                this.dbArmature = DragonBones.FromJsonData(skeletonJsonData, this.textureAtlas, SonOfRobinGame.GfxDev).Armature;
                //this.dbArmature = DragonBones.FromJson(skeletonPath, this.textureAtlas, SonOfRobinGame.GfxDev).Armature;
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

        private readonly List<DragonBonesAnim> dragonBonesAnims;

        public DragonBonesTestScene(int priority = 0) : base(inputType: InputTypes.None, priority: priority, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.dragonBonesAnims = new List<DragonBonesAnim>();

            this.dragonBonesAnims.Add(new DragonBonesAnim(atlasName: "DemonTexture.json", skeletonName: "Demon.json"));
            this.dragonBonesAnims.Add(new DragonBonesAnim(atlasName: "Sheep_tex.json", skeletonName: "Sheep_ske.json"));
            this.dragonBonesAnims.Add(new DragonBonesAnim(atlasName: "Dragon_tex.json", skeletonName: "Dragon_ske.json"));
            this.dragonBonesAnims.Add(new DragonBonesAnim(atlasName: "mecha_1004d_show_tex.json", skeletonName: "mecha_1004d_show_ske.json"));
            this.dragonBonesAnims.Add(new DragonBonesAnim(atlasName: "Ubbie_tex.json", skeletonName: "Ubbie_ske.json"));
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

            for (int i = 0; i < this.dragonBonesAnims.Count; i++)
            {
                DragonBonesAnim dragonBonesAnim = this.dragonBonesAnims[i];
                dragonBonesAnim.Draw(position: new Vector2(100 * (i + 1), 130) * scale, scale: new Vector2(0.17f, 0.17f) * scale);
            }
        }
    }
}