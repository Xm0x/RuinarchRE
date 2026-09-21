using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class DemonRescueBehaviour : CharacterBehaviour
{
	public DemonRescueBehaviour()
	{
		base.priority = 200;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		bool result = false;
		Party currentParty = character.partyComponent.currentParty;
		if (currentParty.isActive && currentParty.partyState == PARTY_STATE.Working)
		{
			DemonRescuePartyQuest demonRescuePartyQuest = currentParty.currentQuest as DemonRescuePartyQuest;
			DemonicStructure targetDemonicStructure = demonRescuePartyQuest.targetDemonicStructure;
			bool hasEndQuest = false;
			demonRescuePartyQuest.CultistBetrayalProcessing(ref hasEndQuest);
			if (hasEndQuest)
			{
				return true;
			}
			if (!currentParty.IsMember(character) || !character.partyComponent.isMemberThatJoinedQuest)
			{
				return true;
			}
			if (targetDemonicStructure.hasBeenDestroyed || targetDemonicStructure.objectsThatContributeToDamage.Count <= 0)
			{
				if (IsInTargetDemonicStructure(character, demonRescuePartyQuest))
				{
					if (!character.hasMarker || !IsInTargetDemonicStructure(demonRescuePartyQuest.targetCharacter, demonRescuePartyQuest))
					{
						demonRescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Nowhere"));
						return true;
					}
					if (!demonRescuePartyQuest.targetCharacter.isDead)
					{
						if (demonRescuePartyQuest.targetCharacter.traitContainer.HasTrait("Restrained", "Unconscious", "Frozen", "Ensnared", "Enslaved"))
						{
							result = character.jobComponent.TriggerReleaseJob(JOB_TYPE.RELEASE_CHARACTER, demonRescuePartyQuest.targetCharacter, out producedJob);
							if (result)
							{
								demonRescuePartyQuest.SetIsReleasing(state: true);
							}
							return result;
						}
						demonRescuePartyQuest.SetIsSuccessful(state: true);
						demonRescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Safe"));
						if (demonRescuePartyQuest.targetCharacter.traitContainer.HasTrait("Paralyzed") && !demonRescuePartyQuest.targetCharacter.IsPOICurrentlyTargetedByAPerformingAction(JOB_TYPE.RESCUE_MOVE_CHARACTER))
						{
							character.jobComponent.TryTriggerRescueMoveCharacter(demonRescuePartyQuest.targetCharacter, out producedJob);
						}
						return true;
					}
					demonRescuePartyQuest.EndQuest(PartyQuest.GetLocalizedEndQuestReason("Target_Dead"));
				}
				else
				{
					LocationGridTile randomPassableTileThatHasPathToFor = GetRandomPassableTileThatHasPathToFor(character, demonRescuePartyQuest.targetDemonicStructureTiles);
					if (randomPassableTileThatHasPathToFor != null)
					{
						return character.jobComponent.CreatePartyGoToSpecificTileJob(randomPassableTileThatHasPathToFor, out producedJob);
					}
				}
			}
			else
			{
				TileObject tileObject = null;
				IDamageable nearestDamageableThatContributeToHP = targetDemonicStructure.GetNearestDamageableThatContributeToHP(character.gridTileLocation);
				if (nearestDamageableThatContributeToHP != null && nearestDamageableThatContributeToHP is TileObject tileObject2)
				{
					tileObject = tileObject2;
				}
				if (tileObject != null)
				{
					character.combatComponent.Fight(tileObject, "Clear_Demonic_Intrusion");
					return true;
				}
			}
			result = RoamAroundStructureOrHex(character, currentParty.currentQuest.target, out producedJob);
		}
		if (producedJob != null)
		{
			producedJob.SetIsThisAPartyJob(state: true);
		}
		return result;
	}

	private bool RoamAroundStructureOrHex(Character actor, IPartyQuestTarget target, out JobQueueItem producedJob)
	{
		if (target != null && target.currentStructure != null && target.currentStructure.structureType == STRUCTURE_TYPE.WILDERNESS && target is Character { gridTileLocation: not null, areaLocation: var areaLocation })
		{
			JOB_TYPE jobType = JOB_TYPE.ROAM_AROUND_STRUCTURE;
			if (!actor.IsAtHome())
			{
				jobType = JOB_TYPE.IDLE_RETURN_HOME_HIGHER;
			}
			return actor.jobComponent.TriggerRoamAroundTile(jobType, out producedJob, areaLocation.gridTileComponent.GetRandomTile());
		}
		return actor.jobComponent.TriggerRoamAroundStructure(out producedJob);
	}

	private bool IsInTargetDemonicStructure(Character p_character, DemonRescuePartyQuest p_quest)
	{
		if (!p_quest.targetDemonicStructure.hasBeenDestroyed)
		{
			return p_character.currentStructure == p_quest.targetDemonicStructure;
		}
		return p_quest.targetDemonicStructureTiles.Contains(p_character.gridTileLocation);
	}

	private LocationGridTile GetRandomPassableTileThatHasPathToFor(Character p_character, List<LocationGridTile> p_tiles)
	{
		LocationGridTile result = null;
		if (p_character.gridTileLocation != null)
		{
			List<LocationGridTile> list = RuinarchListPool<LocationGridTile>.Claim();
			for (int i = 0; i < p_tiles.Count; i++)
			{
				LocationGridTile locationGridTile = p_tiles[i];
				if (p_character.movementComponent.HasPathToEvenIfDiffRegion(locationGridTile) && locationGridTile.IsPassable())
				{
					list.Add(locationGridTile);
				}
			}
			if (list.Count > 0)
			{
				result = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			}
			RuinarchListPool<LocationGridTile>.Release(list);
		}
		return result;
	}
}
