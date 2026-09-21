using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class CultistKit : TileObject
{
	public CultistKit()
	{
		Initialize(TILE_OBJECT_TYPE.CULTIST_KIT);
	}

	public CultistKit(SaveDataTileObject data)
		: base(data)
	{
	}

	public override string ToString()
	{
		return "Cultist Kit " + base.id;
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (IsOwnedBy(actor) || gridTileLocation == null)
		{
			return;
		}
		List<Character> list = RuinarchListPool<Character>.Claim();
		if (base.structureLocation is ManMadeStructure && base.structureLocation.GetNumberOfResidentsAndPopulateListExcluding(list, actor) > 0)
		{
			actor.reactionComponent.assumptionSuspects.Clear();
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					Character character = list[i];
					if (actor.relationshipContainer.GetAwarenessState(actor, character) == AWARENESS_STATE.Available && !character.isDead)
					{
						actor.reactionComponent.assumptionSuspects.Add(character);
					}
				}
			}
			Character randomElement = CollectionUtilities.GetRandomElement(actor.reactionComponent.assumptionSuspects);
			if (randomElement != null && CrimeManager.Instance.IsConsideredACrimeByCharacter(actor, randomElement, this, CRIME_TYPE.Demon_Worship))
			{
				actor.assumptionComponent.CreateAndReactToNewAssumption(randomElement, this, INTERACTION_TYPE.IS_CULTIST, REACTION_STATUS.WITNESSED, !randomElement.traitContainer.IsReligiousCultist(RELIGION.Demon_Worship));
			}
		}
		RuinarchListPool<Character>.Release(list);
	}
}
