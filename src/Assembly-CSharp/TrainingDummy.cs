using System;

public class TrainingDummy : TileObject
{
	public Character currentUser { get; private set; }

	public override Type serializedData => typeof(SaveDataTrainingDummy);

	public TrainingDummy()
	{
		Initialize(TILE_OBJECT_TYPE.TRAINING_DUMMY);
	}

	public TrainingDummy(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void LoadSecondWave(SaveDataTileObject data)
	{
		base.LoadSecondWave(data);
		SaveDataTrainingDummy saveDataTrainingDummy = data as SaveDataTrainingDummy;
		if (!string.IsNullOrEmpty(saveDataTrainingDummy.currentUserID))
		{
			currentUser = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(saveDataTrainingDummy.currentUserID);
		}
	}

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		base.OnDoActionToObject(action);
		if (action.action.goapType == INTERACTION_TYPE.TRAIN_COMBAT_MAGIC || action.action.goapType == INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT)
		{
			currentUser = action.actor;
		}
	}

	public override void OnDoneActionToObject(ActualGoapNode action)
	{
		base.OnDoneActionToObject(action);
		if (action.action.goapType == INTERACTION_TYPE.TRAIN_COMBAT_MAGIC || action.action.goapType == INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT)
		{
			currentUser = null;
		}
	}

	public override void OnCancelActionTowardsObject(ActualGoapNode action)
	{
		base.OnCancelActionTowardsObject(action);
		if (action.action.goapType == INTERACTION_TYPE.TRAIN_COMBAT_MAGIC || action.action.goapType == INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT)
		{
			currentUser = null;
		}
	}

	public override void OnPlacePOI()
	{
		base.OnPlacePOI();
		if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.MAGIC_ACADEMY)
		{
			AddAdvertisedAction(INTERACTION_TYPE.TRAIN_COMBAT_MAGIC);
			return;
		}
		if (gridTileLocation != null && gridTileLocation.structure.structureType == STRUCTURE_TYPE.BARRACKS)
		{
			AddAdvertisedAction(INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT);
			return;
		}
		RemoveAdvertisedAction(INTERACTION_TYPE.TRAIN_COMBAT_MAGIC);
		RemoveAdvertisedAction(INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT);
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		RemoveAdvertisedAction(INTERACTION_TYPE.TRAIN_COMBAT_MAGIC);
		RemoveAdvertisedAction(INTERACTION_TYPE.TRAIN_PHYSICAL_COMBAT);
	}

	public override string GetAdditionalTestingData()
	{
		return base.GetAdditionalTestingData() + "\n\t- Current User: " + currentUser?.name;
	}

	protected override void DisconnectFromCharacter(Character p_character)
	{
		base.DisconnectFromCharacter(p_character);
		if (currentUser == p_character)
		{
			currentUser = null;
		}
	}
}
