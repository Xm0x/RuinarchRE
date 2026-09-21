using System.Collections.Generic;
using UnityEngine;
using UtilityScripts;

public abstract class CharacterBehaviour
{
	private List<Character> _isDisabledFor;

	protected BEHAVIOUR_COMPONENT_ATTRIBUTE[] attributes;

	public int priority { get; protected set; }

	public abstract bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob);

	public virtual void OnAddBehaviourToCharacter(Character character)
	{
	}

	public virtual void OnRemoveBehaviourFromCharacter(Character character)
	{
	}

	public virtual void OnLoadBehaviourToCharacter(Character character)
	{
	}

	private void DisableFor(Character character)
	{
		if (_isDisabledFor == null)
		{
			_isDisabledFor = new List<Character>();
		}
		_isDisabledFor.Add(character);
	}

	private void EnableFor(Character character)
	{
		if (!character.hasBeenCleanedUp)
		{
			_isDisabledFor.Remove(character);
		}
	}

	public bool IsDisabledFor(Character character)
	{
		if (_isDisabledFor != null)
		{
			return _isDisabledFor.Contains(character);
		}
		return false;
	}

	public bool CanDoBehaviour(Character character)
	{
		if (HasAttribute(default(BEHAVIOUR_COMPONENT_ATTRIBUTE)) && !character.IsInHomeSettlement())
		{
			return false;
		}
		return true;
	}

	public bool WillContinueProcess()
	{
		return HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING);
	}

	public bool StopsBehaviourLoop()
	{
		return HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.STOPS_BEHAVIOUR_LOOP);
	}

	public void PostProcessAfterSuccessfulDoBehaviour(Character character)
	{
		if (HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY))
		{
			DisableFor(character);
			GameDate gameDate = GameManager.Instance.Today().AddDays(1);
			gameDate.SetTicks(1);
			SchedulingManager.Instance.AddEntry(gameDate, delegate
			{
				EnableFor(character);
			}, this);
		}
	}

	protected bool HasAttribute(params BEHAVIOUR_COMPONENT_ATTRIBUTE[] passedAttributes)
	{
		if (attributes != null)
		{
			for (int i = 0; i < attributes.Length; i++)
			{
				for (int j = 0; j < passedAttributes.Length; j++)
				{
					if (attributes[i] == passedAttributes[j])
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	protected WeightedDictionary<Character> GetCharacterToVisitWeights(Character actor)
	{
		Area areaLocation = actor.areaLocation;
		if (areaLocation != null)
		{
			WeightedDictionary<Character> weightedDictionary = new WeightedDictionary<Character>();
			List<Character> list = RuinarchListPool<Character>.Claim();
			actor.relationshipContainer.PopulateAliveFriendCharacters(list);
			for (int i = 0; i < list.Count; i++)
			{
				Character character = list[i];
				if (character.homeStructure == null || !character.movementComponent.HasPathToEvenIfDiffRegion(character.homeStructure.GetRandomTile()))
				{
					continue;
				}
				if (character.faction != null && actor.faction != null)
				{
					FactionRelationship relationshipWith = character.faction.GetRelationshipWith(actor.faction);
					if (relationshipWith != null && relationshipWith.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile)
					{
						continue;
					}
				}
				if (areaLocation.GetAreaDistanceTo(character.homeStructure.occupiedArea) <= 10)
				{
					int num = 10;
					if (character.homeSettlement == actor.homeSettlement)
					{
						num += 100;
					}
					weightedDictionary.AddElement(character, num);
				}
			}
			RuinarchListPool<Character>.Release(list);
			return weightedDictionary;
		}
		return null;
	}

	protected bool DoPartyJobsInPartyJobBoard(Character p_character, Party p_party, ref JobQueueItem producedJob)
	{
		if (p_character.limiterComponent.canTakeJobs)
		{
			JobQueueItem firstJobBasedOnVision = p_party.jobBoard.GetFirstJobBasedOnVision(p_character);
			if (firstJobBasedOnVision != null)
			{
				producedJob = firstJobBasedOnVision;
				return true;
			}
			firstJobBasedOnVision = p_party.jobBoard.GetFirstUnassignedJobToCharacterJob(p_character);
			if (firstJobBasedOnVision != null)
			{
				producedJob = firstJobBasedOnVision;
				return true;
			}
		}
		return false;
	}

	protected void PartyLogic(Character character, ref string log)
	{
		if ((!character.characterClass.IsCombatant() && !(character.characterClass.className == "Noble")) || character.traitContainer.HasTrait("Enslaved") || character.faction == null)
		{
			return;
		}
		bool flag = true;
		if (character.HasAfflictedByPlayerWith(PLAYER_SKILL_TYPE.AGORAPHOBIA))
		{
			flag = !PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.AGORAPHOBIA).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.No_Longer_Join_Parties);
		}
		if (!flag || character.homeSettlement == null || character.structureComponent.HasWorkPlaceStructure() || character.crimeComponent.IsWantedBy(character.faction))
		{
			return;
		}
		if (!character.partyComponent.hasParty)
		{
			Party preferredUnfullPartyThatCharacterCanJoin = character.homeSettlement.GetPreferredUnfullPartyThatCharacterCanJoin(character);
			if (preferredUnfullPartyThatCharacterCanJoin == null)
			{
				if (GameUtilities.RollChance(10, ref log))
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Create_Party, character);
				}
			}
			else if (GameUtilities.RollChance(15, ref log))
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Join_Party, preferredUnfullPartyThatCharacterCanJoin.members[0]);
			}
			return;
		}
		bool flag2 = true;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.partyLeader != character && currentParty.partyLeader != null)
		{
			if (currentParty.partyLeader.relationshipContainer.HasGrudgeAgainst(character))
			{
				if (currentParty.partyLeader.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Free_Time)
				{
					character.interruptComponent.TriggerInterrupt(INTERRUPT.Removed_From_Party, character, "", null, "Removed_From_Party_Grudge");
					flag2 = false;
				}
			}
			else if (character.relationshipContainer.HasGrudgeAgainst(currentParty.partyLeader) && character.dailyScheduleComponent.schedule.GetScheduleType(GameManager.Instance.currentTick) == DAILY_SCHEDULE.Free_Time)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, character, "", null, "Left_Party_Grudge");
				flag2 = false;
			}
		}
		if (!flag2)
		{
			return;
		}
		if (ChanceData.RollChance(CHANCE_TYPE.Check_Free_Time_Leave_Party))
		{
			int totalOpinionOfCharacterTowardsParty = currentParty.GetTotalOpinionOfCharacterTowardsParty(character);
			if (totalOpinionOfCharacterTowardsParty < -20 && GameUtilities.RollChance(Mathf.Abs(totalOpinionOfCharacterTowardsParty)))
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Left_Party, character, "", null, "Left_Party_Does_Not_Like");
			}
		}
		else if (ChanceData.RollChance(CHANCE_TYPE.Check_Free_Time_Kickout_Party))
		{
			int totalOpinionOfPartyMembersTowardsCharacterIgnoreNewcomer = currentParty.GetTotalOpinionOfPartyMembersTowardsCharacterIgnoreNewcomer(character);
			if (totalOpinionOfPartyMembersTowardsCharacterIgnoreNewcomer < -20 && GameUtilities.RollChance(Mathf.Abs(totalOpinionOfPartyMembersTowardsCharacterIgnoreNewcomer)))
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Removed_From_Party, character, "", null, "Removed_From_Party_Others_Did_Not_Like");
			}
		}
	}

	protected bool PartyMemberPreparationBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.partyComponent.hasParty && ChanceData.RollChance(CHANCE_TYPE.Create_Food_Pack, ref log) && !character.HasItem(TILE_OBJECT_TYPE.LUNCH_PACK) && character.needsComponent.HasNeeds() && !character.traitContainer.HasTrait("Vampire"))
		{
			TileObject tileObject = null;
			for (int i = 0; i < character.ownedItems.Count; i++)
			{
				TileObject tileObject2 = character.ownedItems[i];
				if (tileObject2 is FoodPile { resourceInPile: >=10 } && tileObject2.gridTileLocation != null)
				{
					tileObject = tileObject2;
					break;
				}
			}
			if (tileObject == null && character.homeStructure != null)
			{
				List<TileObject> list = RuinarchListPool<TileObject>.Claim();
				character.homeStructure.PopulateBuiltTileObjectsOfType<FoodPile>(list);
				for (int j = 0; j < list.Count; j++)
				{
					TileObject tileObject3 = list[j];
					if (tileObject3 is FoodPile { resourceInPile: >=10 } && tileObject3.gridTileLocation != null)
					{
						tileObject = tileObject3;
						break;
					}
				}
				RuinarchListPool<TileObject>.Release(list);
			}
			if (tileObject != null && character.jobComponent.TryCreatePackFood(character, tileObject, out producedJob))
			{
				return true;
			}
		}
		producedJob = null;
		return false;
	}
}
