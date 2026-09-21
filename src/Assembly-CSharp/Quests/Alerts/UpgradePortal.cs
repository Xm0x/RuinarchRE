using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using Maccima_Games.Util;
using Tutorial;

namespace Quests.Alerts;

public class UpgradePortal : GameAlert
{
	protected override BOOKMARK_CATEGORY bookmarkCategory => BOOKMARK_CATEGORY.Portal;

	public override BOOKMARK_TYPE bookmarkType => BOOKMARK_TYPE.Text;

	public UpgradePortal()
		: base(Game_Alert.Upgrade_Portal)
	{
	}

	public UpgradePortal(SaveDataGameAlert p_data)
		: base(p_data, Game_Alert.Upgrade_Portal)
	{
	}

	public override void SetAsSpawned()
	{
		Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnPlayerAdjustedSpiritEnergy);
		if (PlayerManager.Instance.player.playerSkillComponent.IsCurrentlyUpgradingPortal())
		{
			Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPlayerFinishedPortalUpgrade);
			Messenger.AddListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
		}
		SetAsActiveUpgradePortal();
	}

	public override void SetAsActive()
	{
		if (!_isActive)
		{
			UpdateDisplayName();
			base.SetAsActive();
			Messenger.AddListener(PlayerSignals.PLAYER_STARTED_PORTAL_UPGRADE, OnPlayerStartedPortalUpgrade);
		}
	}

	protected override void SetAsCleared()
	{
		base.SetAsCleared();
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnPlayerAdjustedSpiritEnergy);
	}

	public override void CleanUp()
	{
		base.CleanUp();
		Messenger.RemoveListener(PlayerSignals.PLAYER_STARTED_PORTAL_UPGRADE, OnPlayerStartedPortalUpgrade);
		Messenger.RemoveListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPlayerFinishedPortalUpgrade);
		Messenger.RemoveListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
		Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnPlayerAdjustedSpiritEnergy);
	}

	public override void LoadReferences(SaveDataGameAlert data)
	{
		base.LoadReferences(data);
		if (_isActive)
		{
			Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_SPIRIT_ENERGY, OnPlayerAdjustedSpiritEnergy);
		}
	}

	private void OnPlayerAdjustedSpiritEnergy(int p_amountGained, int p_currentSpiritEnergy)
	{
		if (_isActive)
		{
			UpdateDisplayName();
			base.bookmarkEventDispatcher.ExecuteBookmarkChangedNameOrElementsEvent(this);
		}
	}

	private void OnPlayerStartedPortalUpgrade()
	{
		RemoveBookmark();
		SetAsInactive();
		Messenger.AddListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPlayerFinishedPortalUpgrade);
		Messenger.AddListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
	}

	private void OnPlayerFinishedPortalUpgrade(int p_portalLevel)
	{
		Messenger.RemoveListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPlayerFinishedPortalUpgrade);
		Messenger.RemoveListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
		if ((PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal).IsMaxLevel())
		{
			SetAsCleared();
		}
		else
		{
			RespawnAndSetAsActive();
		}
	}

	private void OnPlayerCancelledPortalUpgrade()
	{
		Messenger.RemoveListener<int>(PlayerSignals.PLAYER_FINISHED_PORTAL_UPGRADE, OnPlayerFinishedPortalUpgrade);
		Messenger.RemoveListener(PlayerSignals.PORTAL_UPGRADE_CANCELLED, OnPlayerCancelledPortalUpgrade);
		RespawnAndSetAsActive();
	}

	private void SetAsInactive()
	{
		if (_isActive)
		{
			_isActive = false;
			TutorialManager.Instance.RemoveAlertFromActiveList(this);
			Messenger.AddListener(PlayerSignals.PLAYER_STARTED_PORTAL_UPGRADE, OnPlayerStartedPortalUpgrade);
		}
	}

	private void RespawnAndSetAsActive()
	{
		SetAsActiveUpgradePortal();
	}

	private void SetAsActiveUpgradePortal()
	{
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		if (!PlayerManager.Instance.player.playerSkillComponent.IsCurrentlyUpgradingPortal() && !thePortal.IsMaxLevel())
		{
			SetAsActive();
		}
	}

	private void UpdateDisplayName()
	{
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		if (!thePortal.IsMaxLevel() && !PlayerManager.Instance.player.currenciesComponent.CanAfford(thePortal.nextTier.upgradeCost))
		{
			Cost cost = thePortal.nextTier.upgradeCost[0];
			int num = cost.amount - PlayerManager.Instance.player.currenciesComponent.spiritEnergy;
			Dictionary<string, string> dictionary = MaccimaDictionaryPool<string, string>.Claim();
			dictionary.Add("spiritEnergy", $"{num}{cost.currency.GetCurrencyTextSprite()}");
			_displayName = LocalizationManager.Instance.GetLocalizedValue("GameAlertStrings_Table", "Upgrade_Portal_Pending_Title", dictionary);
			MaccimaDictionaryPool<string, string>.Release(dictionary);
		}
		else
		{
			_displayName = GetLocalizedString("Upgrade_Portal_Title");
		}
	}

	public override void OnSelectBookmark()
	{
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		if (!thePortal.IsMaxLevel())
		{
			UIManager.Instance.ShowUpgradePortalUI(thePortal);
		}
	}

	public override void OnHoverOverBookmarkItem(UIHoverPosition p_pos)
	{
		base.OnHoverOverBookmarkItem(p_pos);
		string p_key = "Upgrade_Portal_Tooltip_1";
		ThePortal thePortal = PlayerManager.Instance.player.playerSettlement.GetFirstStructureOfType(STRUCTURE_TYPE.THE_PORTAL) as ThePortal;
		if (!thePortal.IsMaxLevel() && !PlayerManager.Instance.player.currenciesComponent.CanAfford(thePortal.nextTier.upgradeCost))
		{
			p_key = "Upgrade_Portal_Tooltip_2";
		}
		UIManager.Instance.ShowSmallInfo(GetLocalizedString(p_key), p_pos, "", autoReplaceText: false);
	}

	public override void OnHoverOutBookmarkItem()
	{
		base.OnHoverOutBookmarkItem();
		UIManager.Instance.HideSmallInfo();
	}

	public override void RemoveBookmark()
	{
		PlayerManager.Instance.player.bookmarkComponent.RemoveBookmark(this);
	}
}
