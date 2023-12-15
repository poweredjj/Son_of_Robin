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
    private static readonly Dictionary<string, Queue<DbArmature>> freeAnimInstancesQueuesById = new();
    private static readonly Dictionary<string, HashSet<DbArmature>> usedAnimInstancesSetsById = new();
    private static readonly Dictionary<Sprite, DbArmature> animForSpriteDict = new();
    public static int AnimTemplatesCount { get { return animTemplatesById.Count; } }
    public static int AnimForSpriteDictCount { get { return animForSpriteDict.Count; } }
    public static int FreeAnimInstancesCount { get { return freeAnimInstancesQueuesById.Values.Sum(e => e.Count); } }
    public static int UsedAnimInstancesCount { get { return usedAnimInstancesSetsById.Values.Sum(e => e.Count); } }

    private static readonly Dictionary<DbArmature, int> lastUsedInFrameDict = new();

    private static int lastUpdated = 0;

    public static DbArmature GetDragonBonesAnim(string skeletonName, string atlasName, Sprite sprite = null)
    {
        if (SonOfRobinGame.CurrentUpdate - lastUpdated > 60 * 10) Update();

        string armatureId = $"{skeletonName}-{atlasName}";

        if (sprite != null)
        {
            if (animForSpriteDict.ContainsKey(sprite))
            {
                lastUsedInFrameDict[animForSpriteDict[sprite]] = SonOfRobinGame.CurrentDraw;
                return animForSpriteDict[sprite];
            }

            if (freeAnimInstancesQueuesById.ContainsKey(armatureId) && freeAnimInstancesQueuesById[armatureId].Count > 0)
            {
                DbArmature dbArmature = freeAnimInstancesQueuesById[armatureId].Dequeue();
                usedAnimInstancesSetsById[armatureId].Add(dbArmature);
                return dbArmature;
            }
        }

        if (!animTemplatesById.ContainsKey(armatureId))
        {
            animTemplatesById[armatureId] = CreateNewDragonBonesArmature(skeletonName: skeletonName, atlasName: atlasName);
            freeAnimInstancesQueuesById[armatureId] = new();
            usedAnimInstancesSetsById[armatureId] = new();
        }

        DbArmature dbArmatureInstance = DbArmature.MakeTemplateCopy(animTemplatesById[armatureId]);
        usedAnimInstancesSetsById[armatureId].Add(dbArmatureInstance);

        if (sprite != null)
        {
            animForSpriteDict[sprite] = dbArmatureInstance;
            lastUsedInFrameDict[dbArmatureInstance] = SonOfRobinGame.CurrentDraw;
        }

        return dbArmatureInstance;
    }

    private static void Update()
    {
        lastUpdated = SonOfRobinGame.CurrentUpdate;

        World world = World.GetTopWorld();

        var offCameraAnimsBySprites = animForSpriteDict.Where(kvp => !kvp.Key.IsInCameraRect || !kvp.Key.boardPiece.exists || kvp.Key.world != world);

        foreach (var kvp1 in offCameraAnimsBySprites)
        {
            Sprite sprite = kvp1.Key;
            DbArmature armature = kvp1.Value;

            animForSpriteDict.Remove(sprite);

            foreach (var kvp2 in usedAnimInstancesSetsById)
            {
                string armatureId = kvp2.Key;
                HashSet<DbArmature> usedArmatureSet = kvp2.Value;

                if (usedArmatureSet.Contains(armature))
                {
                    usedArmatureSet.Remove(armature);
                    freeAnimInstancesQueuesById[armatureId].Enqueue(armature);
                    break;
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