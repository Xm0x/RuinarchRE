using Inner_Maps;
using Inner_Maps.Location_Structures;

namespace Traits;

public class Impregnated : Status
{
	public Impregnated()
	{
		name = "Impregnated";
		description = "Impregnated";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		isStacking = false;
	}

	public override void OnRemoveStatusBySchedule(ITraitable removedFrom)
	{
		base.OnRemoveStatusBySchedule(removedFrom);
		if (removedFrom is Character p_target)
		{
			KillAndSpawnGoblins(p_target);
		}
	}

	private void KillAndSpawnGoblins(Character p_target)
	{
		LocationGridTile gridTileLocation = p_target.gridTileLocation;
		if (gridTileLocation == null)
		{
			return;
		}
		LocationStructure homeStructure = null;
		if (gridTileLocation.structure.structureType.IsSpecialStructure())
		{
			if (gridTileLocation.structure is Cave cave && !cave.HasConnectedMines())
			{
				homeStructure = cave;
			}
			else if (gridTileLocation.structure.settlementLocation == null || gridTileLocation.structure.settlementLocation.owner == null)
			{
				homeStructure = gridTileLocation.structure;
			}
		}
		for (int i = 0; i < 2; i++)
		{
			Summon summon = CharacterManager.Instance.CreateNewSummon(SUMMON_TYPE.Goblin, FactionManager.Instance.GetDefaultFactionForMonster(SUMMON_TYPE.Goblin), null, gridTileLocation.parentMap.region, homeStructure, "", bypassIdeologyChecking: true);
			CharacterManager.Instance.PlaceSummonInitially(summon, gridTileLocation);
			GameManager.Instance.CreateParticleEffectAt(summon, PARTICLE_EFFECT.Spawn_Effect);
		}
		p_target.Death();
	}
}
