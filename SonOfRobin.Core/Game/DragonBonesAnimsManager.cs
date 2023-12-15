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
    private static readonly Dictionary<string, Queue<DbArmature>> freeAnimInstancesListsById = new();
    private static readonly Dictionary<string, List<DbArmature>> usedAnimInstancesListsById = new();
    private static readonly Dictionary<Sprite, DbArmature> animForSpriteIdDict = new();

    private static readonly Dictionary<DbArmature, int> lastUsedInFrameDict = new();

    private static int lastUpdated = 0;

    public static DbArmature GetDragonBonesAnim(string skeletonName, string atlasName, Sprite sprite = null)
    {
        if (SonOfRobinGame.CurrentUpdate - lastUpdated > 60 * 10) Update();

        string armatureId = $"{skeletonName}-{atlasName}";

        if (sprite != null)
        {
            if (animForSpriteIdDict.ContainsKey(sprite))
            {
                lastUsedInFrameDict[animForSpriteIdDict[sprite]] = SonOfRobinGame.CurrentDraw;
                return animForSpriteIdDict[sprite];
            }

            if (freeAnimInstancesListsById.ContainsKey(armatureId) && freeAnimInstancesListsById[armatureId].Count > 0)
            {
                DbArmature dbArmature = freeAnimInstancesListsById[armatureId].Dequeue();
                usedAnimInstancesListsById[armatureId].Add(dbArmature);
                return dbArmature;
            }
        }

        if (!animTemplatesById.ContainsKey(armatureId))
        {
            animTemplatesById[armatureId] = CreateNewDragonBonesArmature(skeletonName: skeletonName, atlasName: atlasName);
            freeAnimInstancesListsById[armatureId] = new();
            usedAnimInstancesListsById[armatureId] = new();
        }

        DbArmature dbArmatureInstance = DbArmature.MakeTemplateCopy(animTemplatesById[armatureId]);
        usedAnimInstancesListsById[armatureId].Add(dbArmatureInstance);

        if (sprite != null)
        {
            animForSpriteIdDict[sprite] = dbArmatureInstance;
            lastUsedInFrameDict[dbArmatureInstance] = SonOfRobinGame.CurrentDraw;
        }

        return dbArmatureInstance;
    }

    private static void Update()
    {
        lastUpdated = SonOfRobinGame.CurrentUpdate;

        var offCameraAnimsBySprites = animForSpriteIdDict.Where(kvp => !kvp.Key.IsInCameraRect);

        foreach (var kvp in offCameraAnimsBySprites)
        {
            Sprite sprite = kvp.Key;
            DbArmature armature = kvp.Value;
        }


        foreach (string armatureId in usedAnimInstancesListsById.Keys)
        {
            List<DbArmature> usedArmatureList = usedAnimInstancesListsById[armatureId];
            Queue<DbArmature> freeArmatureList = freeAnimInstancesListsById[armatureId];

            foreach (DbArmature armature in usedArmatureList.ToArray())
            {
                if (SonOfRobinGame.CurrentUpdate - lastUsedInFrameDict[armature] > 60 * 10)
                {
                    usedArmatureList.Remove(armature);
                    freeArmatureList.Enqueue(armature);
                }
            }
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