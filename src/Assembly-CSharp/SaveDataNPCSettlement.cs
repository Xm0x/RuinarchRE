using System.Collections.Generic;
using Locations.Settlements;

public class SaveDataNPCSettlement : SaveDataBaseSettlement
{
	public string regionID;

	public List<string> jobIDs;

	public List<string> forceCancelJobIDs;

	public string prisonID;

	public string mainStorageID;

	public string rulerID;

	public SaveDataSettlementType settlementType;

	public SaveDataLocationEventManager eventManager;

	public List<TILE_OBJECT_TYPE> neededObjects;

	public bool hasTriedToStealCorpse;

	public bool isUnderSiege;

	public bool isPlagued;

	public GameDate plaguedExpiry;

	public bool hasPeasants;

	public bool hasWorkers;

	public bool hasOccupiedVillageSpot;

	public Point occupiedVillageSpot;

	public GameDate clearBlacklistScheduleDate;

	public bool hasClearBlacklistSchedule;

	public GameDate dateOfVillageCreation;

	public SaveDataSettlementVillageMigrationComponent migrationComponent;

	public SaveDataSettlementResourcesComponent resourcesComponent;

	public SaveDataSettlementClassComponent classComponent;

	public SaveDataSettlementPartyComponent partyComponent;

	public SaveDataSettlementStructureComponent structureComponent;

	public SaveDataSettlementExpirationComponent expirationComponent;

	public SaveDataSettlementTileObjectComponent tileObjectComponent;

	public SaveDataSettlementFactionIdeologyComponent factionIdeologyComponent;

	public override void Save(BaseSettlement baseSettlement)
	{
		base.Save(baseSettlement);
		NPCSettlement nPCSettlement = baseSettlement as NPCSettlement;
		hasTriedToStealCorpse = nPCSettlement.hasTriedToStealCorpse;
		regionID = nPCSettlement.region.persistentID;
		jobIDs = new List<string>();
		for (int i = 0; i < nPCSettlement.availableJobs.Count; i++)
		{
			JobQueueItem jobQueueItem = nPCSettlement.availableJobs[i];
			if (jobQueueItem.jobType != JOB_TYPE.NONE)
			{
				jobIDs.Add(jobQueueItem.persistentID);
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(jobQueueItem);
			}
		}
		forceCancelJobIDs = new List<string>();
		for (int j = 0; j < nPCSettlement.forcedCancelJobsOnTickEnded.Count; j++)
		{
			JobQueueItem jobQueueItem2 = nPCSettlement.forcedCancelJobsOnTickEnded[j];
			if (jobQueueItem2.jobType != JOB_TYPE.NONE)
			{
				forceCancelJobIDs.Add(jobQueueItem2.persistentID);
			}
		}
		rulerID = nPCSettlement.ruler?.persistentID ?? string.Empty;
		prisonID = ((nPCSettlement.prison != null) ? nPCSettlement.prison.persistentID : string.Empty);
		mainStorageID = ((nPCSettlement.mainStorage != null) ? nPCSettlement.mainStorage.persistentID : string.Empty);
		if (nPCSettlement.settlementType != null)
		{
			settlementType = new SaveDataSettlementType();
			settlementType.Save(nPCSettlement.settlementType);
		}
		eventManager = new SaveDataLocationEventManager();
		eventManager.Save(nPCSettlement.eventManager);
		neededObjects = new List<TILE_OBJECT_TYPE>(nPCSettlement.neededObjects);
		isUnderSiege = nPCSettlement.isUnderSiege;
		isPlagued = nPCSettlement.isPlagued;
		if (isPlagued)
		{
			plaguedExpiry = nPCSettlement.plaguedExpiryDate;
		}
		hasPeasants = nPCSettlement.hasPeasants;
		hasWorkers = nPCSettlement.hasWorkers;
		migrationComponent = new SaveDataSettlementVillageMigrationComponent();
		migrationComponent.Save(nPCSettlement.migrationComponent);
		resourcesComponent = new SaveDataSettlementResourcesComponent();
		resourcesComponent.Save(nPCSettlement.resourcesComponent);
		classComponent = new SaveDataSettlementClassComponent();
		classComponent.Save(nPCSettlement.classComponent);
		partyComponent = new SaveDataSettlementPartyComponent();
		partyComponent.Save(nPCSettlement.partyComponent);
		structureComponent = new SaveDataSettlementStructureComponent();
		structureComponent.Save(nPCSettlement.structureComponent);
		expirationComponent = new SaveDataSettlementExpirationComponent();
		expirationComponent.Save(nPCSettlement.expirationComponent);
		tileObjectComponent = new SaveDataSettlementTileObjectComponent();
		tileObjectComponent.Save(nPCSettlement.tileObjectComponent);
		factionIdeologyComponent = new SaveDataSettlementFactionIdeologyComponent();
		factionIdeologyComponent.Save(nPCSettlement.factionIdeologyComponent);
		hasOccupiedVillageSpot = nPCSettlement.occupiedVillageSpot != null;
		if (nPCSettlement.occupiedVillageSpot != null)
		{
			occupiedVillageSpot = new Point(nPCSettlement.occupiedVillageSpot.coreSpot.areaData.xCoordinate, nPCSettlement.occupiedVillageSpot.coreSpot.areaData.yCoordinate);
		}
		clearBlacklistScheduleDate = nPCSettlement.clearBlacklistScheduleDate;
		hasClearBlacklistSchedule = nPCSettlement.hasClearBlacklistSchedule;
		dateOfVillageCreation = nPCSettlement.dateVillageWasLastClaimed;
	}

	public override BaseSettlement Load()
	{
		return LandmarkManager.Instance.LoadNPCSettlement(this);
	}
}
