using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class BerserkOrb : Artifact
{
	public BerserkOrb()
		: base(ARTIFACT_TYPE.Berserk_Orb)
	{
		base.maxHP = 700;
		base.currentHP = base.maxHP;
	}

	public BerserkOrb(SaveDataArtifact data)
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
		if (gridTileLocation != null)
		{
			base.ActivateTileObject();
			GameManager.Instance.CreateParticleEffectAt(gridTileLocation, PARTICLE_EFFECT.Berserk_Orb_Activate);
			GameManager.Instance.StartCoroutine(BerserkOrbEffect(gridTileLocation));
		}
	}

	private IEnumerator BerserkOrbEffect(LocationGridTile tileLocation)
	{
		yield return GameUtilities.waitForHalfSecond;
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		tileLocation.PopulateTilesInRadius(list, 3);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			if (locationGridTile.charactersHere.Count > 0)
			{
				for (int j = 0; j < locationGridTile.charactersHere.Count; j++)
				{
					Character character = locationGridTile.charactersHere[j];
					character.traitContainer.AddTrait(character, "Berserked");
				}
			}
		}
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public override void OnInspect(Character inspector)
	{
		base.OnInspect(inspector);
		inspector.traitContainer.AddTrait(inspector, "Berserked");
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Tile Object", "TileObjectAlerts_Table", "Berserk Orb inspect", LOG_TAG.Life_Changes);
		log.AddToFillers(inspector, inspector.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		log.AddToFillers(this, base.name, LOG_IDENTIFIER.TARGET_CHARACTER);
		log.AddLogToDatabase(releaseLogAfter: true);
		if (GameUtilities.RollChance(30))
		{
			gridTileLocation.structure.RemovePOI(this, inspector);
		}
	}
}
