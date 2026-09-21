using System;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

namespace Traits;

public class BoobyTrapped : Status
{
	private ELEMENTAL_TYPE _element;

	public List<Character> awareCharacters { get; }

	private ITraitable traitable { get; set; }

	public ELEMENTAL_TYPE element => _element;

	public override Type serializedData => typeof(SaveDataBoobyTrapped);

	public BoobyTrapped()
	{
		name = "Booby Trapped";
		description = "This object will explode with [Element] damage if someone interacts with it.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		advertisedInteractions = new List<INTERACTION_TYPE> { INTERACTION_TYPE.REMOVE_TRAP };
		awareCharacters = new List<Character>(20);
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(24);
		AddTraitOverrideFunctionIdentifier("Start_Perform_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataBoobyTrapped saveDataBoobyTrapped = saveDataTrait as SaveDataBoobyTrapped;
		_element = saveDataBoobyTrapped.elementalType;
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataBoobyTrapped saveDataBoobyTrapped = p_saveDataTrait as SaveDataBoobyTrapped;
		awareCharacters.AddRange(SaveUtilities.ConvertIDListToCharacters(saveDataBoobyTrapped.awareCharacterIDs));
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		traitable = addTo;
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is BoobyTrapped boobyTrapped)
		{
			_element = boobyTrapped.element;
			awareCharacters.AddRange(boobyTrapped.awareCharacters);
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		traitable = addedTo;
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		traitable = null;
	}

	public bool OnPerformGoapAction(ActualGoapNode node, ref bool willStillContinueAction)
	{
		if ((node.action.actionCategory == ACTION_CATEGORY.DIRECT || node.action.actionCategory == ACTION_CATEGORY.CONSUME) && TryActivateTrap(node.actor, node.target, ref willStillContinueAction))
		{
			return true;
		}
		return false;
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		string testingData = base.GetTestingData(traitable);
		testingData += "\n\tAware Characters: ";
		for (int i = 0; i < awareCharacters.Count; i++)
		{
			Character character = awareCharacters[i];
			testingData = testingData + character.name + ",";
		}
		return testingData;
	}

	public override void VillagerReactionToTileObjectTrait(TileObject owner, Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObjectTrait(owner, actor, ref debugLog);
		if (actor.characterClass.className == "Archer")
		{
			actor.jobComponent.TriggerRemoveStatusTarget(owner, "Booby Trapped");
		}
	}

	public void AddAwareCharacter(Character character)
	{
		if (!awareCharacters.Contains(character))
		{
			awareCharacters.Add(character);
			if (traitable is TileObject tileObject && base.responsibleCharacter != null && (!CharacterManager.Instance.IsCultistOfSameReligion(character, base.responsibleCharacter) || tileObject.IsOwnedBy(character)))
			{
				character.jobComponent.TriggerRemoveStatusTarget(tileObject, "Booby Trapped");
			}
		}
	}

	public void RemoveAwareCharacter(Character character)
	{
		awareCharacters.Remove(character);
	}

	public bool IsImmuneToTrap(Character p_character)
	{
		if (!(p_character.characterClass.className == "Archer"))
		{
			return IsResponsibleForTrait(p_character);
		}
		return true;
	}

	private bool TryActivateTrap(Character actor, IPointOfInterest target, ref bool willStillContinueAction)
	{
		if (!IsImmuneToTrap(actor) && target.gridTileLocation != null)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Trait", "Traits_Table", name + " trap_activated", LOG_TAG.Life_Changes);
			log.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			log.AddToFillers(target, target.name, LOG_IDENTIFIER.TARGET_CHARACTER);
			log.AddLogToDatabase(releaseLogAfter: true);
			DamageTargetByTrap(actor, target);
			willStillContinueAction = false;
			return true;
		}
		return false;
	}

	public void DamageTargetByTrap(Character actor, IPointOfInterest target)
	{
		List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
		target.gridTileLocation.PopulateTilesInRadius(list, 1, 0, includeCenterTile: true, includeTilesInDifferentStructure: true);
		for (int i = 0; i < list.Count; i++)
		{
			LocationGridTile locationGridTile = list[i];
			List<IPointOfInterest> list2 = RuinarchListPool<IPointOfInterest>.Claim();
			locationGridTile.PopulatePOIsOnTile(list2);
			for (int j = 0; j < list2.Count; j++)
			{
				list2[j].AdjustHP(-800, element, triggerDeath: true);
			}
			RuinarchListPool<IPointOfInterest>.Release(list2);
		}
		target.traitContainer.RemoveTrait(target, this);
		actor.traitContainer.AddTrait(actor, "Unconscious");
		RuinarchListPool<LocationGridTile>.Release(list);
	}

	public void SetElementType(ELEMENTAL_TYPE element)
	{
		_element = element;
		description = "This object will explode with " + element.ToStringEnum() + " damage if someone interacts with it.";
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = traitable;
		awareCharacters.Contains(p_character);
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		awareCharacters.Remove(p_character);
	}
}
