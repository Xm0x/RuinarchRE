using System;
using System.Collections.Generic;
using Locations.Settlements.Settlement_Events;

public class LocationEventManager
{
	private readonly NPCSettlement _location;

	private readonly List<SettlementEvent> _activeEvents;

	public List<SettlementEvent> activeEvents => _activeEvents;

	public LocationEventManager(NPCSettlement location)
	{
		_location = location;
		_activeEvents = new List<SettlementEvent>();
	}

	public LocationEventManager(NPCSettlement location, SaveDataLocationEventManager saveData)
	{
		_location = location;
		_activeEvents = new List<SettlementEvent>();
		for (int i = 0; i < saveData.settlementEvents.Count; i++)
		{
			SettlementEvent settlementEvent = saveData.settlementEvents[i].Load();
			settlementEvent.LoadAdditionalData(location);
			_activeEvents.Add(settlementEvent);
		}
	}

	public void AddNewActiveEvent(SETTLEMENT_EVENT settlementEvent)
	{
		SettlementEvent settlementEvent2 = CreateNewSettlementEvent(settlementEvent);
		activeEvents.Add(settlementEvent2);
		settlementEvent2.ActivateEvent(_location);
	}

	public void DeactivateEvent(SettlementEvent settlementEvent)
	{
		if (activeEvents.Remove(settlementEvent))
		{
			settlementEvent.DeactivateEvent(_location);
		}
	}

	public bool HasActiveEvent(SETTLEMENT_EVENT p_settlementEventType)
	{
		for (int i = 0; i < activeEvents.Count; i++)
		{
			if (activeEvents[i].eventType == p_settlementEventType)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasActiveEvent<T>(out T p_settlementEvent) where T : SettlementEvent
	{
		for (int i = 0; i < activeEvents.Count; i++)
		{
			if (activeEvents[i] is T val)
			{
				p_settlementEvent = val;
				return true;
			}
		}
		p_settlementEvent = null;
		return false;
	}

	public bool HasActiveEvent(SettlementEvent p_settlementEvent)
	{
		return activeEvents.Contains(p_settlementEvent);
	}

	public T GetActiveEvent<T>() where T : SettlementEvent
	{
		for (int i = 0; i < activeEvents.Count; i++)
		{
			if (activeEvents[i] is T result)
			{
				return result;
			}
		}
		return null;
	}

	private SettlementEvent CreateNewSettlementEvent(SETTLEMENT_EVENT settlementEvent)
	{
		string text = "Locations.Settlements.Settlement_Events." + settlementEvent.ToStringEnumNoSpace();
		Type type = Type.GetType(text);
		if (type != null)
		{
			return Activator.CreateInstance(type, _location) as SettlementEvent;
		}
		throw new Exception("Could not create new instance of settlement event of type " + text);
	}

	private T CreateNewSettlementEvent<T>() where T : SettlementEvent
	{
		Type typeFromHandle = typeof(T);
		if (Activator.CreateInstance(typeFromHandle, _location) is T result)
		{
			return result;
		}
		throw new Exception($"Could not create new instance of settlement event of type {typeFromHandle}");
	}

	public void OnResidentAdded(Character character)
	{
		for (int i = 0; i < activeEvents.Count; i++)
		{
			activeEvents[i].ProcessNewVillager(character);
		}
	}

	public void OnResidentRemoved(Character character)
	{
		for (int i = 0; i < activeEvents.Count; i++)
		{
			activeEvents[i].ProcessRemovedVillager(character);
		}
	}

	public bool CanHaveEvents()
	{
		if (_location.owner == null)
		{
			return false;
		}
		if (_location.owner.isPlayerFaction)
		{
			return false;
		}
		if (!_location.owner.isMajorFaction)
		{
			return false;
		}
		return true;
	}

	public void OnSettlementDestroyed()
	{
		List<SettlementEvent> list = new List<SettlementEvent>(_activeEvents);
		for (int i = 0; i < list.Count; i++)
		{
			DeactivateEvent(list[i]);
		}
	}
}
