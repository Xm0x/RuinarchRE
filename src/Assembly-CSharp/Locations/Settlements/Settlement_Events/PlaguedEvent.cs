using System;
using System.Collections.Generic;
using System.Linq;
using Factions.Faction_Components;
using Locations.Settlements.Components;
using Plague.Transmission;
using UtilityScripts;

namespace Locations.Settlements.Settlement_Events;

public class PlaguedEvent : SettlementEvent, FactionEventDispatcher.IListener, NPCSettlementEventDispatcher.IListener, IPlagueTransmissionListener
{
	private PLAGUE_EVENT_RESPONSE _rulerDecision;

	private GameDate _endDate;

	private string _endScheduleTicket;

	private bool _hasLeaderMadeADecision;

	public override SETTLEMENT_EVENT eventType => SETTLEMENT_EVENT.Plagued_Event;

	public PLAGUE_EVENT_RESPONSE rulerDecision => _rulerDecision;

	public GameDate endDate => _endDate;

	public bool hasLeaderMadeADecision => _hasLeaderMadeADecision;

	public PlaguedEvent(NPCSettlement location)
		: base(location)
	{
		_rulerDecision = PLAGUE_EVENT_RESPONSE.Undecided;
	}

	public PlaguedEvent(SaveDataPlaguedSettlementEvent data)
		: base(data)
	{
		LoadEnd(data.endDate);
		_rulerDecision = data.rulerDecision;
		_hasLeaderMadeADecision = data.hasLeaderMadeADecision;
	}

	public override void ActivateEvent(NPCSettlement p_settlement)
	{
		Character leaderThatWillDecideResponse = GetLeaderThatWillDecideResponse(p_settlement);
		LogEvent(leaderThatWillDecideResponse, p_settlement, "started");
		DetermineLeaderResponse(leaderThatWillDecideResponse, p_settlement);
		SubscribeListeners(p_settlement);
		ScheduleEnd(p_settlement);
		AkSoundEngine.PostEvent("Play_Plagued_Event", InnerMapCameraMove.Instance.gameObject);
	}

	public override void DeactivateEvent(NPCSettlement p_settlement)
	{
		if (p_settlement.owner != null)
		{
			RevertFactionEffects(p_settlement.owner);
		}
		List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
		p_settlement.PopulateJobsOfType(list, JOB_TYPE.PLAGUE_CARE, JOB_TYPE.QUARANTINE);
		for (int i = 0; i < list.Count; i++)
		{
			list[i].ForceCancelJob("Settlement_No_Quarantine");
		}
		RuinarchListPool<JobQueueItem>.Release(list);
		UnsubscribeListeners(p_settlement);
		if (!string.IsNullOrEmpty(_endScheduleTicket))
		{
			SchedulingManager.Instance.RemoveSpecificEntry(_endScheduleTicket);
		}
	}

	public override SaveDataSettlementEvent Save()
	{
		SaveDataPlaguedSettlementEvent saveDataPlaguedSettlementEvent = new SaveDataPlaguedSettlementEvent();
		saveDataPlaguedSettlementEvent.Save(this);
		return saveDataPlaguedSettlementEvent;
	}

	private void SubscribeListeners(NPCSettlement p_settlement)
	{
		p_settlement.npcSettlementEventDispatcher.SubscribeToFactionOwnerChangedEvent(this);
		p_settlement.npcSettlementEventDispatcher.SubscribeToSettlementRulerChangedEvent(this);
		p_settlement.owner?.factionEventDispatcher.SubscribeToFactionLeaderChangedEvent(this);
		Transmission<AirborneTransmission>.Instance.SubscribeToTransmission(this);
		Transmission<ConsumptionTransmission>.Instance.SubscribeToTransmission(this);
		Transmission<PhysicalContactTransmission>.Instance.SubscribeToTransmission(this);
		Transmission<CombatRateTransmission>.Instance.SubscribeToTransmission(this);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
		Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		Messenger.AddListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
	}

