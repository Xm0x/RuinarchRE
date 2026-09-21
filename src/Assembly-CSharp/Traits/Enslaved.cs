namespace Traits;

public class Enslaved : Status
{
	public Enslaved()
	{
		name = "Enslaved";
		description = "Forced to gather food for its master.";
		thoughtText = "I miss freedom.";
		type = TRAIT_TYPE.FLAW;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		moodEffect = -8;
		hindersSocials = true;
	}

	public override void OnAddTrait(ITraitable sourcePOI)
	{
		base.OnAddTrait(sourcePOI);
		if (!(sourcePOI is Character character))
		{
			return;
		}
		if (character.partyComponent.hasParty)
		{
			character.partyComponent.currentParty.RemoveMember(character);
		}
		character.traitContainer.RemoveRestrainAndImprison(character);
		character.behaviourComponent.UpdateDefaultBehaviourSet();
		if (character.isNotSummonAndDemonAndZombie)
		{
			character.classComponent.AssignClass("Farmer");
		}
		if (base.responsibleCharacter != null)
		{
			if (base.responsibleCharacter.faction != null)
			{
				character.ChangeFactionTo(base.responsibleCharacter.faction, bypassIdeologyChecking: true);
			}
			if (base.responsibleCharacter.homeStructure != null)
			{
				character.MigrateHomeStructureTo(base.responsibleCharacter.homeStructure);
			}
			else if (base.responsibleCharacter.homeSettlement != null)
			{
				character.MigrateHomeTo(base.responsibleCharacter.homeSettlement);
			}
		}
		else if (base.responsibleCharacters != null && base.responsibleCharacters.Count > 0)
		{
			Character character2 = base.responsibleCharacters[0];
			if (character2.faction != null)
			{
				character.ChangeFactionTo(character2.faction, bypassIdeologyChecking: true);
			}
			if (character2.homeStructure != null)
			{
				character.MigrateHomeStructureTo(character2.homeStructure);
			}
			else if (character2.homeSettlement != null)
			{
				character.MigrateHomeTo(character2.homeSettlement);
			}
		}
		character.jobComponent.AddAbleJob(JOB_TYPE.PRODUCE_FOOD);
		character.jobComponent.AddAbleJob(JOB_TYPE.HAUL);
		Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)character);
	}

	public override void OnRemoveTrait(ITraitable sourcePOI, Character removedBy)
	{
		base.OnRemoveTrait(sourcePOI, removedBy);
		if (sourcePOI is Character character)
		{
			character.ChangeToDefaultFaction();
			character.MigrateHomeStructureTo(null);
			character.behaviourComponent.UpdateDefaultBehaviourSet();
			character.jobComponent.RemoveAbleJob(JOB_TYPE.PRODUCE_FOOD);
			character.jobComponent.RemoveAbleJob(JOB_TYPE.HAUL);
			Messenger.Broadcast(PlayerSkillSignals.RELOAD_PLAYER_ACTIONS, (IPlayerActionTarget)character);
		}
	}
}
