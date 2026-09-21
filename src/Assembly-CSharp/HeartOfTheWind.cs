using UnityEngine;
using UtilityScripts;

public class HeartOfTheWind : Artifact
{
	public HeartOfTheWind()
		: base(ARTIFACT_TYPE.Heart_Of_The_Wind)
	{
		base.maxHP = 700;
		base.currentHP = base.maxHP;
	}

	public HeartOfTheWind(SaveDataArtifact data)
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
			SpawnTornado();
		}
	}

	private void SpawnTornado()
	{
		Tornado tornado = new Tornado();
		tornado.SetRadius(1);
		tornado.SetExpiryDate(GameManager.Instance.Today().AddTicks(GameManager.Instance.GetTicksBasedOnHour(Random.Range(1, 4))));
		tornado.SetGridTileLocation(gridTileLocation);
		tornado.OnPlacePOI();
	}

	public override void OnInspect(Character inspector)
	{
		base.OnInspect(inspector);
		SpawnTornado();
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
