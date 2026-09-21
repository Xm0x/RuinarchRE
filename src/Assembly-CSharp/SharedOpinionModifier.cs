using System;
using Inner_Maps.Location_Structures;
using UnityEngine;
using UtilityScripts;

public abstract class SharedOpinionModifier : ISavable
{
	public string persistentID { get; private set; }

	public virtual string modifierName => "Shared opinion towards " + targetCharacter?.bookmarkName;

	public int modifierValue { get; protected set; }

	public Character targetCharacter { get; protected set; }

	public SharedOpinionModifierEventDispatcher eventDispatcher { get; }

	public OBJECT_TYPE objectType => OBJECT_TYPE.Shared_Opinion_Modifier;

	public virtual Type serializedData => typeof(SaveDataSharedOpinionModifier);

	protected SharedOpinionModifier(Character p_targetCharacter)
	{
		persistentID = Utilities.GetNewUniqueID();
		eventDispatcher = new SharedOpinionModifierEventDispatcher();
		targetCharacter = p_targetCharacter;
		DatabaseManager.Instance.sharedOpinionDatabase.AddSharedOpinion(this);
	}

	protected SharedOpinionModifier(SaveDataSharedOpinionModifier data)
	{
		persistentID = data.persistentID;
		modifierValue = data.modifierValue;
		eventDispatcher = new SharedOpinionModifierEventDispatcher();
		DatabaseManager.Instance.sharedOpinionDatabase.AddSharedOpinion(this);
	}

	public void LoadSecondWave(SaveDataSharedOpinionModifier data)
	{
		targetCharacter = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.targetCharacter);
		if (targetCharacter == null)
		{
			Debug.LogError("Was unable to get character with id " + data.targetCharacter + " " + GetType().ToString());
		}
	}

	public void DecreaseModifierValue(int p_amount)
	{
		modifierValue -= p_amount;
		OnDecreaseModifierValue();
	}

	public void IncreaseModifierValue(int p_amount)
	{
		modifierValue += p_amount;
		OnIncreaseModifierValue();
	}

	protected virtual void OnDecreaseModifierValue()
	{
		if (targetCharacter != null)
		{
			Messenger.Broadcast(CharacterSignals.SHARED_OPINION_MODIFIER_DECREASED, this);
		}
	}

	protected virtual void OnIncreaseModifierValue()
	{
		if (targetCharacter != null)
		{
			Messenger.Broadcast(CharacterSignals.SHARED_OPINION_MODIFIER_INCREASED, this);
		}
	}

	public virtual void OnModifierRemovedFromDatabase()
	{
	}

	public virtual bool CanAddModifierToCharacter(Character p_character)
	{
		return true;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		_ = targetCharacter;
	}
}
