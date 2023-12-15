using DragonBonesMG.Core;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class DragonBonesTestScene : Scene
    {
        private readonly List<DbArmature> dragonBonesArmatures;

        public DragonBonesTestScene(int priority = 0) : base(inputType: InputTypes.None, priority: priority, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.dragonBonesArmatures = new List<DbArmature>();

            var atlasAndSkeletonNamesList = new List<List<string>>
            {
                new List<string> { "Demon.json", "DemonTexture.json" },
                new List<string> { "Sheep_ske.json", "Sheep_tex.json" },
                new List<string> { "Dragon_ske.json", "Dragon_tex.json" },
                new List<string> { "mecha_1004d_show_ske.json", "mecha_1004d_show_tex.json" },
                new List<string> { "Ubbie_ske.json", "Ubbie_tex.json" },
            };

            foreach (List<string> list in atlasAndSkeletonNamesList)
            {
                this.dragonBonesArmatures.Add(DragonBonesAnimManager.GetDragonBonesAnim(skeletonName: list[0], atlasName: list[1]));
            }
        }

        public override void Update()
        {
            foreach (DbArmature dragonBonesAnim in this.dragonBonesArmatures)
            {
                dragonBonesAnim.Update(SonOfRobinGame.CurrentGameTime.ElapsedGameTime);
            }
        }

        public override void Draw()
        {
            float scale = (float)SonOfRobinGame.ScreenHeight / 250f;

            for (int i = 0; i < this.dragonBonesArmatures.Count; i++)
            {
                DbArmature dbArmature = this.dragonBonesArmatures[i];

                dbArmature.Draw(s: SonOfRobinGame.SpriteBatch, position: new Vector2(100 * (i + 1), 130) * scale, rotation: 0f, scale: new Vector2(0.17f, 0.17f) * scale, color: Color.White);
            }
        }
    }
}