using System;
using System.Collections.Generic;

namespace SonOfRobin
{
    public class Seed : BoardPiece
    {
        private PieceTemplate.Name plantToGrow;

        public Seed(World world, string id, AnimData.PkgName animPackage, PieceTemplate.Name name, AllowedTerrain allowedTerrain, string readableName, string description,
            byte animSize = 0, string animName = "default", bool floatsOnWater = false, int generation = 0, bool rotatesWhenDropped = true) :

            base(world: world, id: id, animPackage: animPackage, animSize: animSize, animName: animName, name: name, allowedTerrain: allowedTerrain, floatsOnWater: floatsOnWater, generation: generation, rotatesWhenDropped: rotatesWhenDropped, readableName: readableName, description: description, activeState: State.Empty)
        {
        }

        public bool PlantToGrowHasBeenAssigned
        { get { return this.plantToGrow != PieceTemplate.Name.Empty; } }

        public PieceTemplate.Name PlantToGrow
        {
            get
            {
                if (!this.PlantToGrowHasBeenAssigned) throw new ArgumentException("PlantToGrow has not been assigned.");
                return this.plantToGrow;
            }
            set
            {
                if (this.PlantToGrowHasBeenAssigned) throw new ArgumentException($"Cannot assign {value}, because plant name has already been assigned.");
                this.plantToGrow = value;
            }
        }

        public override Dictionary<string, Object> Serialize()
        {
            Dictionary<string, Object> pieceData = base.Serialize();
            pieceData["seed_plantToGrow"] = this.plantToGrow;

            return pieceData;
        }

        public override void Deserialize(Dictionary<string, Object> pieceData)
        {
            base.Deserialize(pieceData);
            this.plantToGrow = (PieceTemplate.Name)(Int64)pieceData["seed_plantToGrow"];
        }
    }
}