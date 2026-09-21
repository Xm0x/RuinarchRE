using System;
using System.Collections.Generic;
using Object_Pools;
using UtilityScripts;

namespace Traits;

public class Obsessed : Status
{
	private Character _owner;

	public Character targetCharacter { get; private set; }

	public List<Character> sawCarriersOfTargetOfObsession { get; private set; }

	public override Type serializedData => typeof(SaveDataObsessed);

	public Obsessed()
	{
		name = "Obsessed";
		description = "Excessively preoccupied with the thought of another.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
		sawCarriersOfTargetOfObsession = new List<Character>();
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			character.behaviourComponent.AddBehaviourComponent(typeof(ObsessedBehaviour));
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			character.behaviourComponent.RemoveBehaviourComponent(typeof(ObsessedBehaviour));
		}
		targetCharacter = null;
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI == targetCharacter)
		{
			if (targetCharacter.traitContainer.HasTrait("Mummified") && characterThatWillDoJob.homeStructure != null && targetCharacter.grave == null)
			{
				if (targetCharacter.currentStructure != characterThatWillDoJob.homeStructure)
				{
					return characterThatWillDoJob.jobComponent.TryTriggerMoveCharacter(targetCharacter, characterThatWillDoJob.homeStructure);
				}
				if (ChanceData.RollChance(CHANCE_TYPE.Obsessed_Mummified_Actions))
				{
					if (GameUtilities.RollChance(50))
					{
						return characterThatWillDoJob.jobComponent.TriggerCleanMummifiedCorpse(targetCharacter);
					}
					return characterThatWillDoJob.jobComponent.TriggerAdoreMummifiedCorpse(targetCharacter);
				}
			}
			else if (targetCharacter.isDead && !targetCharacter.traitContainer.HasTrait("Mummified") && targetCharacter.grave == null)
			{
				return characterThatWillDoJob.jobComponent.TriggerMummifyCorpse(targetCharacter);
			}
		}
		else if (targetPOI is Character character && character.carryComponent.IsPOICarried(targetCharacter))
		{
			ObsessedCharacterSawSomeoneCarryingTargetOfObsession(characterThatWillDoJob, targetCharacter.isBeingCarriedBy);
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}

	protected override string GetDescriptionInUI()
	{
		if (targetCharacter != null)
		{
			Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Traits", "Traits_Table", "Obsessed_With");
			log.AddToFillers(targetCharacter, targetCharacter.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			string logText = log.logText;
			LogPool.Release(log);
			return logText;
		}
		return base.GetDescriptionInUI();
	}

	public override void DisconnectFromCharacter(IPointOfInterest p_owner, Character p_character)
	{
		base.DisconnectFromCharacter(p_owner, p_character);
		sawCarriersOfTargetOfObsession.Remove(p_character);
		if (p_character == targetCharacter)
		{
			targetCharacter = null;
			p_owner.traitContainer.RemoveTrait(p_owner, this);
		}
	}

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		SaveDataObsessed saveDataObsessed = p_saveDataTrait as SaveDataObsessed;
		targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(saveDataObsessed.targetCharacter);
		if (saveDataObsessed.sawCarriersOfTargetOfObsession == null)
		{
			return;
		}
		for (int i = 0; i < saveDataObsessed.sawCarriersOfTargetOfObsession.Count; i++)
		{
			Character characterByPersistentID = CharacterManager.Instance.GetCharacterByPersistentID(saveDataObsessed.sawCarriersOfTargetOfObsession[i]);
			if (characterByPersistentID != null)
			{
				sawCarriersOfTargetOfObsession.Add(characterByPersistentID);
			}
		}
	}

	private void ObsessedCharacterSawSomeoneCarryingTargetOfObsession(Character obsessedCharacter, Character carrier)
	{
		if (targetCharacter.traitContainer.HasTrait("Mummified"))
		{
			if (!sawCarriersOfTargetOfObsession.Contains(carrier))
			{
				sawCarriersOfTargetOfObsession.Add(carrier);
				obsessedCharacter.needsComponent.AdjustHappiness(-20f);
				obsessedCharacter.traitContainer.AddTrait(obsessedCharacter, "Angry", carrier);
				obsessedCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Feeling_Angry, carrier);
				obsessedCharacter.relationshipContainer.AdjustOpinion(obsessedCharacter, carrier, "Base", -20);
			}
		}
		else if (!sawCarriersOfTargetOfObsession.Contains(carrier))
		{
			sawCarriersOfTargetOfObsession.Add(carrier);
			obsessedCharacter.interruptComponent.TriggerInterrupt(INTERRUPT.Cry, targetCharacter, "", null, "Saw_Dead");
		}
	}

	public void SetTargetCharacter(Character p_source, Character p_target)
	{
		if (targetCharacter != p_target)
		{
			if (targetCharacter != null)
			{
				targetCharacter.traitComponent.RemoveObsessedCharacter(p_source);
			}
			targetCharacter = p_target;
			if (targetCharacter != null)
			{
				targetCharacter.traitComponent.AddObsessedCharacter(p_source);
			}
		}
	}

	public bool IsCharacterTargetOfObsession(Character p_target)
	{
		return p_target == targetCharacter;
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = targetCharacter;
		sawCarriersOfTargetOfObsession.Contains(p_character);
	}
}
