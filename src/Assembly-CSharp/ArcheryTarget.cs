using System;

public class ArcheryTarget : TileObject
{
	public Character currentUser { get; private set; }

	public override Type serializedData => typeof(SaveDataArcheryTarget);

	public ArcheryTarget()
	{
		Initialize(TILE_OBJECT_TYPE.ARCHERY_TARGET);
	}

	public ArcheryTarget(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		if (data is SaveDataArcheryTarget saveDataArcheryTarget && !string.IsNullOrEmpty(saveDataArcheryTarget.currentUserID))
		{
			currentUser = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataArcheryTarget.currentUserID);
		}
	}

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		base.OnDoActionToObject(action);
		if (action.action.goapType == INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT)
		{
			currentUser = action.actor;
		}
	}

	public override void OnDoneActionToObject(ActualGoapNode action)
	{
		base.OnDoneActionToObject(action);
		if (action.action.goapType == INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT)
		{
			currentUser = null;
		}
	}

	public override void OnCancelActionTowardsObject(ActualGoapNode action)
	{
		base.OnCancelActionTowardsObject(action);
		if (action.action.goapType == INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT)
		{
			currentUser = null;
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.BARRACKS)
		{
			AddAdvertisedAction(INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT);
		}
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		RemoveAdvertisedAction(INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT);
	}

	public override string GetAdditionalTestingData()
	{
		return base.GetAdditionalTestingData() + "\n\t- Current User: " + currentUser?.name;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = currentUser;
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		if (p_character == currentUser)
		{
			currentUser = null;
		}
	}
}
