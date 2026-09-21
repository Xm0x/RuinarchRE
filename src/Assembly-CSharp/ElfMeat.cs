using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class ElfMeat : FoodPile
{
	public override CONCRETE_RESOURCES specificProvidedResource => CONCRETE_RESOURCES.Elf_Meat;

	public ElfMeat()
		: base(TILE_OBJECT_TYPE.ELF_MEAT)
	{
		AddAdvertisedAction(INTERACTION_TYPE.DISPOSE_FOOD);
	}

	public ElfMeat(SaveDataTileObject saveDataTileObject)
		: base(saveDataTileObject)
	{
	}

	public override string ToString()
	{
		return "Elf Meat " + base.id;
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
			if (randomElement != null && CrimeManager.Instance.IsConsideredACrimeByCharacter(actor, randomElement, this, CRIME_TYPE.Cannibalism))
			{
				actor.assumptionComponent.CreateAndReactToNewAssumption(randomElement, this, INTERACTION_TYPE.IS_CANNIBAL, REACTION_STATUS.WITNESSED, !randomElement.traitContainer.HasTrait("Cannibal"));
			}
		}
		RuinarchListPool<Character>.Release(list);
	}
}