	private void UnsubscribeListeners(NPCSettlement p_settlement)
	{
		p_settlement.npcSettlementEventDispatcher.UnsubscribeToFactionOwnerChangedEvent(this);
		p_settlement.npcSettlementEventDispatcher.UnsubscribeToSettlementRulerChangedEvent(this);
		p_settlement.owner?.factionEventDispatcher.UnsubscribeToFactionLeaderChangedEvent(this);
		Transmission<AirborneTransmission>.Instance.UnsubscribeToTransmission(this);
		Transmission<ConsumptionTransmission>.Instance.UnsubscribeToTransmission(this);
		Transmission<PhysicalContactTransmission>.Instance.UnsubscribeToTransmission(this);
		Transmission<CombatRateTransmission>.Instance.UnsubscribeToTransmission(this);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_MOVE_AGAIN, OnCharacterCanMoveAgain);
		Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_PERFORM_AGAIN, OnCharacterCanPerformAgain);
		Messenger.RemoveListener<Character, Area>(CharacterSignals.CHARACTER_ENTERED_AREA, OnCharacterEnteredArea);
	}

	private void OnCharacterCanMoveAgain(Character p_character)
	{
		if (!_hasLeaderMadeADecision && GetLeaderThatWillDecideResponse(_location) == p_character && CanLeaderMakeADecision(p_character))
		{
			DetermineLeaderResponse(p_character, _location);
		}
	}

	private void OnCharacterCanPerformAgain(Character p_character)
	{
		if (!_hasLeaderMadeADecision && GetLeaderThatWillDecideResponse(_location) == p_character && CanLeaderMakeADecision(p_character))
		{
			DetermineLeaderResponse(p_character, _location);
		}
	}

	private void OnCharacterEnteredArea(Character p_character, Area p_area)
	{
		if (!_hasLeaderMadeADecision && GetLeaderThatWillDecideResponse(_location) == p_character && CanLeaderMakeADecision(p_character))
		{
			DetermineLeaderResponse(p_character, _location);
		}
	}

	private void ScheduleEnd(NPCSettlement p_settlement)
	{
		_endDate = GameManager.Instance.Today();
		_endDate = _endDate.AddDays(3);
		_endScheduleTicket = SchedulingManager.Instance.AddEntry(_endDate, delegate
		{
			DeactivateEventBySchedule(p_settlement);
		}, p_settlement);
	}

	private void RescheduleEnd(NPCSettlement p_settlement)
	{
		SchedulingManager.Instance.RemoveSpecificEntry(_endScheduleTicket);
		ScheduleEnd(p_settlement);
	}

	private void DeactivateEventBySchedule(NPCSettlement p_settlement)
	{
		p_settlement.eventManager.DeactivateEvent(this);
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Settlement Event", "EventAlerts_Table", "Plagued ended", LOG_TAG.Major);
		log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		if (p_settlement.owner != null)
		{
			log.AddInvolvedObjectManual(p_settlement.owner.persistentID);
		}
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	private void DetermineLeaderResponse(Character p_leader, NPCSettlement p_settlement)
	{
		string text = string.Empty;
		PLAGUE_EVENT_RESPONSE pLAGUE_EVENT_RESPONSE;
		if (p_leader == null)
		{
			pLAGUE_EVENT_RESPONSE = PLAGUE_EVENT_RESPONSE.Do_Nothing;
			_hasLeaderMadeADecision = false;
		}
		else if (!CanLeaderMakeADecision(p_leader))
		{
			pLAGUE_EVENT_RESPONSE = PLAGUE_EVENT_RESPONSE.Do_Nothing;
			_hasLeaderMadeADecision = false;
		}
		else
		{
			pLAGUE_EVENT_RESPONSE = GetLeaderPlagueEventResponse(p_leader, p_settlement);
			text = pLAGUE_EVENT_RESPONSE.ToStringEnum();
			_hasLeaderMadeADecision = true;
		}
		if (_rulerDecision != pLAGUE_EVENT_RESPONSE)
		{
			RevertEffectsOfLeaderPreviousResponseToPlague(_rulerDecision, p_settlement.owner, p_settlement);
			SetLeaderResponse(pLAGUE_EVENT_RESPONSE);
			ExecuteEffectsOfLeaderResponseToPlague(pLAGUE_EVENT_RESPONSE, p_settlement.owner, p_settlement);
			if (!string.IsNullOrEmpty(text))
			{
				LogEvent(p_leader, p_settlement, text);
			}
		}
	}

	private void LogEvent(Character p_leader, NPCSettlement p_settlement, string key)
	{
		Log log = GameManager.CreateNewLogUsingNewLocalization(GameManager.Instance.Today(), "Settlement Event", "EventAlerts_Table", "Plagued " + key, LOG_TAG.Major);
		log.AddToFillers(p_settlement, p_settlement.name, LOG_IDENTIFIER.LANDMARK_1);
		if (p_leader != null)
		{
			log.AddToFillers(p_leader, p_leader.name, LOG_IDENTIFIER.ACTIVE_CHARACTER);
		}
		if (p_settlement.owner != null)
		{
			log.AddInvolvedObjectManual(p_settlement.owner.persistentID);
		}
		log.AddLogToDatabase();
		PlayerManager.Instance.player.ShowNotificationFromPlayer(log, releaseLogAfter: true);
	}

	private PLAGUE_EVENT_RESPONSE GetLeaderPlagueEventResponse(Character p_leader, NPCSettlement p_settlement)
	{
		if (p_leader.traitContainer.HasTrait("Evil", "Psychopath", "Ruthless"))
		{
			return PLAGUE_EVENT_RESPONSE.Slay;
		}
		if (p_leader.traitContainer.HasTrait("Demon Cultist"))
		{
			if (p_settlement.owner != null && p_settlement.owner.factionType.type == FACTION_TYPE.Demon_Cult)
			{
				if (GameUtilities.RollChance(30))
				{
					return PLAGUE_EVENT_RESPONSE.Slay;
				}
				if (p_settlement.HasStructure(STRUCTURE_TYPE.HOSPICE))
				{
					return PLAGUE_EVENT_RESPONSE.Quarantine;
				}
				return PLAGUE_EVENT_RESPONSE.Exile;
			}
			return PLAGUE_EVENT_RESPONSE.Slay;
		}
		if (p_leader.traitContainer.HasTrait("Diplomatic", "Inspiring"))
		{
			return p_settlement.HasStructure(STRUCTURE_TYPE.HOSPICE) ? PLAGUE_EVENT_RESPONSE.Quarantine : PLAGUE_EVENT_RESPONSE.Exile;
		}
		if (p_leader.traitContainer.HasTrait("Coward", "Lazy"))
		{
			return PLAGUE_EVENT_RESPONSE.Do_Nothing;
		}
		return p_settlement.HasStructure(STRUCTURE_TYPE.HOSPICE) ? PLAGUE_EVENT_RESPONSE.Quarantine : PLAGUE_EVENT_RESPONSE.Exile;
	}

	private void SetLeaderResponse(PLAGUE_EVENT_RESPONSE p_response)
	{
		_rulerDecision = p_response;
	}

	private void RevertEffectsOfLeaderPreviousResponseToPlague(PLAGUE_EVENT_RESPONSE p_response, Faction p_faction, NPCSettlement p_settlement)
	{
		if (p_response == PLAGUE_EVENT_RESPONSE.Quarantine)
		{
			List<JobQueueItem> list = RuinarchListPool<JobQueueItem>.Claim();
			p_settlement.PopulateJobsOfType(list, JOB_TYPE.PLAGUE_CARE, JOB_TYPE.QUARANTINE);
			for (int i = 0; i < list.Count; i++)
			{
				list[i].ForceCancelJob("Settlement_No_Quarantine");
			}
			RuinarchListPool<JobQueueItem>.Release(list);
		}
	}

	private void ExecuteEffectsOfLeaderResponseToPlague(PLAGUE_EVENT_RESPONSE p_response, Faction p_faction, NPCSettlement p_settlement)
	{
		switch (p_response)
		{
		case PLAGUE_EVENT_RESPONSE.Do_Nothing:
			p_faction.factionType.AddCrime(CRIME_TYPE.Plagued, CRIME_SEVERITY.Infraction);
			break;
		case PLAGUE_EVENT_RESPONSE.Quarantine:
			p_faction.factionType.AddCrime(CRIME_TYPE.Plagued, CRIME_SEVERITY.Infraction);
			p_settlement.settlementJobTriggerComponent.AddJobTrigger(p_settlement, SETTLEMENT_JOB_TRIGGER.Plague_Care);
			break;
		case PLAGUE_EVENT_RESPONSE.Slay:
			p_faction.factionType.AddCrime(CRIME_TYPE.Plagued, CRIME_SEVERITY.Heinous);
			break;
		case PLAGUE_EVENT_RESPONSE.Exile:
			p_faction.factionType.AddCrime(CRIME_TYPE.Plagued, CRIME_SEVERITY.Serious);
			break;
		default:
			throw new ArgumentOutOfRangeException("p_response", p_response, null);
		}
		Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, p_faction);
	}

	private Character GetLeaderThatWillDecideResponse(NPCSettlement p_settlement)
	{
		if (p_settlement.owner != null && p_settlement.owner.leader is Character { homeSettlement: not null } character && character.homeSettlement == p_settlement)
		{
			return character;
		}
		if (p_settlement.ruler != null)
		{
			return p_settlement.ruler;
		}
		return null;
	}

	private bool CanLeaderMakeADecision(Character p_leader)
	{
		if (p_leader.limiterComponent.canMove && p_leader.limiterComponent.canPerform)
		{
			return p_leader.IsAtHome();
		}
		return false;
	}

	private void RevertFactionEffects(Faction p_faction)
	{
		if (!p_faction.ownedSettlements.Any((BaseSettlement s) => s is NPCSettlement nPCSettlement && nPCSettlement.eventManager.HasActiveEvent(SETTLEMENT_EVENT.Plagued_Event)))
		{
			p_faction.factionType.RemoveCrime(CRIME_TYPE.Plagued);
			Messenger.Broadcast(FactionSignals.FACTION_CRIMES_CHANGED, p_faction);
		}
	}

	public void OnFactionLeaderChanged(ILeader p_newLeader)
	{
		if (p_newLeader is Character { homeSettlement: not null } character && character.homeSettlement.eventManager.HasActiveEvent(this))
		{
			DetermineLeaderResponse(character, character.homeSettlement);
		}
	}

	public void OnSettlementRulerChanged(Character p_newLeader, NPCSettlement p_settlement)
	{
		if (p_newLeader != null && GetLeaderThatWillDecideResponse(p_settlement) == p_newLeader)
		{
			DetermineLeaderResponse(p_newLeader, p_settlement);
		}
	}

	public void OnFactionOwnerChanged(Faction p_previousOwner, Faction p_newOwner, NPCSettlement p_settlement)
	{
		if (p_previousOwner != null)
		{
			RevertFactionEffects(p_previousOwner);
			p_previousOwner.factionEventDispatcher.UnsubscribeToFactionLeaderChangedEvent(this);
		}
		p_newOwner?.factionEventDispatcher.SubscribeToFactionLeaderChangedEvent(this);
		if (p_newOwner == null)
		{
			p_settlement.eventManager.DeactivateEvent(this);
		}
	}

	public void OnPlagueTransmitted(IPointOfInterest p_target)
	{
		if (p_target is Character { homeSettlement: not null } character && character.homeSettlement.eventManager.HasActiveEvent(this))
		{
			RescheduleEnd(character.homeSettlement);
		}
	}

	private void LoadEnd(GameDate date)
	{
		_endDate = date;
		_endScheduleTicket = SchedulingManager.Instance.AddEntry(date, delegate
		{
			DeactivateEventBySchedule(base.location);
		}, base.location);
	}

	public override void LoadAdditionalData(NPCSettlement p_settlement)
	{
		base.LoadAdditionalData(p_settlement);
		SubscribeListeners(p_settlement);
		if (_rulerDecision == PLAGUE_EVENT_RESPONSE.Quarantine)
		{
			p_settlement.settlementJobTriggerComponent.AddJobTrigger(p_settlement, SETTLEMENT_JOB_TRIGGER.Plague_Care);
		}
	}

	public static bool HasMinimumAmountOfPlaguedVillagersForEvent(NPCSettlement p_settlement)
	{
		if (p_settlement.residents.Count >= 1)
		{
			int num = p_settlement.residents.Count((Character r) => !r.isDead);
			return (float)p_settlement.residents.Count((Character r) => !r.isDead && r.traitContainer.HasTrait("Plagued")) / (float)num >= 0.1f;
		}
		return false;
	}

	public override string GetTestingInfo()
	{
		string testingInfo = base.GetTestingInfo();
		return testingInfo + " will end on " + endDate.ToString() + "(" + _rulerDecision.ToStringEnum() + ")";
	}
}
