using System.Collections.Generic;
using System.Linq;
using Inner_Maps.Location_Structures;
using Traits;
using UtilityScripts;

public class VampireBehaviour : CharacterBehaviour
{
	public VampireBehaviour()
	{
		base.priority = 40;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if ((character.moodComponent.moodState == MOOD_STATE.Bad || character.moodComponent.moodState == MOOD_STATE.Critical) && character.traitContainer.HasTrait("Hemophobic"))
		{
			return character.jobComponent.TriggerSuicideJob(out producedJob, "Suicide_Reason_Hemophobic_Vampire");
		}
		if (character.characterClass.className == "Vampire Lord")
		{
			if (character.homeStructure == null || character.homeStructure.structureType != STRUCTURE_TYPE.VAMPIRE_CASTLE)
			{
				StructureSetting structureToPlace = new StructureSetting(STRUCTURE_TYPE.VAMPIRE_CASTLE, RESOURCE.STONE);
				if (character.homeSettlement != null)
				{
					LocationStructure firstUnoccupiedStructureOfType = character.homeSettlement.GetFirstUnoccupiedStructureOfType(STRUCTURE_TYPE.VAMPIRE_CASTLE);
					if (firstUnoccupiedStructureOfType != null)
					{
						character.interruptComponent.TriggerInterrupt(INTERRUPT.Set_Home, firstUnoccupiedStructureOfType.tiles.First().tileObjectComponent.genericTileObject);
						producedJob = null;
						return true;
					}
					if (GameUtilities.RollChance(15, ref log) && character.faction != null && character.faction.factionType.type != FACTION_TYPE.Vagrants && LandmarkManager.Instance.CanPlaceStructureBlueprint(character.faction.factionType.type, character.homeSettlement, structureToPlace, out var targetTile, out var structurePrefabName, out var _, out var _))
					{
						return character.jobComponent.TriggerBuildVampireCastle(targetTile, out producedJob, structurePrefabName);
					}
				}
			}
			if (character.homeStructure != null)
			{
				bool flag = false;
				for (int i = 0; i < character.homeStructure.charactersHere.Count; i++)
				{
					Character character2 = character.homeStructure.charactersHere[i];
					Prisoner traitOrStatus = character2.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
					if (character2 != character && traitOrStatus != null && !character2.isDead)
					{
						flag = true;
						break;
					}
				}
				if (!flag && (!character.traitContainer.GetTraitOrStatus<Vampire>("Vampire").dislikedBeingVampire || character.traitContainer.HasTrait("Evil", "Treacherous", "Glutton")) && GameUtilities.RollChance(15))
				{
					return character.jobComponent.TriggerImprisonBloodSource(character.homeStructure, out producedJob, ref log);
				}
			}
		}
		else
		{
			Vampire traitOrStatus2 = character.traitContainer.GetTraitOrStatus<Vampire>("Vampire");
			if (!traitOrStatus2.hasAlreadyBecomeVampireLord && PlayerSkillManager.Instance.GetAfflictionData(PLAYER_SKILL_TYPE.VAMPIRISM).HasAddedEffectForCurrentLevel(POWER_ADDED_EFFECT.Can_Become_Vampire_Lord) && !(character.characterClass.className == "Necromancer") && !(character.characterClass.className == "Werewolf") && !character.traitContainer.HasTrait("Enslaved") && ChanceData.RollChance(CHANCE_TYPE.Vampire_Lord_Chance, ref log) && traitOrStatus2.numOfConvertedVillagers >= 3)
			{
				character.interruptComponent.TriggerInterrupt(INTERRUPT.Become_Vampire_Lord, character);
				producedJob = null;
				return true;
			}
		}
		if (character.needsComponent.isSulking && GameUtilities.RollChance(10, ref log))
		{
			WeightedDictionary<Character> vampiricEmbraceTargetWeights = GetVampiricEmbraceTargetWeights(character);
			if (vampiricEmbraceTargetWeights.GetTotalOfWeights() > 0)
			{
				Character target = vampiricEmbraceTargetWeights.PickRandomElementGivenWeights();
				return character.jobComponent.CreateVampiricEmbraceJob(JOB_TYPE.VAMPIRIC_EMBRACE, target, out producedJob);
			}
		}
		producedJob = null;
		return false;
	}

