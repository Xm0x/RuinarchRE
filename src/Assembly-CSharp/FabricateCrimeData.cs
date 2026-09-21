using System.Collections.Generic;
using UtilityScripts;

public class FabricateCrimeData : PlayerAction
{
	public override PLAYER_SKILL_TYPE type => PLAYER_SKILL_TYPE.FABRICATE_CRIME;

	public override string name => "Fabricate Crime";

	public override string description => "This Ability instructs the character to create a fake crime against a target and then share it with someone. Only available on Cultists.";

	public override bool canBeCastOnBlessed => true;

	public FabricateCrimeData()
	{
		base.targetTypes = new SPELL_TARGET[1] { SPELL_TARGET.CHARACTER };
	}

	public override void ActivateAbility(IPointOfInterest targetPOI)
	{
		Character character = targetPOI as Character;
		if (character != null)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			character.PopulateListOfCultistTargets(list, (Character x) => x.isNormalCharacter && x.race.IsSapient() && !x.isDead && x.faction == character.faction);
			UIManager.Instance.ShowClickableObjectPicker(list, delegate(object o)
			{
				OnChooseCharacter(o, character);
			}, null, null, "", null, null, "", showCover: true, 25);
			RuinarchListPool<Character>.Release(list);
		}
	}

	public override bool CanPerformAbilityTowards(Character targetCharacter)
	{
		if (base.CanPerformAbilityTowards(targetCharacter))
		{
			if (targetCharacter.isDead)
			{
				return false;
			}
			if (!targetCharacter.faction.isMajorNonPlayer)
			{
				return false;
			}
			if (!targetCharacter.limiterComponent.canPerform)
			{
				return false;
			}
			if (targetCharacter.traitContainer.HasTrait("Enslaved"))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool IsValid(IPlayerActionTarget target)
	{
		if (!PlayerSkillManager.Instance.unlockAllSkills && !PlayerSkillManager.Instance.selectedArchetype.IsPuppetmasterLoadout())
		{
			return false;
		}
		return base.IsValid(target);
	}

	public override string GetReasonsWhyCannotPerformAbilityTowards(Character targetCharacter)
	{
		string text = base.GetReasonsWhyCannotPerformAbilityTowards(targetCharacter);
		if (!targetCharacter.faction.isMajorNonPlayer)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Not_In_Major_Faction") + "|";
		}
		if (!targetCharacter.limiterComponent.canPerform)
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Target_Incapacitated") + "|";
		}
		if (targetCharacter.traitContainer.HasTrait("Enslaved"))
		{
			text = text + GetLocalizedReasonWhyCannotPerformAbilityTowards("Slave_Cannot_Perform") + "|";
		}
		return text;
	}

	private void OnChooseCharacter(object obj, Character cultist)
	{
		if (!(obj is Character character))
		{
			return;
		}
		UIManager.Instance.HideObjectPicker();
		List<CRIME_TYPE> list = RuinarchListPool<CRIME_TYPE>.Claim();
		List<INTERACTION_TYPE> list2 = RuinarchListPool<INTERACTION_TYPE>.Claim();
		character.faction.factionType.PopulateCrimeTypesBySeverity(list, CRIME_SEVERITY.Heinous, CRIME_SEVERITY.Serious);
		list.Remove(CRIME_TYPE.Murder);
		InteractionManager.Instance.PopulateActionTypesBasedOnCrimeType(list2, character, list);
		INTERACTION_TYPE iNTERACTION_TYPE = INTERACTION_TYPE.NONE;
		if (list2.Count > 0)
		{
			iNTERACTION_TYPE = list2[GameUtilities.RandomBetweenTwoNumbers(0, list2.Count - 1)];
		}
		RuinarchListPool<INTERACTION_TYPE>.Release(list2);
		RuinarchListPool<CRIME_TYPE>.Release(list);
		ActualGoapNode actualGoapNode = null;
		if (iNTERACTION_TYPE != INTERACTION_TYPE.NONE)
		{
			if (!InteractionManager.Instance.goapActionData[iNTERACTION_TYPE].isTargetSelf)
			{
				Character randomAliveCharacterWithOpinion = character.relationshipContainer.GetRandomAliveCharacterWithOpinion();
				if (randomAliveCharacterWithOpinion != null)
				{
					actualGoapNode = InteractionManager.Instance.CreateNewIllusionAction(character, randomAliveCharacterWithOpinion, iNTERACTION_TYPE, shouldLog: false);
					actualGoapNode.SetIsFabricated(p_state: true);
				}
			}
			else
			{
				actualGoapNode = InteractionManager.Instance.CreateNewIllusionAction(character, character, iNTERACTION_TYPE, shouldLog: false);
				actualGoapNode.SetIsFabricated(p_state: true);
			}
		}
		if (actualGoapNode != null)
		{
			Character character2 = null;
			List<Character> list3 = RuinarchListPool<Character>.Claim();
			for (int i = 0; i < cultist.faction.characters.Count; i++)
			{
				Character character3 = cultist.faction.characters[i];
				if (!character3.isDead && character3.IsAtHome() && character3.limiterComponent.canPerform && character3 != cultist && character3 != actualGoapNode.actor && character3 != actualGoapNode.target)
				{
					list3.Add(character3);
				}
			}
			if (list3.Count > 0)
			{
				character2 = list3[GameUtilities.RandomBetweenTwoNumbers(0, list3.Count - 1)];
			}
			if (character2 != null)
			{
				cultist.jobComponent.CreateSpreadNegativeInfoJob(JOB_TYPE.CULTIST_INSTRUCTION, character2, actualGoapNode);
			}
			RuinarchListPool<Character>.Release(list3);
		}
		base.ActivateAbility((IPointOfInterest)cultist);
	}
}
