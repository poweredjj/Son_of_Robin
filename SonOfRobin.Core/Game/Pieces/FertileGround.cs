using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class FertileGround : BoardPiece
    {

        public FertileGround(World world, int id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description, float maxHitPoints) :

            base(world: world, id: id, animPackage: animPackage, name: name, allowedTerrain: allowedTerrain, readableName: readableName, description: description, activeState: State.Empty, maxHitPoints: maxHitPoints)
        {
        }

        public override void Destroy()
        {
            if (!this.exists) return;

            if (!this.world.BuildMode) this.DestroyContainedPlants(delay: 0); // BuildMode check, to avoid invoking when destroying simulatedPieceToBuild
            base.Destroy();
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            // data to serialize here
            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            // data to deserialize here
        }
    }
}