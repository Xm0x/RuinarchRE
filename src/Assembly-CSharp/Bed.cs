public class Bed : BaseBed
{
	public Bed()
		: base(2)
	{
		Initialize(TILE_OBJECT_TYPE.BED);
		AddAdvertisedAction(INTERACTION_TYPE.SLEEP);
		AddAdvertisedAction(INTERACTION_TYPE.NAP);
	}

	public Bed(SaveDataTileObject data)
		: base(data, 2)
	{
	}

	public override string ToString()
	{
		return "Bed " + base.id;
	}

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		base.OnDoActionToObject(action);
		switch (action.goapType)
		{
		case INTERACTION_TYPE.SLEEP:
		case INTERACTION_TYPE.NAP:
			AddUser(action.actor);
			break;
		case INTERACTION_TYPE.MAKE_LOVE:
			AddUser(action.actor);
			AddUser(action.poiTarget as Character);
			break;
		}
	}

	public override void OnDoneActionToObject(ActualGoapNode action)
	{
		base.OnDoneActionToObject(action);
		switch (action.goapType)
		{
		case INTERACTION_TYPE.SLEEP:
		case INTERACTION_TYPE.NAP:
			RemoveUser(action.actor);
			break;
		case INTERACTION_TYPE.MAKE_LOVE:
			RemoveUser(action.actor);
			RemoveUser(action.poiTarget as Character);
			break;
		}
	}

	public override void OnCancelActionTowardsObject(ActualGoapNode action)
	{
		base.OnCancelActionTowardsObject(action);
		switch (action.goapType)
		{
		case INTERACTION_TYPE.SLEEP:
		case INTERACTION_TYPE.NAP:
			RemoveUser(action.actor);
			break;
		case INTERACTION_TYPE.MAKE_LOVE:
			RemoveUser(action.actor);
			RemoveUser(action.poiTarget as Character);
			break;
		}
	}

	protected override bool AddUser(Character character)
	{
		if (base.AddUser(character))
		{
			if (!IsSlotAvailable())
			{
				SetPOIState(POI_STATE.INACTIVE);
			}
			return true;
		}
		return false;
	}

	public override bool RemoveUser(Character character)
	{
		if (base.RemoveUser(character))
		{
			if (IsSlotAvailable() && !base.traitContainer.HasTrait("Burning"))
			{
				SetPOIState(POI_STATE.ACTIVE);
			}
			return true;
		}
		return false;
	}

	protected override void OnSetObjectAsUnbuilt()
	{
		base.OnSetObjectAsUnbuilt();
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
		AddAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
	}

	protected override void OnSetObjectAsBuilt()
	{
		base.OnSetObjectAsBuilt();
		RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_STONE);
		RemoveAdvertisedAction(INTERACTION_TYPE.CRAFT_FURNITURE_WOOD);
	}

	public void WakeUpUsersBecauseOfWet()
	{
		Character[] array = users;
		if (array == null)
		{
			return;
		}
		foreach (Character character in array)
		{
			if (character != null && character.traitContainer.HasTrait("Resting"))
			{
				character.currentJob?.CancelJob();
				character.traitContainer.AddTrait(character, "Irate");
			}
		}
	}

	public override bool CanUseBed(Character character)
	{
		if (base.CanUseBed(character))
		{
			for (int i = 0; i < users.Length; i++)
			{
				if (users[i] != null)
				{
					Character character2 = users[i];
					if (!character.relationshipContainer.HasRelationshipWith(character2) || character.relationshipContainer.IsEnemiesWith(character2) || character.relationshipContainer.HasOpinionLabelWithCharacter(character2, "Acquaintance"))
					{
						return false;
					}
				}
			}
		}
		return true;
	}
}
