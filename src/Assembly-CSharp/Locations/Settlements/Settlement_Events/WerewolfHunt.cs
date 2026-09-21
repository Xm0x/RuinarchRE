namespace Locations.Settlements.Settlement_Events;

public class WerewolfHunt : SettlementEvent
{
	private GameDate _endDate;

	private string _currentScheduleKey;

	public override SETTLEMENT_EVENT eventType => SETTLEMENT_EVENT.Werewolf_Hunt;

	public GameDate endDate => _endDate;

	public WerewolfHunt(NPCSettlement location)
		: base(location)
	{
	}

	public WerewolfHunt(SaveDataWerewolfHunt data)
		: base(data)
	{
		LoadEnd(data.endDate);
		SubscribeListeners();
	}

	private void SubscribeListeners()
	{
		Messenger.AddListener<Character, CRIME_TYPE, Character>(CharacterSignals.CHARACTER_ACCUSED_OF_CRIME, OnCharacterAccusedOfCrime);
		Messenger.AddListener<Faction>(FactionSignals.FACTION_CRIMES_CHANGED, OnFactionCrimesChanged);
	}

	private void UnsubscribeListeners()
	{
		Messenger.RemoveListener<Character, CRIME_TYPE, Character>(CharacterSignals.CHARACTER_ACCUSED_OF_CRIME, OnCharacterAccusedOfCrime);
		Messenger.RemoveListener<Faction>(FactionSignals.FACTION_CRIMES_CHANGED, OnFactionCrimesChanged);
	}

	public override void ActivateEvent(NPCSettlement p_settlement)
	{
		p_settlement.AddNeededItems(TILE_OBJECT_TYPE.PHYLACTERY);
		ScheduleEnd();
		SubscribeListeners();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Settlement Event", "EventAlerts_Table", "Werewolf Hunt started", LOG_TAG.Major);
		log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		if (p_settlement.owner != null)
		{
			log.AddInvolvedObjectManual(p_settlement.owner.persistentID);
		}
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		AkSoundEngine.PostEvent("Play_Lycan_Hunt", InnerMapCameraMove.Instance.gameObject);
	}

	public override void DeactivateEvent(NPCSettlement p_settlement)
	{
		p_settlement.RemoveNeededItems(TILE_OBJECT_TYPE.PHYLACTERY);
		UnsubscribeListeners();
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Settlement Event", "EventAlerts_Table", "Werewolf Hunt ended", LOG_TAG.Major);
		log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		if (p_settlement.owner != null)
		{
			log.AddInvolvedObjectManual(p_settlement.owner.persistentID);
		}
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
		if (!string.IsNullOrEmpty(_currentScheduleKey))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_currentScheduleKey);
			_currentScheduleKey = string.Empty;
		}
	}

	private void ScheduleEnd()
	{
		_endDate = GameManager.Instance.Today();
		_endDate.AddDays(3);
		_currentScheduleKey = SchedulingManager.Instance.AddEntry(_endDate, delegate
		{
			base.location.eventManager.DeactivateEvent(this);
		}, base.location);
	}

	private void RescheduleEnd()
	{
		SchedulingManager.Instance.RemoveSpecificEntry(_currentScheduleKey);
		ScheduleEnd();
	}

	private void LoadEnd(GameDate date)
	{
		_endDate = date;
		_currentScheduleKey = SchedulingManager.Instance.AddEntry(date, delegate
		{
			base.location.eventManager.DeactivateEvent(this);
		}, base.location);
	}

	private void OnCharacterAccusedOfCrime(Character criminal, CRIME_TYPE crimeType, Character accuser)
	{
		if ((criminal.currentSettlement == base.location || criminal.homeSettlement == base.location) && crimeType == CRIME_TYPE.Werewolf)
		{
			RescheduleEnd();
		}
	}

	private void OnFactionCrimesChanged(Faction faction)
	{
		if (base.location.owner == faction && !faction.factionType.GetCrimeSeverity(CRIME_TYPE.Werewolf).IsConsideredACrime())
		{
			base.location.eventManager.DeactivateEvent(this);
		}
	}

	public override SaveDataSettlementEvent Save()
	{
		SaveDataWerewolfHunt saveDataWerewolfHunt = new SaveDataWerewolfHunt();
		saveDataWerewolfHunt.Save(this);
		return saveDataWerewolfHunt;
	}

	public override string GetTestingInfo()
	{
		return base.GetTestingInfo() + " will end on " + _endDate.ToString();
	}
}
