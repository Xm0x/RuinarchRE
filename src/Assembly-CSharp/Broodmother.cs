using System.Collections.Generic;
using Locations.Settlements;
using UtilityScripts;

public class Broodmother : SkinnableAnimal
{
	public const string ClassName = "Broodmother";

	public override bool defaultDigMode => true;

	public override TILE_OBJECT_TYPE produceableMaterial => TILE_OBJECT_TYPE.SPIDER_SILK;

	public Broodmother()
		: base(SUMMON_TYPE.Broodmother, "Broodmother", RACE.SPIDER, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public Broodmother(string className)
		: base(SUMMON_TYPE.Broodmother, className, RACE.SPIDER, Utilities.GetRandomGender())
	{
		base.traitContainer.AddTrait(this, "Poison Resistant");
	}

	public Broodmother(SaveDataSkinnableAnimal data)
		: base(data)
	{
	}

	public override void SubscribeToSignals()
	{
		if (!base.hasSubscribedToSignals)
		{
			base.SubscribeToSignals();
			Messenger.AddListener<bool>(SettingsSignals.ARACHNOPHOBIA_TOGGLED, OnArachnophobiaToggled);
		}
	}

	public override void UnsubscribeSignals()
	{
		if (base.hasSubscribedToSignals)
		{
			base.UnsubscribeSignals();
			Messenger.RemoveListener<bool>(SettingsSignals.ARACHNOPHOBIA_TOGGLED, OnArachnophobiaToggled);
		}
	}

	public override bool Agitate(ref JobQueueItem p_agitateJob)
	{
		if (base.limiterComponent.IsIncapacitated())
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Incapacitated);
			return false;
		}
		if (base.gridTileLocation != null && base.gridTileLocation.tileObjectComponent.objHere == null && base.jobComponent.TryTriggerLayEgg(this, 4, TILE_OBJECT_TYPE.SPIDER_EGG, out p_agitateJob) && p_agitateJob is GoapPlanJob goapPlanJob)
		{
			goapPlanJob.SetIsAgitateJob(p_state: true);
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
			return true;
		}
		bool flag = false;
		if (base.homeSettlement != null)
		{
			flag = base.homeSettlement.HasResidentThatIsAliveAndMonsterTypeIs(SUMMON_TYPE.Giant_Spider);
		}
		else if (base.homeStructure != null)
		{
			flag = base.homeStructure.HasResidentThatIsAliveAndMonsterTypeIs(SUMMON_TYPE.Giant_Spider);
		}
		else if (HasTerritory())
		{
			flag = base.homeRegion.HasAliveCharacterWithSameTerritoryAndMonsterTypeIs(this, SUMMON_TYPE.Giant_Spider);
		}
		if (!flag)
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.Special_2);
			return false;
		}
		List<Area> list = RuinarchListPool<Area>.Claim();
		if (base.homeSettlement != null)
		{
			list.AddRange(base.homeSettlement.areas);
		}
		else if (base.homeStructure != null)
		{
			if (base.homeStructure.occupiedAreas != null)
			{
				foreach (Area key in base.homeStructure.occupiedAreas.Keys)
				{
					list.Add(key);
				}
			}
			if (base.homeStructure.occupiedArea != null && !list.Contains(base.homeStructure.occupiedArea))
			{
				list.Add(base.homeStructure.occupiedArea);
			}
		}
		else if (HasTerritory())
		{
			list.Add(base.territory);
		}
		if (list.Count > 0)
		{
			BaseSettlement baseSettlement = null;
			List<BaseSettlement> list2 = RuinarchListPool<BaseSettlement>.Claim();
			bool flag2 = false;
			for (int i = 0; i < LandmarkManager.Instance.allNonPlayerSettlements.Count; i++)
			{
				BaseSettlement baseSettlement2 = LandmarkManager.Instance.allNonPlayerSettlements[i];
				if (baseSettlement2 == base.homeSettlement || baseSettlement2.locationType != LOCATION_TYPE.VILLAGE || !baseSettlement2.HasResidentThatIsAliveAndInsideSettlement())
				{
					continue;
				}
				flag2 = false;
				for (int j = 0; j < baseSettlement2.areas.Count; j++)
				{
					Area area = baseSettlement2.areas[j];
					for (int k = 0; k < list.Count; k++)
					{
						Area p_area = list[k];
						if (area.IsNearbyTo(p_area))
						{
							list2.Add(baseSettlement2);
							flag2 = true;
							break;
						}
					}
					if (flag2)
					{
						break;
					}
				}
			}
			RuinarchListPool<Area>.Release(list);
			if (list2.Count > 0)
			{
				baseSettlement = CollectionUtilities.GetRandomElement(list2);
			}
			RuinarchListPool<BaseSettlement>.Release(list2);
			if (baseSettlement != null)
			{
				base.jobComponent.TriggerBroodmotherOrderAttack(JOB_TYPE.AGITATED, baseSettlement, out p_agitateJob);
				if (p_agitateJob is GoapPlanJob goapPlanJob2)
				{
					goapPlanJob2.SetIsAgitateJob(p_state: true);
					CreateAgitateLog(AGITATE_MESSAGE_TYPE.Agitate_Success);
					return true;
				}
			}
			else
			{
				CreateAgitateLog(AGITATE_MESSAGE_TYPE.Broodmother_No_Target);
			}
		}
		else
		{
			CreateAgitateLog(AGITATE_MESSAGE_TYPE.No_Home);
			RuinarchListPool<Area>.Release(list);
		}
		return false;
	}

	private void OnArachnophobiaToggled(bool p_isOn)
	{
		base.visuals.UpdateAllVisuals(this);
	}
}
