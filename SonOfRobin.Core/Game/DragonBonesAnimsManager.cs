using DragonBonesMG;
using DragonBonesMG.Core;
using DragonBonesMG.Display;
using Microsoft.Xna.Framework;
using SonOfRobin;
using System.Collections.Generic;
using System.IO;

public class DragonBonesAnimManager
{
    private static readonly Dictionary<string, DbArmature> animTemplatesById = new();
    private static readonly Dictionary<string, List<DbArmature>> animInstancesById = new();

    public static readonly Dictionary<int, DbArmature> animForSpriteIdDict = new();

    private static readonly string contentDirPath = Path.Combine(SonOfRobinGame.ContentMgr.RootDirectory, "gfx", "_DragonBones");

    private static readonly List<DbArmature> recentAnims;

    public static DbArmature GetDragonBonesAnim(string skeletonName, string atlasName, Sprite sprite = null)
    {
        if (sprite != null && animForSpriteIdDict.ContainsKey(sprite.id)) return animForSpriteIdDict[sprite.id];

        string id = $"{skeletonName}-{atlasName}";
        if (!animTemplatesById.ContainsKey(id)) animTemplatesById[id] = CreateNewDragonBonesArmature(skeletonName: skeletonName, atlasName: atlasName);

        DbArmature dbArmatureInstance = DbArmature.MakeTemplateCopy(animTemplatesById[id]);
        if (!animInstancesById.ContainsKey(id)) animInstancesById[id] = new();
        animInstancesById[id].Add(dbArmatureInstance);

        if (sprite != null) animForSpriteIdDict[sprite.id] = dbArmatureInstance;

        return dbArmatureInstance;
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
