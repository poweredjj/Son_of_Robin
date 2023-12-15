using DragonBonesMG;
using DragonBonesMG.Core;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;
using SonOfRobin;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class DragonBonesAnimManager
{
    private static readonly string contentDirPath = Path.Combine(SonOfRobinGame.ContentMgr.RootDirectory, "gfx", "_DragonBones");

    private static readonly Dictionary<string, DbArmature> animTemplatesById = new();
    private static readonly Dictionary<string, List<DbArmature>> freeAnimInstancesListsById = new();
    private static readonly Dictionary<string, List<DbArmature>> usedAnimInstancesListsById = new();
    private static readonly Dictionary<int, DbArmature> animForSpriteIdDict = new();


    private static readonly Dictionary<DbArmature, int> lastUsedInFrameDict = new();

    private static int lastUpdated = 0;

    public static DbArmature GetDragonBonesAnim(string skeletonName, string atlasName, Sprite sprite = null)
    {
        if (SonOfRobinGame.CurrentUpdate - lastUpdated > 60 * 10) Update();

        if (sprite != null && animForSpriteIdDict.ContainsKey(sprite.id))
        {
            lastUsedInFrameDict[animForSpriteIdDict[sprite.id]] = SonOfRobinGame.CurrentDraw;
            return animForSpriteIdDict[sprite.id];
        }

        string armatureId = $"{skeletonName}-{atlasName}";
        if (!animTemplatesById.ContainsKey(armatureId)) animTemplatesById[armatureId] = CreateNewDragonBonesArmature(skeletonName: skeletonName, atlasName: atlasName);

        DbArmature dbArmatureInstance = DbArmature.MakeTemplateCopy(animTemplatesById[armatureId]);
        if (!usedAnimInstancesListsById.ContainsKey(armatureId)) usedAnimInstancesListsById[armatureId] = new();
        usedAnimInstancesListsById[armatureId].Add(dbArmatureInstance);

        if (sprite != null)
        {
            animForSpriteIdDict[sprite.id] = dbArmatureInstance;
            lastUsedInFrameDict[dbArmatureInstance] = SonOfRobinGame.CurrentDraw;
        }

        return dbArmatureInstance;
    }

    private static void Update()
    {
        lastUpdated = SonOfRobinGame.CurrentUpdate;

        foreach (string armatureId in usedAnimInstancesListsById.Keys)
        {
            var unusedArmatures = usedAnimInstancesListsById[armatureId].Where(a => SonOfRobinGame.CurrentUpdate - lastUsedInFrameDict[a] > 60 * 10);
        }
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