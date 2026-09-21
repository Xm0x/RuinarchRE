using System;
using Characters.Components;
using Traits;

public class BedClinic : BaseBed, CharacterEventDispatcher.ITraitListener
{
	public override Type serializedData => typeof(SaveDataBedClinic);

	public BedClinic()
		: base(1)
	{
		Initialize(TILE_OBJECT_TYPE.BED_CLINIC);
		AddAdvertisedAction(INTERACTION_TYPE.SLEEP);
		AddAdvertisedAction(INTERACTION_TYPE.NAP);
		AddAdvertisedAction(INTERACTION_TYPE.RECUPERATE);
	}

	public BedClinic(SaveDataTileObject data)
		: base(data, 1)
	{
	}

	public override void LoadAdditionalInfo(SaveDataTileObject data)
	{
		base.LoadAdditionalInfo(data);
		if (data is SaveDataBedClinic saveDataBedClinic)
		{
			for (int i = 0; i < saveDataBedClinic.userIDS.Count; i++)
			{
				string text = saveDataBedClinic.userIDS[i];
				Character characterByPersistentID = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(text);
				LoadUser(characterByPersistentID);
				characterByPersistentID.eventDispatcher.SubscribeToCharacterLostTrait(this);
			}
		}
		AddAdvertisedAction(INTERACTION_TYPE.SLEEP);
		AddAdvertisedAction(INTERACTION_TYPE.NAP);
		AddAdvertisedAction(INTERACTION_TYPE.RECUPERATE);
	}

	public override void OnDoActionToObject(ActualGoapNode action)
	{
		base.OnDoActionToObject(action);
		switch (action.goapType)
		{
		case INTERACTION_TYPE.QUARANTINE:
			AddUser(action.poiTarget as Character);
			break;
		case INTERACTION_TYPE.SLEEP:
		case INTERACTION_TYPE.NAP:
			mapVisual?.UpdateTileObjectVisual(this);
			break;
		case INTERACTION_TYPE.RECUPERATE:
			AddUser(action.actor);
			mapVisual?.UpdateTileObjectVisual(this);
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
			mapVisual?.UpdateTileObjectVisual(this);
			break;
		case INTERACTION_TYPE.RECUPERATE:
			RemoveUser(action.actor);
			mapVisual?.UpdateTileObjectVisual(this);
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
			if (gridTileLocation != null && mapVisual != null)
			{
				mapVisual.UpdateTileObjectVisual(this);
			}
			break;
		case INTERACTION_TYPE.RECUPERATE:
			RemoveUser(action.actor);
			if (gridTileLocation != null && mapVisual != null)
			{
				mapVisual.UpdateTileObjectVisual(this);
			}
			break;
		}
	}

	protected override bool AddUser(Character character)
	{
		if (base.AddUser(character))
		{
			character.eventDispatcher.SubscribeToCharacterLostTrait(this);
			return true;
		}
		return false;
	}

	public override bool RemoveUser(Character character)
	{
		if (base.RemoveUser(character))
		{
			character.eventDispatcher.UnsubscribeToCharacterLostTrait(this);
			character.traitContainer.RemoveTrait(character, "Quarantined");
			return true;
		}
		return false;
	}

	public override void OnDestroyPOI(Character p_destroyer = null)
	{
		base.OnDestroyPOI(p_destroyer);
		Character[] array = users;
		if (array == null || array.Length == 0)
		{
			return;
		}
		foreach (Character character in array)
		{
			if (character != null)
			{
				RemoveUser(character);
			}
		}
	}

	public void OnCharacterGainedTrait(Character p_character, Trait p_gainedTrait)
	{
	}

	public void OnCharacterLostTrait(Character p_character, Trait p_lostTrait, Character p_removedBy)
	{
		if (p_lostTrait is Quarantined)
		{
			RemoveUser(p_character);
		}
	}
}
