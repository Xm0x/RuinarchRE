using System;
using System.Collections.Generic;
using Locations.Settlements;
using UnityEngine;

namespace Inner_Maps.Location_Structures;

public class HallowedGround : NaturalStructureWithStructureObject
{
	public RELIGION claimedByReligion { get; private set; }

	public List<Area> nearAreas { get; private set; }

	public override bool shouldBeLoadedOnMainThread => true;

	public override Type serializedData => typeof(SaveDataHallowedGround);

	public HallowedGround(Region location)
		: base(STRUCTURE_TYPE.HALLOWED_GROUND, location)
	{
		claimedByReligion = RELIGION.None;
		nearAreas = new List<Area>();
		SetMaxHPAndReset(6000);
	}

	public HallowedGround(Region location, SaveDataNaturalStructureWithStructureObject data)
		: base(location, data)
	{
		nearAreas = new List<Area>();
		SetMaxHP(6000);
		SaveDataHallowedGround saveDataHallowedGround = data as SaveDataHallowedGround;
		claimedByReligion = saveDataHallowedGround.claimedByReligion;
	}

	public override void LoadStructureSecondWaveInMainThread(SaveDataLocationStructure saveDataLocationStructure)
	{
		base.LoadStructureSecondWaveInMainThread(saveDataLocationStructure);
		UpdateStructureObjectVisual();
	}

	public override void SetStructureObject(LocationStructureObject structureObj)
	{
		base.SetStructureObject(structureObj);
		UpdateStructureObjectVisual();
	}

	public override void OnBuiltNewStructure()
	{
		base.OnBuiltNewStructure();
		base.occupiedArea.PopulateAreasInRange(nearAreas, 6, includeCenterTile: true);
	}

	public override void OnDoneLoadStructure()
	{
		base.OnDoneLoadStructure();
		base.occupiedArea.PopulateAreasInRange(nearAreas, 6, includeCenterTile: true);
	}

	public bool IsNearHallowedGroundForPilgrimage(Character p_character)
	{
		if (p_character.homeSettlement != null && IsNearHallowedGrounds(p_character.homeSettlement))
		{
			return true;
		}
		return IsNearHallowedGrounds(p_character.areaLocation);
	}

	public bool IsNearHallowedGrounds(BaseSettlement p_settlement)
	{
		for (int i = 0; i < p_settlement.areas.Count; i++)
		{
			Area p_area = p_settlement.areas[i];
			if (IsNearHallowedGrounds(p_area))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsNearHallowedGrounds(Area p_area)
	{
		return nearAreas.Contains(p_area);
	}

	public override string GetTestingInfo()
	{
		return base.GetTestingInfo() + "Near Areas: " + nearAreas.ComafyList();
	}

	public void ClaimHallowedGround(RELIGION p_religion)
	{
		claimedByReligion = p_religion;
		Messenger.Broadcast(StructureSignals.HALLOWED_GROUND_CLAIMED, p_religion);
		UpdateStructureObjectVisual();
	}

	private void UpdateStructureObjectVisual()
	{
		if (base.structureObj is HallowedGroundStructureObject hallowedGroundStructureObject)
		{
			Color colorToUse = GetColorToUse();
			hallowedGroundStructureObject.SetParticlesColor(colorToUse);
			hallowedGroundStructureObject.UpdateGroundVisualBasedOnReligion(claimedByReligion);
		}
	}

	private Color GetColorToUse()
	{
		return claimedByReligion switch
		{
			RELIGION.None => Color.white, 
			RELIGION.Demon_Worship => new Color(0.6784314f, 0.09019608f, 1f), 
			RELIGION.Divine_Worship => new Color(1f, 47f / 51f, 0.4392157f), 
			RELIGION.Nature_Worship => new Color(0.5019608f, 47f / 51f, 0.09019608f), 
			_ => throw new ArgumentOutOfRangeException("claimedByReligion", claimedByReligion, null), 
		};
	}

	public override void CleanUp()
	{
		if (DatabaseManager.Instance.structureDatabase.HasStructure(base.persistentID))
		{
			nearAreas.Clear();
			base.CleanUp();
		}
	}
}
