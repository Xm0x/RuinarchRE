using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

public class VengefulGhostBehaviour : BaseMonsterBehaviour
{
	public VengefulGhostBehaviour()
	{
		base.priority = 8;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		if (character.gridTileLocation != null)
		{
			if (character.homeStructure == null && !character.HasTerritory())
			{
				character.SetTerritory(character.gridTileLocation.area);
			}
			if (character.faction != null && !character.faction.IsFriendlyWith(PlayerManager.Instance.player.playerFaction))
			{
				character.traitContainer.RemoveTrait(character, "Invader");
				if (!character.behaviourComponent.isAttackingDemonicStructure && GameUtilities.RollChance(15))
				{
					LocationStructure randomStructureInRegion = PlayerManager.Instance.player.playerSettlement.GetRandomStructureInRegion(character.currentRegion);
					if (randomStructureInRegion != null)
					{
						character.behaviourComponent.SetIsAttackingDemonicStructure(state: true, randomStructureInRegion as DemonicStructure);
						return true;
					}
				}
				if (GameUtilities.RollChance(20))
				{
					Character randomCharacterThatIsNotDeadAndInDemonFactionAndHasPathTo = character.currentRegion.GetRandomCharacterThatIsNotDeadAndInDemonFactionAndHasPathTo(character);
					if (randomCharacterThatIsNotDeadAndInDemonFactionAndHasPathTo != null)
					{
						character.combatComponent.Fight(randomCharacterThatIsNotDeadAndInDemonFactionAndHasPathTo, "Hostility");
						return true;
					}
				}
				if (character.homeStructure != null)
				{
					if (!character.isAtHomeStructure)
					{
						return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
					}
					return character.jobComponent.TriggerRoamAroundTile(out producedJob);
				}
				if (character.HasTerritory() && !character.IsInTerritory())
				{
					return character.jobComponent.PlanReturnHome(JOB_TYPE.IDLE_RETURN_HOME, out producedJob);
				}
				return character.jobComponent.TriggerRoamAroundTile(out producedJob);
			}
			if (!character.traitContainer.HasTrait("Invader"))
			{
				character.traitContainer.AddTrait(character, "Invader");
				return true;
			}
		}
		return false;
	}

	protected override bool TamedBehaviour(Character p_character, ref string p_log, out JobQueueItem p_producedJob)
	{
		p_producedJob = null;
		if (p_character.faction != null && !p_character.faction.IsFriendlyWith(PlayerManager.Instance.player.playerFaction) && !p_character.isRecruitedByAMajorFaction)
		{
			if (!p_character.behaviourComponent.isAttackingDemonicStructure && GameUtilities.RollChance(15))
			{
				LocationStructure randomStructureInRegion = PlayerManager.Instance.player.playerSettlement.GetRandomStructureInRegion(p_character.currentRegion);
				if (randomStructureInRegion != null)
				{
					p_character.behaviourComponent.SetIsAttackingDemonicStructure(state: true, randomStructureInRegion as DemonicStructure);
					return true;
				}
			}
		}
		else
		{
			if (InvadeBehaviour(p_character, ref p_log, out p_producedJob))
			{
				return true;
			}
			if (GameUtilities.RollChance(10) && p_character.behaviourComponent.invadeVillageTarget.Count <= 0)
			{
				PopulateVillageTargetsByPriority(p_character.behaviourComponent.invadeVillageTarget, p_character);
			}
		}
		return TriggerRoamAroundTerritory(p_character, ref p_log, out p_producedJob);
	}

	private bool InvadeBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.behaviourComponent.invadeVillageTarget.Count > 0)
		{
			Area areaLocation = character.areaLocation;
			if (areaLocation != null && character.behaviourComponent.invadeVillageTarget.Contains(areaLocation))
			{
				List<Character> list = RuinarchListPool<Character>.Claim();
				PopulateTargetChoicesFor(list, character, character.behaviourComponent.invadeVillageTarget);
				if (list.Count > 0)
				{
					Character randomElement = CollectionUtilities.GetRandomElement(list);
					character.combatComponent.Fight(randomElement, "Hostility");
				}
				else
				{
					character.behaviourComponent.ResetInvadeVillageTarget();
				}
				RuinarchListPool<Character>.Release(list);
				producedJob = null;
				return true;
			}
			LocationGridTile randomElement2 = CollectionUtilities.GetRandomElement(CollectionUtilities.GetRandomElement(character.behaviourComponent.invadeVillageTarget).gridTileComponent.gridTiles);
			return character.jobComponent.CreateGoToSpecificTileJob(randomElement2, out producedJob);
		}
		producedJob = null;
		return false;
	}

	private void PopulateTargetChoicesFor(List<Character> p_targetChoices, Character p_invader, List<Area> p_areas)
	{
		for (int i = 0; i < p_areas.Count; i++)
		{
			p_areas[i].locationCharacterTracker.PopulateCharacterListInsideHexForVengefulGhostBehaviour(p_targetChoices, p_invader);
		}
	}

	private bool IsCharacterValidForInvade(Character character)
	{
		if (!character.isDead && !character.traitContainer.HasTrait("Hibernating", "Indestructible") && !character.isInLimbo && !character.isBeingSeized)
		{
			return character.carryComponent.IsNotBeingCarried();
		}
		return false;
	}

	private void PopulateVillageTargetsByPriority(List<Area> areas, Character p_invader)
	{
		if (p_invader.currentRegion == null)
		{
			return;
		}
		List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim(20);
		for (int i = 0; i < p_invader.currentRegion.settlementsInRegion.Count; i++)
		{
			BaseSettlement baseSettlement = p_invader.currentRegion.settlementsInRegion[i];
			if (baseSettlement.locationType != LOCATION_TYPE.DEMONIC_INTRUSION && baseSettlement.owner != null && baseSettlement.owner != p_invader.faction && p_invader.faction.IsHostileWith(baseSettlement.owner) && baseSettlement.residents.Count(IsCharacterValidForInvade) > 0)
			{
				list.Add(baseSettlement);
			}
		}
		if (list.Count > 0)
		{
			BaseSettlement baseSettlement2 = list[GameUtilities.RandomBetweenTwoNumbers(0, list.Count - 1)];
			areas.AddRange(baseSettlement2.areas);
		}
		RuinarchListPool<BaseSettlement>.Release(list);
	}
}
