using System.Collections.Generic;
using System.Linq;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

namespace Characters.Behaviour;

public class CriticalBreakBehaviour : CharacterBehaviour
{
	public CriticalBreakBehaviour()
	{
		base.priority = 1085;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (!character.moodComponent.isInCriticalBreak)
		{
			character.behaviourComponent.StopNonInstantCriticalBreak();
			producedJob = null;
			return false;
		}
		switch (character.moodComponent.currentCriticalBreak)
		{
		case CRITICAL_BREAK_ACTION.Destroy_Structure:
		{
			if (character.behaviourComponent.criticalBreakDestroyStructureTarget == null || character.behaviourComponent.criticalBreakFiresCreated >= 1)
			{
				character.behaviourComponent.StopArsonCriticalBreak();
				producedJob = null;
				return false;
			}
			ManMadeStructure p_structure = character.behaviourComponent.criticalBreakDestroyStructureTarget as ManMadeStructure;
			TileObject tileObjectToAttackToDestroyStructure = GetTileObjectToAttackToDestroyStructure(p_structure);
			if (tileObjectToAttackToDestroyStructure != null)
			{
				return character.jobComponent.TriggerArson(JOB_TYPE.CRITICAL_BREAK, tileObjectToAttackToDestroyStructure, out producedJob);
			}
			break;
		}
		case CRITICAL_BREAK_ACTION.Commit_Suicide:
			if (character.traitContainer.HasTrait("Paralyzed"))
			{
				character.behaviourComponent.EndCriticalBreakAbruptly();
				producedJob = null;
				return false;
			}
			if (character.jobComponent.TriggerSuicideJobForCriticalBreak(out producedJob, "Suicide_Reason_Mental_Break"))
			{
				return true;
			}
			break;
		case CRITICAL_BREAK_ACTION.Kill_Target:
			if (character.behaviourComponent.criticalBreakKillTarget == null || character.behaviourComponent.criticalBreakKillTarget.isDead)
			{
				character.behaviourComponent.EndCriticalBreakAbruptly();
				producedJob = null;
				return false;
			}
			character.combatComponent.Fight(character.behaviourComponent.criticalBreakKillTarget, "Critical_Break");
			producedJob = null;
			return true;
		}
		producedJob = null;
		return false;
	}

	private TileObject GetTileObjectToAttackToDestroyStructure(ManMadeStructure p_structure)
	{
		TileObject result = null;
		List<TileObject> list = RuinarchListPool<TileObject>.Claim();
		for (int i = 0; i < p_structure.tiles.Count; i++)
		{
			LocationGridTile locationGridTile = p_structure.tiles.ElementAt(i);
			if (!locationGridTile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Burning", "Fire Resistant") && locationGridTile.tileObjectComponent.genericTileObject.traitContainer.HasTrait("Flammable"))
			{
				list.Add(locationGridTile.tileObjectComponent.genericTileObject);
			}
			TileObject objHere = locationGridTile.tileObjectComponent.objHere;
			if (objHere != null && !objHere.traitContainer.HasTrait("Burning", "Fire Resistant") && objHere.traitContainer.HasTrait("Flammable"))
			{
				list.Add(objHere);
			}
		}
		if (list.Count > 0)
		{
			result = CollectionUtilities.GetRandomElement(list);
		}
		RuinarchListPool<TileObject>.Release(list);
		return result;
	}

	public override void OnRemoveBehaviourFromCharacter(Character character)
	{
		base.OnRemoveBehaviourFromCharacter(character);
		if (character.moodComponent.isInCriticalBreak)
		{
			character.behaviourComponent.EndCriticalBreakAbruptly();
		}
	}
}
