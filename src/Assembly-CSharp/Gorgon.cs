using System;
using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class Gorgon : Summon
{
	public override bool defaultDigMode => true;

	public override Type serializedData => typeof(SaveDataGorgon);

	public Gorgon()
		: base(SUMMON_TYPE.Gorgon, "Gorgon", RACE.GORGON, Utilities.GetRandomGender())
	{
	}

	public Gorgon(string className)
		: base(SUMMON_TYPE.Gorgon, className, RACE.GORGON, Utilities.GetRandomGender())
	{
	}

	public Gorgon(SaveDataGorgon data)
		: base(data)
	{
	}

	public override void OnJobAddedToCharacterJobQueue(JobQueueItem job, Character character)
	{
		if (character == this && job is GoapPlanJob { isAgitateJob: not false })
		{
			base.combatComponent.SetCombatMode(COMBAT_MODE.Passive);
		}
		base.OnJobAddedToCharacterJobQueue(job, character);
	}

	public override void OnJobRemovedFromCharacterJobQueue(JobQueueItem job, Character character, bool shouldBlacklist = false)
	{
		if (character == this && job is GoapPlanJob { isAgitateJob: not false })
		{
			base.combatComponent.SetCombatMode(defaultCombatMode);
		}
		base.OnJobRemovedFromCharacterJobQueue(job, character, shouldBlacklist);
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		if (base.homeStructure != null)
		{
			if (GetStonedCountInHomeStructure() < 5)
			{
				Character targetCharacterForAgitate = GetTargetCharacterForAgitate();
				if (targetCharacterForAgitate != null)
				{
					GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.MONSTER_ABDUCT, INTERACTION_TYPE.DROP_RESTRAINED, targetCharacterForAgitate, this);
					goapPlanJob.AddOtherData(INTERACTION_TYPE.DROP_RESTRAINED, new object[1] { base.homeStructure });
					goapPlanJob.SetDoNotRecalculate(state: true);
					goapPlanJob.SetIsAgitateJob(p_state: true);
					p_agitateJob = goapPlanJob;
					CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
					return true;
				}
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Abduct_Character_No_Target);
				return false;
			}
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_1);
			return false;
		}
		CreateAgitateLog(AGITATE_MESSAGE_TYPE.No_Home);
		return false;
	}

	protected override string GetAgitateTooltipKey()
	{
		return AGITATE_MESSAGE_TYPE.Abduct_Villager_Tooltip.ToStringEnum();
	}

	private Character GetTargetCharacterForAgitate()
	{
		List<Character> list = RuinarchListPool<Character>.Claim();
		Character result = null;
		LocationGridTile locationGridTile = base.gridTileLocation;
		if (locationGridTile != null)
		{
			for (int i = 0; i < CharacterManager.Instance.allCharacters.Count; i++)
			{
				Character character = CharacterManager.Instance.allCharacters[i];
				LocationGridTile locationGridTile2 = character.gridTileLocation;
				if (!character.isDead && locationGridTile2 != null && !character.isBeingSeized && !character.isHidden && !character.isInLimbo && !character.traitContainer.HasTrait("Petrasol") && character != this && character.race != base.race && locationGridTile.area.IsNearbyTo(locationGridTile2.area) && locationGridTile2.structure != base.homeStructure && !character.HasJobTargetingThis(JOB_TYPE.MONSTER_ABDUCT))
				{
					list.Add(character);
				}
			}
		}
		if (list.Count > 0)
		{
			result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
		}
		RuinarchListPool<Character>.Release(list);
		return result;
	}

	private int GetStonedCountInHomeStructure()
	{
		int num = 0;
		if (base.homeStructure != null)
		{
			for (int i = 0; i < base.homeStructure.charactersHere.Count; i++)
			{
				Character character = base.homeStructure.charactersHere[i];
				if (!character.isDead && character.traitContainer.HasTrait("Stoned"))
				{
					num++;
				}
			}
		}
		return num;
	}

	public override bool ReactionToAnotherCharacter(Character actor, Character targetCharacter, Character disguisedActor, Character disguisedTarget, bool isHostile, ref string debugLog)
	{
		LocationGridTile locationGridTile = targetCharacter.gridTileLocation;
		if (locationGridTile != null && isHostile)
		{
			if (locationGridTile.IsInHomeOf(actor))
			{
				if (targetCharacter.traitContainer.HasTrait("Stoned", "Restrained") && GameUtilities.RollChance(25, ref debugLog))
				{
					targetCharacter.traitContainer.RemoveTrait(targetCharacter, "Stoned");
					targetCharacter.traitContainer.AddTrait(targetCharacter, "Stoned");
					return true;
				}
			}
			else if (!actor.combatComponent.isInCombat && actor.limiterComponent.canPerform && actor.limiterComponent.canMove && !targetCharacter.isDead && actor.homeStructure != null && !targetCharacter.isBeingSeized && targetCharacter.traitContainer.HasTrait("Stoned") && !actor.jobQueue.HasJob(JOB_TYPE.MONSTER_ABDUCT))
			{
				base.jobComponent.TriggerMonsterAbduct(JOB_TYPE.MONSTER_ABDUCT, targetCharacter, out var producedJob);
				if (producedJob != null && actor.jobQueue.AddJobInQueue(producedJob))
				{
					return true;
				}
			}
		}
		return base.ReactionToAnotherCharacter(actor, targetCharacter, disguisedActor, disguisedTarget, isHostile, ref debugLog);
	}
}
