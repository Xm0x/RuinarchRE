using System.Collections.Generic;
using UtilityScripts;

namespace Inner_Maps.Location_Structures;

public class Tavern : ManMadeStructure
{
	public Tavern(Region location)
		: base(STRUCTURE_TYPE.TAVERN, location)
	{
		SetMaxHPAndReset(8000);
	}

	public Tavern(Region location, SaveDataManMadeStructure data)
		: base(location, data)
	{
		SetMaxHP(8000);
	}

	protected override void ProcessWorkStructureJobsByWorker(Character p_worker, out JobQueueItem producedJob)
	{
		producedJob = null;
		string log = string.Empty;
		if (GameUtilities.RollChance(15, ref log))
		{
			List<TileObject> tileObjectsOfType = GetTileObjectsOfType(TILE_OBJECT_TYPE.TABLE);
			Table table = null;
			if (tileObjectsOfType != null && tileObjectsOfType.Count > 0)
			{
				for (int i = 0; i < tileObjectsOfType.Count; i++)
				{
					if (tileObjectsOfType[i] is Table { food: <=0 } table2)
					{
						table = table2;
						break;
					}
				}
			}
			if (table != null)
			{
				p_worker.jobComponent.TryCreateBuyFoodForTavernTable(table, out producedJob);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		List<TileObject> tileObjectsOfType2 = GetTileObjectsOfType(TILE_OBJECT_TYPE.HERB_PLANT);
		if (tileObjectsOfType2 == null || tileObjectsOfType2.Count < 3)
		{
			HerbPlant firstAvailableHerbPlant = p_worker.homeSettlement.SettlementResources.GetFirstAvailableHerbPlant(p_worker.homeSettlement, p_worker);
			if (firstAvailableHerbPlant != null)
			{
				p_worker.jobComponent.TriggerGatherHerb(firstAvailableHerbPlant, out producedJob);
				if (producedJob != null)
				{
					return;
				}
			}
		}
		List<TileObject> tileObjectsOfType3 = GetTileObjectsOfType(TILE_OBJECT_TYPE.HEALING_POTION);
		if ((tileObjectsOfType3 == null || tileObjectsOfType3.Count <= 0) && tileObjectsOfType2 != null && tileObjectsOfType2.Count >= 1)
		{
			p_worker.jobComponent.TriggerCraftWorkplacePotion(tileObjectsOfType2[0], out producedJob);
			if (producedJob != null)
			{
				return;
			}
		}
		int chance = 100;
		if (p_worker.traitContainer.HasTrait("Lazy"))
		{
			chance = 6;
		}
		if (GameUtilities.RollChance(chance, ref log))
		{
			p_worker.jobComponent.TryCreateCleanItemJob(this, out producedJob);
			_ = producedJob;
		}
	}

	public override bool CanHireAWorker()
	{
		return true;
	}

	public override bool CanPurchaseFromHere(Character p_buyer, out bool needsToPay, out int buyerOpinionOfWorker)
	{
		return DefaultCanPurchaseFromHereForSingleWorkerStructures(p_buyer, out needsToPay, out buyerOpinionOfWorker);
	}
}
