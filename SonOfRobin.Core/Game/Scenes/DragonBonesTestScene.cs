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
        private readonly List<DragonBonesAnim> dragonBonesAnims;
        public readonly DragonBonesAnim testPlayerAnim;

        public DragonBonesTestScene(int priority = 0) : base(inputType: InputTypes.None, priority: priority, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.dragonBonesAnims = new List<DragonBonesAnim>();

            var atlasNamesForSkeletonNames = new Dictionary<string, string>
            {
                //{ "Demon.json", "DemonTexture.json" },
                //{ "Sheep_ske.json", "Sheep_tex.json" },
                //{ "Dragon_ske.json", "Dragon_tex.json" },
                //{ "mecha_1004d_show_ske.json", "mecha_1004d_show_tex.json" },
                //{ "Ubbie_ske.json", "Ubbie_tex.json" },
            };

            foreach (var kvp in atlasNamesForSkeletonNames)
            {
                this.dragonBonesAnims.Add(new DragonBonesAnim(atlasName: kvp.Value, skeletonName: kvp.Key));
            }

            this.testPlayerAnim = new DragonBonesAnim(atlasName: "Ubbie_tex.json", skeletonName: "Ubbie_ske.json");
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

    public readonly struct DragonBonesAnim
    {
        private static readonly string contentDirPath = Path.Combine(SonOfRobinGame.ContentMgr.RootDirectory, "gfx", "_DragonBones");

        private readonly TextureAtlas textureAtlas;
        private readonly DbArmature dbArmature;
        private readonly string[] animNames;
        private readonly Queue<string> animsToPlayQueue;

        public DragonBonesAnim(string skeletonName, string atlasName)
        {
            string atlasPath = Path.Combine(contentDirPath, atlasName);
            string skeletonPath = Path.Combine(contentDirPath, skeletonName);

            string atlasJsonData = ReadFile(Path.Combine(contentDirPath, atlasName));
            string skeletonJsonData = ReadFile(Path.Combine(contentDirPath, skeletonName));

            this.textureAtlas = TextureAtlas.FromJsonData(atlasJsonData);

            this.textureAtlas.LoadContent(SonOfRobinGame.ContentMgr);
            this.dbArmature = DragonBones.FromJsonData(skeletonJsonData, this.textureAtlas, SonOfRobinGame.GfxDev).Armature;
            this.animNames = this.dbArmature.Animations.Select(a => a.Name).ToArray();
            this.dbArmature.GotoAndPlay(animation: this.animNames[0], loop: false);

            this.animsToPlayQueue = new Queue<string>();
        }

        private static string ReadFile(string path)
        {
            using (var stream = TitleContainer.OpenStream(path))
            {
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
        }

        public void Update()
        {
            this.dbArmature.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);

            if (this.dbArmature.IsDoneAnimating())
            {
                if (this.animsToPlayQueue.Count == 0)
                {
                    foreach (string animName in this.animNames)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            this.animsToPlayQueue.Enqueue(animName);
                        }
                    }
                }

                this.dbArmature.GotoAndPlay(animation: this.animsToPlayQueue.Dequeue(), loop: false);
            }
        }

        public void Draw(Vector2 position, Vector2 scale)
        {
            this.dbArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: position, rotation: 0f, scale: scale, color: Color.White);
        }
    }
}