namespace SonOfRobin
{
    public class Flame : BoardPiece
    {
        public Flame(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, State activeState, bool serialize,
            byte animSize = 0, string animName = "default", ushort minDistance = 0, ushort maxDistance = 100, int destructionDelay = 0, bool floatsOnWater = true, int generation = 0, bool canBePickedUp = false, bool visible = true, LightEngine lightEngine = null, bool ignoresCollisions = true, AllowedDensity allowedDensity = null, int[] maxMassForSize = null) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, blocksMovement: false, minDistance: minDistance, maxDistance: maxDistance, ignoresCollisions: ignoresCollisions, name: name, destructionDelay: destructionDelay, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, maxMassForSize: maxMassForSize, generation: generation, canBePickedUp: canBePickedUp, serialize: serialize, readableName: readableName, description: description, category: Category.Indestructible, visible: visible, activeState: activeState, lightEngine: lightEngine, allowedDensity: allowedDensity, isAffectedByWind: false, fireAffinity: 0f)
        {
            this.soundPack.AddAction(action: PieceSoundPack.Action.IsOn, sound: new Sound(name: SoundData.Name.Bonfire, maxPitchVariation: 0.5f, isLooped: true));
        }

        public override void SM_FlameBurn()
        {
            if (!this.soundPack.IsPlaying(PieceSoundPack.Action.IsOn)) this.soundPack.Play(PieceSoundPack.Action.IsOn);

            // var nearbyPieces = this.world.Grid.GetPiecesWithinDistance(groupName: Cell.Group.ColMovement, mainSprite: this.sprite, distance: 700, compareWithBottom: true);

            this.Mass++; // for testing

            // TODO add code
        }
    }
}