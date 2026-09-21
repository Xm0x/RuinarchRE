using System;
using System.Collections.Generic;

namespace Traits;

public class Landmined : Status
{
	public List<Character> awareCharacters { get; private set; }

	public GameDate dateAdded { get; private set; }

	public override Type serializedData => typeof(SaveDataLandmined);

	public Landmined()
	{
		name = "Landmined";
		description = "This object has a hidden landmine.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.REMOVE_TRAP };
		ticksDuration = 0;
		isTangible = true;
		isHidden = true;
		awareCharacters = new List<Character>(20);
		AddTraitOverrideFunctionIdentifier("Villager_Reaction");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataLandmined saveDataLandmined = saveDataTrait as SaveDataLandmined;
		dateAdded = saveDataLandmined.dateAdded;
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataLandmined saveDataLandmined = p_saveDataTrait as SaveDataLandmined;
		awareCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataLandmined.awareCharacterIDs));
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is GenericTileObject)
		{
			dateAdded = GameManager.Instance.Today();
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		awareCharacters.Clear();
		awareCharacters = null;
		dateAdded = default(GameDate);
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		awareCharacters.Remove(p_character);
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		string text = base.GetTestingData(traitable);
		if (awareCharacters != null)
		{
			text = text + "/nAware Characters: " + awareCharacters.ComafyList();
		}
		return text;
	}

	public void AddAwareCharacter(Character p_character)
	{
		if (!awareCharacters.Contains(p_character))
		{
			awareCharacters.Add(p_character);
		}
	}

	public override void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObjectTrait(owner, actor, ref debugLog);
		if (!actor.combatComponent.isInActualCombat && !owner.HasJobTargetingThis(JOB_TYPE.REMOVE_TRAP))
		{
			if (awareCharacters.Contains(actor) && !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.REMOVE_TRAP))
			{
				actor.jobComponent.TriggerRemovePlayerTrap(actor, owner, "Landmined");
			}
			else if (actor.characterClass.className == "Archer")
			{
				actor.jobComponent.TriggerRemovePlayerTrap(actor, owner, "Landmined");
			}
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		awareCharacters.Contains(p_character);
	}
}
