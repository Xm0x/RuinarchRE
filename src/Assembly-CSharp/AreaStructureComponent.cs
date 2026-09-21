using System.Collections.Generic;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class AreaStructureComponent : AreaComponent
{
	private AutoDestroyParticle _buildParticles;

	public List<LocationStructure> structures { get; private set; }

	public List<StructureConnector> structureConnectors { get; private set; }

	public AreaStructureComponent()
	{
		structures = new List<LocationStructure>();
		structureConnectors = new List<StructureConnector>();
	}

	public bool AddStructureInArea(LocationStructure p_structure)
	{
		if (!structures.Contains(p_structure))
		{
			structures.Add(p_structure);
			return true;
		}
		return false;
	}

	public bool RemoveStructureInArea(LocationStructure p_structure)
	{
		return structures.Remove(p_structure);
	}

	public bool HasStructureInArea()
	{
		return structures.Count > 0;
	}

	public bool HasStructureInArea(STRUCTURE_TYPE p_structureType)
	{
		for (int i = 0; i < structures.Count; i++)
		{
			if (structures[i].structureType == p_structureType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasStructureInArea(List<STRUCTURE_TYPE> p_structureTypes)
	{
		for (int i = 0; i < structures.Count; i++)
		{
			LocationStructure locationStructure = structures[i];
			if (p_structureTypes.Contains(locationStructure.structureType))
			{
				return true;
			}
		}
		return false;
	}

	public LocationStructure GetMostImportantStructureOnTile()
	{
		LocationStructure locationStructure = null;
		for (int i = 0; i < structures.Count; i++)
		{
			LocationStructure locationStructure2 = structures[i];
			if (locationStructure2.HasTileOnArea(base.owner))
			{
				int num = locationStructure2.structureType.StructurePriority();
				if (locationStructure == null || num > locationStructure.structureType.StructurePriority())
				{
					locationStructure = locationStructure2;
				}
			}
		}
		if (locationStructure == null)
		{
			locationStructure = base.owner.region.wilderness;
		}
		return locationStructure;
	}

	public bool CanBuildNormalStructureHere(STRUCTURE_TYPE structureType, out string o_cannotBuildReason)
	{
		if (structureType == STRUCTURE_TYPE.LICH_GRAVEYARD && GridMap.Instance.mainRegion.HasStructure(STRUCTURE_TYPE.LICH_GRAVEYARD))
		{
			o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_one_lich_graveyard");
			return false;
		}
		o_cannotBuildReason = string.Empty;
		return true;
	}

	public bool CanBuildDemonicStructureHere(STRUCTURE_TYPE structureType, out string o_cannotBuildReason)
	{
		if (InnerMapManager.Instance.currentlyShowingLocation == null && structureType != STRUCTURE_TYPE.THE_PORTAL)
		{
			o_cannotBuildReason = string.Empty;
			return false;
		}
		switch (structureType)
		{
		case STRUCTURE_TYPE.THE_PORTAL:
			if (CanBuildDemonicStructureHere(out o_cannotBuildReason))
			{
				if (base.owner.HasSettlementLocationType(LOCATION_TYPE.VILLAGE))
				{
					o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_portal_village");
					return false;
				}
				return true;
			}
			return false;
		case STRUCTURE_TYPE.SPIRE:
			if (CanBuildDemonicStructureHere(out o_cannotBuildReason))
			{
				if (InnerMapManager.Instance.currentlyShowingLocation != null && InnerMapManager.Instance.currentlyShowingLocation.HasStructure(STRUCTURE_TYPE.SPIRE))
				{
					o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_one_SPIRE");
					return false;
				}
				return true;
			}
			return false;
		case STRUCTURE_TYPE.PRIMORDIAL_POOL:
			if (CanBuildDemonicStructureHere(out o_cannotBuildReason))
			{
				if (InnerMapManager.Instance.currentlyShowingLocation != null && InnerMapManager.Instance.currentlyShowingLocation.HasStructure(STRUCTURE_TYPE.PRIMORDIAL_POOL))
				{
					o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_one_PRIMORDIAL_POOL");
					return false;
				}
				return true;
			}
			return false;
		case STRUCTURE_TYPE.MEDDLER:
			if (CanBuildDemonicStructureHere(out o_cannotBuildReason))
			{
				if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.MEDDLER))
				{
					o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_one_meddler");
					return false;
				}
				return true;
			}
			return false;
		case STRUCTURE_TYPE.BIOLAB:
			if (CanBuildDemonicStructureHere(out o_cannotBuildReason))
			{
				if (PlayerManager.Instance.player.playerSettlement.HasStructure(STRUCTURE_TYPE.BIOLAB))
				{
					o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_one_biolab");
					return false;
				}
				return true;
			}
			return false;
		default:
			return CanBuildDemonicStructureHere(out o_cannotBuildReason);
		}
	}

	private bool CanBuildDemonicStructureHere(out string o_cannotBuildReason)
	{
		if (PlayerManager.Instance.player != null && PlayerManager.Instance.player.isCurrentlyBuildingDemonicStructure)
		{
			o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_currently_building");
			return false;
		}
		if (_buildParticles != null)
		{
			o_cannotBuildReason = LocalizationManager.Instance.GetLocalizedValue("LocationAlerts_Table", "invalid_build_currently_building");
			return false;
		}
		o_cannotBuildReason = string.Empty;
		return true;
	}

	public void AddStructureConnector(StructureConnector p_connector)
	{
		if (!structureConnectors.Contains(p_connector))
		{
			structureConnectors.Add(p_connector);
		}
	}

	public void RemoveStructureConnector(StructureConnector p_connector)
	{
		structureConnectors.Remove(p_connector);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		structures.Contains(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}
}
