using DragonBonesMG;
using DragonBonesMG.Core;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace SonOfRobin
{
    public class DragonBonesTestScene : Scene
    {
        private readonly List<DbArmature> dragonBonesArmatures;
        public readonly Dictionary<Sprite, DbArmature> animsForSpritesDict;

        public DragonBonesTestScene(int priority = 0) : base(inputType: InputTypes.None, priority: priority, blocksUpdatesBelow: false, alwaysUpdates: false, alwaysDraws: false, touchLayout: TouchLayout.Empty, tipsLayout: ControlTips.TipsLayout.Empty)
        {
            this.dragonBonesArmatures = new List<DbArmature>();

            var atlasAndSkeletonNamesList = new List<List<string>>
            {
                new List<string> { "Demon.json", "DemonTexture.json" },
                //new List<string> { "Sheep_ske.json", "Sheep_tex.json" },
                //new List<string> { "Dragon_ske.json", "Dragon_tex.json" },
                //new List<string> { "mecha_1004d_show_ske.json", "mecha_1004d_show_tex.json" },
                //new List<string> { "Ubbie_ske.json", "Ubbie_tex.json" },
            };

            foreach (List<string> list in atlasAndSkeletonNamesList)
            {
                this.dragonBonesArmatures.Add(DragonBonesAnimManager.GetDragonBonesAnim(skeletonName: list[0], atlasName: list[1]));
            }

            this.animsForSpritesDict = new Dictionary<Sprite, DbArmature>();

            //this.testPlayerAnim = new DragonBonesAnim(atlasName: "Ubbie_tex.json", skeletonName: "Ubbie_ske.json");
        }

        public DbArmature GetDragonBonesArmature(Sprite sprite)
        {
            if (!animsForSpritesDict.ContainsKey(sprite))
            {
                animsForSpritesDict[sprite] = DragonBonesAnimManager.GetDragonBonesAnim(atlasName: "Ubbie_tex.json", skeletonName: "Ubbie_ske.json");
            }

            return animsForSpritesDict[sprite];
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

    public class DragonBonesAnimManager
    {
        private static readonly string contentDirPath = Path.Combine(SonOfRobinGame.ContentMgr.RootDirectory, "gfx", "_DragonBones");

        private static readonly List<DbArmature> recentAnims;
        private static readonly Dictionary<string, DbArmature> animTemplatesById = new();

        public static DbArmature GetDragonBonesAnim(string skeletonName, string atlasName)
        {
            string id = $"{skeletonName}-{atlasName}";
            if (!animTemplatesById.ContainsKey(id)) animTemplatesById[id] = CreateNewDragonBonesArmature(skeletonName: skeletonName, atlasName: atlasName);

            return animTemplatesById[id];
        }

        public static DbArmature CreateNewDragonBonesArmature(string skeletonName, string atlasName)
        {
            string atlasJsonData = ReadFile(Path.Combine(contentDirPath, atlasName));
            string skeletonJsonData = ReadFile(Path.Combine(contentDirPath, skeletonName));

            TextureAtlas textureAtlas = TextureAtlas.FromJsonData(atlasJsonData);
            textureAtlas.LoadContent(SonOfRobinGame.ContentMgr);

            return DragonBones.FromJsonData(skeletonJsonData, textureAtlas, SonOfRobinGame.GfxDev).Armature;
        }

        private static string ReadFile(string path)
        {
            using var stream = TitleContainer.OpenStream(path);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}