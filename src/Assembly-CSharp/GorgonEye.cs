using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class GorgonEye : Artifact
{
	public GorgonEye()
		: base(ARTIFACT_TYPE.Gorgon_Eye)
	{
		base.maxHP = 700;
		base.currentHP = base.maxHP;
	}

	public GorgonEye(SaveDataArtifact data)
		: base(data)
	{
	}

	public override void ConstructDefaultPlayerActions(bool broadcastSignal = true)
	{
		base.ConstructDefaultPlayerActions(broadcastSignal);
		AddAdvertisedAction(INTERACTION_TYPE.INSPECT);
	}

	public override void ActivateTileObject()
	{
		if (gridTileLocation == null)
		{
			return;
		}
		base.ActivateTileObject();
		List<LocationGridTile> diamondTilesFromRadius = GameUtilities.GetDiamondTilesFromRadius(gridTileLocation.parentMap, gridTileLocation.localPlace, 3);
		for (int i = 0; i < diamondTilesFromRadius.Count; i++)
		{
			LocationGridTile locationGridTile = diamondTilesFromRadius[i];
			if (locationGridTile.charactersHere.Count > 0)
			{
				for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
				{
					Character character = locationGridTile.charactersHere[j];
					character.traitContainer.AddTrait(character, "Paralyzed");
				}
			}
		}
		GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Gorgon_Eye);
	}

	public override void OnInspect(Character inspector)
	{
		base.OnInspect(inspector);
		inspector.traitContainer.AddTrait(inspector, "Paralyzed");
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Tile Object", "TileObjectAlerts_Table", "Gorgon Eye inspect", LOG_TAG.Life_Changes);
		log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(this, base.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
		if (GameUtilities.RollChance(30))
		{
			gridTileLocation.structure.RemovePOI(this, inspector);
		}
	}
}
