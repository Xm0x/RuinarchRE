using System;
using System.Collections.Generic;
using UnityEngine.Serialization;

[Serializable]
public class SaveDataPlayerGame : SaveData<Player>
{
	public string factionID;

	public string settlementID;

	public int portalTileXCoordinate;

	public int portalTileYCoordinate;

	public PLAYER_ARCHETYPE archetype;

	public bool hasAlreadyWon;

	public string seizedPOIID;

	public POINT_OF_INTEREST_TYPE seizedPOIType;

	public List<SaveDataActionIntel> actionIntels;

	public List<SaveDataInterruptIntel> interruptIntels;

	public List<SaveDataNotification> allNotifs;

	public List<SaveDataChaosOrb> allChaosOrbs;

	public Dictionary<SUMMON_TYPE, MonsterCapacity> monsterCharges;

	public List<string> charactersThatHaveReportedDemonicStructure;

	public SaveDataSeizeComponent seizeComponent;

	public SaveDataThreatComponent threatComponent;

	public SaveDataPlayerSkillComponent playerSkillComponent;

	[FormerlySerializedAs("plagueComponent")]
	public SaveDataCurrenciesComponent currenciesComponent;

	public SaveDataPlayerUnderlingsComponent underlingsComponent;

	public SaveDataPlayerTileObjectComponent tileObjectComponent;

	public SaveDataStoredTargetsComponent storedTargetsComponent;

	public SaveDataBookmarkComponent bookmarkComponent;

	public SaveDataSummonMeterComponent summonMeterComponent;

	public SaveDataPlayerDamageAccumulator damageAccumulator;

	public SaveDataPlayerRetaliationComponent retaliationComponent;

	public SaveDataPlayerDevastationComponent devastationComponent;

	public SaveDataPrimordialPoolDataHandler primordialPoolDataHandler;

	public SaveDatapartyStructureDataHandler partyStructureDataHandler;

	public SaveDataManaRegenComponent manaRegenComponent;

	public SaveDataGoalComponent goalComponent;

	public override void Save()
	{
		base.Save();
		Player player = PlayerManager.Instance.player;
		factionID = player.playerFaction.persistentID;
		settlementID = player.playerSettlement.persistentID;
		portalTileXCoordinate = player.portalArea.areaData.xCoordinate;
		portalTileYCoordinate = player.portalArea.areaData.yCoordinate;
		hasAlreadyWon = player.hasAlreadyWon;
		archetype = PlayerSkillManager.Instance.selectedArchetype;
		if (player.seizeComponent.hasSeizedPOI)
		{
			seizedPOIID = player.seizeComponent.seizedPOI.persistentID;
			seizedPOIType = player.seizeComponent.seizedPOI.poiType;
		}
		actionIntels = new List<SaveDataActionIntel>();
		interruptIntels = new List<SaveDataInterruptIntel>();
		for (int i = 0; i < player.allIntel.Count; i++)
		{
			IIntel intel = player.allIntel[i];
			if (intel is ActionIntel data)
			{
				SaveDataActionIntel saveDataActionIntel = new SaveDataActionIntel();
				saveDataActionIntel.Save(data);
				actionIntels.Add(saveDataActionIntel);
			}
			else if (intel is InterruptIntel data2)
			{
				SaveDataInterruptIntel saveDataInterruptIntel = new SaveDataInterruptIntel();
				saveDataInterruptIntel.Save(data2);
				interruptIntels.Add(saveDataInterruptIntel);
			}
		}
		charactersThatHaveReportedDemonicStructure = player.charactersThatHaveReportedDemonicStructure;
		allNotifs = new List<SaveDataNotification>();
		for (int j = 0; j < UIManager.Instance.activeNotifications.Count; j++)
		{
			PlayerNotificationItem notif = UIManager.Instance.activeNotifications[j];
			SaveDataNotification saveDataNotification = new SaveDataNotification();
			saveDataNotification.Save(notif);
			allNotifs.Add(saveDataNotification);
		}
		allChaosOrbs = new List<SaveDataChaosOrb>();
		for (int k = 0; k < PlayerManager.Instance.availableChaosOrbs.Count; k++)
		{
			ChaosOrb chaosOrb = PlayerManager.Instance.availableChaosOrbs[k];
			if (chaosOrb.location != null)
			{
				SaveDataChaosOrb saveDataChaosOrb = new SaveDataChaosOrb();
				saveDataChaosOrb.Save(chaosOrb);
				allChaosOrbs.Add(saveDataChaosOrb);
			}
		}
		seizeComponent = new SaveDataSeizeComponent();
		seizeComponent.Save(player.seizeComponent);
		threatComponent = new SaveDataThreatComponent();
		threatComponent.Save(player.threatComponent);
		playerSkillComponent = new SaveDataPlayerSkillComponent();
		playerSkillComponent.Save(player.playerSkillComponent);
		currenciesComponent = new SaveDataCurrenciesComponent();
		currenciesComponent.Save(player.currenciesComponent);
		underlingsComponent = new SaveDataPlayerUnderlingsComponent();
		underlingsComponent.Save(player.underlingsComponent);
		tileObjectComponent = new SaveDataPlayerTileObjectComponent();
		tileObjectComponent.Save(player.tileObjectComponent);
		storedTargetsComponent = new SaveDataStoredTargetsComponent();
		storedTargetsComponent.Save(player.storedTargetsComponent);
		bookmarkComponent = new SaveDataBookmarkComponent();
		bookmarkComponent.Save(player.bookmarkComponent);
		summonMeterComponent = new SaveDataSummonMeterComponent();
		summonMeterComponent.Save(player.summonMeterComponent);
		damageAccumulator = new SaveDataPlayerDamageAccumulator();
		damageAccumulator.Save(player.damageAccumulator);
		retaliationComponent = new SaveDataPlayerRetaliationComponent();
		retaliationComponent.Save(player.retaliationComponent);
		devastationComponent = new SaveDataPlayerDevastationComponent();
		devastationComponent.Save(player.devastationComponent);
		primordialPoolDataHandler = new SaveDataPrimordialPoolDataHandler();
		primordialPoolDataHandler.Save(player.primordialPoolDataHandler);
		partyStructureDataHandler = new SaveDatapartyStructureDataHandler();
		partyStructureDataHandler.Save(player.partyStructureDataHandler);
		manaRegenComponent = new SaveDataManaRegenComponent();
		manaRegenComponent.Save(player.manaRegenComponent);
		goalComponent = new SaveDataGoalComponent();
		goalComponent.Save(player.goalComponent);
	}

	public override Player Load()
	{
		return new Player(this);
	}

	public override void CleanUp()
	{
		actionIntels?.Clear();
		actionIntels = null;
		interruptIntels?.Clear();
		interruptIntels = null;
		allNotifs?.Clear();
		allNotifs = null;
	}
}