	private LocationStructure GetFirstNonSettlementVampireCastles(Character character)
	{
		Region currentRegion = character.currentRegion;
		if (currentRegion != null && currentRegion.HasStructure(STRUCTURE_TYPE.VAMPIRE_CASTLE))
		{
			List<LocationStructure> structuresAtLocation = currentRegion.GetStructuresAtLocation(STRUCTURE_TYPE.VAMPIRE_CASTLE);
			if (structuresAtLocation != null)
			{
				for (int i = 0; i < structuresAtLocation.Count; i++)
				{
					LocationStructure locationStructure = structuresAtLocation[i];
					if (locationStructure.settlementLocation == null || locationStructure.settlementLocation.owner == null)
					{
						return locationStructure;
					}
				}
			}
		}
		return null;
	}

	public static WeightedDictionary<Character> GetVampiricEmbraceTargetWeights(Character character)
	{
		WeightedDictionary<Character> weightedDictionary = new WeightedDictionary<Character>();
		List<Character> list = RuinarchListPool<Character>.Claim();
		if (character.homeSettlement != null)
		{
			for (int i = 0; i < character.homeSettlement.residents.Count; i++)
			{
				Character character2 = character.homeSettlement.residents[i];
				if (character2 != character && (character2.race.IsSapient() || character2.race == RACE.RATMAN))
				{
					list.Add(character2);
				}
			}
		}
		for (int j = 0; j < character.relationshipContainer.charactersWithOpinion.Count; j++)
		{
			Character character3 = character.relationshipContainer.charactersWithOpinion[j];
			if (CharacterManager.Instance.GetCharacterByPersistentID(character3.persistentID) != null && character3 != character && (character3.race.IsSapient() || character3.race == RACE.RATMAN) && character3.homeSettlement != character.homeSettlement)
			{
				list.Add(character3);
			}
		}
		for (int k = 0; k < list.Count; k++)
		{
			Character character4 = list[k];
			AWARENESS_STATE awarenessState = character.relationshipContainer.GetAwarenessState(character, character4);
			if (character4.traitContainer.HasTrait("Vampire") || awarenessState == AWARENESS_STATE.Presumed_Dead || awarenessState == AWARENESS_STATE.Missing || character4.partyComponent.isActiveMember || character4.isDead || !character4.marker || character4.gridTileLocation == null || character4.grave != null)
			{
				continue;
			}
			string opinionLabel = character.relationshipContainer.GetOpinionLabel(character4);
			IRelationshipData relationshipDataWith = character.relationshipContainer.GetRelationshipDataWith(character4);
			if (relationshipDataWith != null && relationshipDataWith.IsLoverOrAffair() && (opinionLabel == "Close Friend" || opinionLabel == "Friend"))
			{
				weightedDictionary.AddElement(character4, 100);
				continue;
			}
			if (relationshipDataWith != null && relationshipDataWith.HasRelationship(RELATIONSHIP_TYPE.AFFAIR) && (opinionLabel == "Close Friend" || opinionLabel == "Friend"))
			{
				weightedDictionary.AddElement(character4, 50);
				continue;
			}
			switch (opinionLabel)
			{
			case "Close Friend":
				weightedDictionary.AddElement(character4, 50);
				break;
			case "Friend":
				weightedDictionary.AddElement(character4, 10);
				break;
			case "Acquaintance":
			case "Enemy":
			case "Rival":
				weightedDictionary.AddElement(character4, 5);
				break;
			default:
				weightedDictionary.AddElement(character4, 5);
				break;
			}
		}
		return weightedDictionary;
	}
}
