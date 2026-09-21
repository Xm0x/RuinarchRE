using System.Collections.Generic;
using Inner_Maps;
using UtilityScripts;

public class AbominationBehaviour : BaseMonsterBehaviour
{
	public AbominationBehaviour()
	{
		base.priority = 10;
	}

	protected override bool WildBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		if (character.behaviourComponent.abominationTarget == null)
		{
			List<Area> list = RuinarchListPool<Area>.Claim(10);
			PopulateTargetChoices(list, character);
			if (list != null)
			{
				Area randomElement = CollectionUtilities.GetRandomElement(list);
				character.behaviourComponent.SetAbominationTarget(randomElement);
			}
			RuinarchListPool<Area>.Release(list);
		}
		if (character.behaviourComponent.abominationTarget == null)
		{
			return character.jobComponent.TriggerRoamAroundTile(out producedJob);
		}
		LocationGridTile randomPassableTile = character.behaviourComponent.abominationTarget.gridTileComponent.GetRandomPassableTile();
		return character.jobComponent.TriggerRoamAroundTile(out producedJob, randomPassableTile);
	}

	private void PopulateTargetChoices(List<Area> areas, Character actor)
	{
		if (actor.areaLocation == null)
		{
			return;
		}
		for (int i = 0; i < actor.areaLocation.neighbourComponent.neighbours.Count; i++)
		{
			Area area = actor.areaLocation.neighbourComponent.neighbours[i];
			if (area.elevationType != ELEVATION.WATER && area.region == actor.currentRegion && actor.movementComponent.HasPathTo(area))
			{
				areas.Add(area);
			}
		}
	}
}
