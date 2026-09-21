using System;
using System.Collections.Generic;
using UtilityScripts;

namespace Traits;

public class Interesting : Trait
{
	public List<Character> charactersThatSaw { get; private set; }

	public override Type serializedData => typeof(SaveDataInteresting);

	public Interesting()
	{
		name = "Interesting";
		description = "An interesting thing.";
		type = TRAIT_TYPE.NEUTRAL;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		isHidden = true;
		advertisedInteractions = new List<INTERACTION_TYPE>
		{
			INTERACTION_TYPE.INSPECT,
			INTERACTION_TYPE.ASSAULT
		};
		charactersThatSaw = new List<Character>();
		AddTraitOverrideFunctionIdentifier("Villager_Reaction");
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataInteresting saveDataInteresting = p_saveDataTrait as SaveDataInteresting;
		charactersThatSaw.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataInteresting.charactersThatSaw));
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		charactersThatSaw.Remove(p_character);
	}

	public void AddCharacterThatSaw(Character character)
	{
		charactersThatSaw.Add(character);
	}

	public bool HasAlreadyBeenSeenByCharacter(Character character)
	{
		return charactersThatSaw.Contains(character);
	}

	public override void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObjectTrait(owner, actor, ref debugLog);
		if (HasAlreadyBeenSeenByCharacter(actor))
		{
			return;
		}
		AddCharacterThatSaw(actor);
		if (actor.traitContainer.HasTrait("Suspicious"))
		{
			if (GameUtilities.RollChance(50))
			{
				actor.jobComponent.TriggerDestroy(owner, "Destroy_Suspicious");
			}
			else
			{
				actor.interruptComponent.TriggerInterrupt(INTERRUPT.Wary, owner);
			}
		}
		else if (owner.tileObjectType == TILE_OBJECT_TYPE.WEREWOLF_PELT && actor.classComponent.IsStalkerCannotBeTurned())
		{
			actor.jobComponent.TriggerDestroy(owner, "Destroy_Stalker_Werewolf_Pelt");
		}
		else if (GameUtilities.RollChance(50) && !actor.jobQueue.HasJob(JOB_TYPE.INSPECT, owner) && !actor.defaultCharacterTrait.HasAlreadyInspectedObject(owner))
		{
			actor.jobComponent.TriggerInspect(owner);
		}
		else if (!actor.IsInventoryAtFullCapacity() && !actor.HasItem(owner.name) && !actor.HasOwnedItemInHomeStructure(owner.name))
		{
			actor.jobComponent.CreateTakeItemOnSightJob(owner);
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		charactersThatSaw.Contains(p_character);
	}
}
