public class LunchPack : TileObject
{
	public LunchPack()
	{
		Initialize(TILE_OBJECT_TYPE.LUNCH_PACK);
		base.traitContainer.AddTrait(this, "Edible");
		AddAdvertisedAction(INTERACTION_TYPE.PICK_UP);
	}

	public LunchPack(SaveDataTileObject data)
		: base(data)
	{
	}

	public override void SetInventoryOwner(Character p_newOwner)
	{
		base.SetInventoryOwner(p_newOwner);
		if (p_newOwner != null)
		{
			AddAdvertisedAction(INTERACTION_TYPE.EAT_INVENTORY_ITEM);
		}
		else
		{
			RemoveAdvertisedAction(INTERACTION_TYPE.EAT_INVENTORY_ITEM);
		}
	}

	public override void VillagerReactionToTileObject(Character actor, ref string debugLog)
	{
		base.VillagerReactionToTileObject(actor, ref debugLog);
		if (actor.characterClass.IsCombatant() && !actor.HasItem(TILE_OBJECT_TYPE.LUNCH_PACK) && actor.needsComponent.HasNeeds() && !actor.traitContainer.HasTrait("Vampire"))
		{
			bool flag = IsOwnedBy(actor);
			if (!flag && (base.characterOwner == null || (gridTileLocation != null && gridTileLocation.structure.residents.Count <= 0)))
			{
				flag = true;
			}
			if (flag && !actor.jobComponent.HasHigherPriorityJobThan(JOB_TYPE.STOCKPILE_FOOD) && !actor.jobQueue.HasJob(JOB_TYPE.STOCKPILE_FOOD))
			{
				actor.jobComponent.CreateTakeItemOnSightJob(this, JOB_TYPE.STOCKPILE_FOOD);
			}
		}
	}
}
