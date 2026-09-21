using System;
using System.Collections.Generic;
using Locations.Settlements;
using UtilityScripts;

[Serializable]
public class Entkin : FactionIdeology
{
	public Entkin()
		: base(FACTION_IDEOLOGY.Entkin)
	{
		base.daysIntervalSettlementEvent = 0;
	}

	public override bool DoesCharacterFitIdeology(Character character)
	{
		return true;
	}

	public override bool DoesCharacterFitIdeology(PreCharacterData character)
	{
		return true;
	}

	protected override void SettlementEvent(BaseSettlement p_settlement)
	{
		base.SettlementEvent(p_settlement);
		if (CanSpawnNewEnt(p_settlement))
		{
			BigTreeObject randomTreeInsideHomeOrSurroundingVillage = GetRandomTreeInsideHomeOrSurroundingVillage(p_settlement);
			if (randomTreeInsideHomeOrSurroundingVillage != null && !randomTreeInsideHomeOrSurroundingVillage.TryAwakenEnt() && randomTreeInsideHomeOrSurroundingVillage.gridTileLocation != null && randomTreeInsideHomeOrSurroundingVillage.SpawnEnt(p_settlement.owner, p_settlement) != null)
			{
				Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Ideology", "FactionIdeologies_Table", "Entkin_Spawn_Ent", LOG_TAG.Work, null);
				log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
				log.AddLogToDatabase();
				PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
			}
		}
	}

	private bool CanSpawnNewEnt(BaseSettlement p_settlement)
	{
		GetNumberOfAliveEntAndSapientResidents(p_settlement, out var p_sapientCount, out var p_entCount);
		int num = p_sapientCount / 4;
		if (p_entCount <= num)
		{
			return true;
		}
		return false;
	}

	private void GetNumberOfAliveEntAndSapientResidents(BaseSettlement p_settlement, out int p_sapientCount, out int p_entCount)
	{
		p_sapientCount = 0;
		p_entCount = 0;
		for (int i = 0; i < p_settlement.residents.Count; i++)
		{
			Character character = p_settlement.residents[i];
			if (!character.isDead)
			{
				if (character.race.IsSapient())
				{
					p_sapientCount++;
				}
				else if (character.race == RACE.ENT)
				{
					p_entCount++;
				}
			}
		}
	}

	private BigTreeObject GetRandomTreeInsideHomeOrSurroundingVillage(BaseSettlement p_settlement)
	{
		BigTreeObject bigTreeObject = null;
		if (p_settlement.areas != null && p_settlement.areas.Count > 0)
		{
			List<Area> list = RuinarchListPool<Area>.Claim();
			for (int i = 0; i < p_settlement.areas.Count; i++)
			{
				list.Add(p_settlement.areas[i]);
			}
			bigTreeObject = GetFirstTreeInsideRandomAreas(list);
			RuinarchListPool<Area>.Release(list);
			if (bigTreeObject != null)
			{
				return bigTreeObject;
			}
		}
		List<Area> list2 = RuinarchListPool<Area>.Claim();
		p_settlement.PopulateSurroundingAreas(list2);
		bigTreeObject = GetFirstTreeInsideRandomAreas(list2);
		RuinarchListPool<Area>.Release(list2);
		return null;
	}

	private BigTreeObject GetFirstTreeInsideRandomAreas(List<Area> p_areas)
	{
		if (p_areas != null && p_areas.Count > 0)
		{
			BigTreeObject bigTreeObject = null;
			while (bigTreeObject == null && p_areas.Count > 0)
			{
				int index = GameUtilities.RandomBetweenTwoNumbers(0, p_areas.Count - 1);
				bigTreeObject = p_areas[index].tileObjectComponent.GetFirstTileObject<BigTreeObject>();
				if (bigTreeObject == null)
				{
					p_areas.RemoveAt(index);
				}
			}
			if (bigTreeObject != null)
			{
				return bigTreeObject;
			}
		}
		return null;
	}
}
