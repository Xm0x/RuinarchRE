using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class HuntPreyBehaviour : CharacterBehaviour
{
	public HuntPreyBehaviour()
	{
		base.priority = 10;
	}

	public override bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob)
	{
		producedJob = null;
		Hunting traitOrStatus = character.traitContainer.GetTraitOrStatus<Hunting>("Hunting");
		if (traitOrStatus != null)
		{
			List<Character> list = RuinarchListPool<Character>.Claim();
			traitOrStatus.targetArea.locationCharacterTracker.PopulateAnimalsListThatCharacterCanReachInsideHexThatIsNotTheSameRaceAs(character, list, character.race);
			if (list.Count > 0)
			{
				List<Character> list2 = RuinarchListPool<Character>.Claim();
				for (int i = 0; i < list.Count; i++)
				{
					Character character2 = list[i];
					if (character2.isDead && character2.hasMarker && character2.gridTileLocation != null)
					{
						list2.Add(character2);
					}
				}
				if (list2.Count > 0)
				{
					Character randomElement = CollectionUtilities.GetRandomElement(list2);
					GoapPlanJob goapPlanJob = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_PREY, INTERACTION_TYPE.EAT_CORPSE, randomElement, character);
					producedJob = goapPlanJob;
				}
				else
				{
					Character randomElement2 = CollectionUtilities.GetRandomElement(list);
					GoapPlanJob goapPlanJob2 = JobManager.Instance.CreateNewGoapPlanJob(JOB_TYPE.HUNT_PREY, INTERACTION_TYPE.ASSAULT, randomElement2, character);
					producedJob = goapPlanJob2;
				}
				RuinarchListPool<Character>.Release(list2);
			}
			else
			{
				character.traitContainer.RemoveTrait(character, "Hunting");
			}
			RuinarchListPool<Character>.Release(list);
		}
		return true;
	}
}
