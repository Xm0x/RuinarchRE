using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Locations.Settlements;
using UtilityScripts;

namespace Interrupts;

public class SetHomeRatman : Interrupt
{
	public SetHomeRatman()
		: base(INTERRUPT.Set_Home_Ratman)
	{
		base.duration = 0;
		base.isSimulateneous = true;
		base.interruptIconString = GoapActionStateDB.No_Icon;
		base.logTags = new LOG_TAG[1];
	}

	public override bool ExecuteInterruptStartEffect(InterruptHolder interruptHolder, ref Log overrideEffectLog, ActualGoapNode goapNode = null)
	{
		Character actor = interruptHolder.actor;
		SetNewHomeStructure(actor);
		if (actor.homeStructure != null)
		{
			overrideEffectLog = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Interrupt", "Interrupts_Table", base.name + " set_new_home", base.logTags);
			overrideEffectLog.AddToFillers(actor, actor.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
			overrideEffectLog.AddToFillers(null, actor.homeStructure.name, LOG_IDENTIFIER.STRING_1);
		}
		return true;
	}

	private void SetNewHomeStructure(Character actor)
	{
		Region currentRegion = actor.currentRegion;
		if (currentRegion != null)
		{
			List<BaseSettlement> list = RuinarchListPool<BaseSettlement>.Claim(20);
			PopulateSettlementChoices(list, currentRegion);
			if (list.Count > 0)
			{
				BaseSettlement randomElement = CollectionUtilities.GetRandomElement(list);
				LocationStructure homeStructure = null;
				if (randomElement is NPCSettlement nPCSettlement && randomElement.locationType == LOCATION_TYPE.DUNGEON)
				{
					homeStructure = nPCSettlement.mainStorage;
				}
				actor.ClearTerritoryAndMigrateHomeSettlementTo(randomElement, homeStructure);
			}
			RuinarchListPool<BaseSettlement>.Release(list);
		}
		if (actor.homeStructure != null && actor.homeStructure.hasBeenDestroyed)
		{
			actor.MigrateHomeStructureTo(null, broadcast: true, addToRegionResidents: true, affectSettlement: false);
		}
	}

	private void PopulateSettlementChoices(List<BaseSettlement> settlementChoices, Region region)
	{
		for (int i = 0; i < region.allStructures.Count; i++)
		{
			LocationStructure locationStructure = region.allStructures[i];
			if (locationStructure is Cave { hasConnectedMine: not false } cave)
			{
				bool flag = false;
				for (int j = 0; j < cave.connectedMines.Count; j++)
				{
					LocationStructure locationStructure2 = cave.connectedMines[j];
					if (locationStructure2.settlementLocation != null && locationStructure2.settlementLocation.HasResidents())
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					continue;
				}
			}
			if (locationStructure.settlementLocation != null && locationStructure.settlementLocation.owner == null && locationStructure.settlementLocation.residents.Count <= 0 && (locationStructure.structureType == STRUCTURE_TYPE.CAVE || locationStructure.structureType == STRUCTURE_TYPE.MONSTER_LAIR || locationStructure.settlementLocation.locationType == LOCATION_TYPE.VILLAGE) && !settlementChoices.Contains(locationStructure.settlementLocation))
			{
				settlementChoices.Add(locationStructure.settlementLocation);
			}
		}
	}
}
