using System;

namespace Locations.Settlements.Components;

public class NPCSettlementEventDispatcher
{
	public interface IListener
	{
		void OnSettlementRulerChanged(Character p_newLeader, NPCSettlement p_settlement);

		void OnFactionOwnerChanged(Faction p_previousOwner, Faction p_newOwner, NPCSettlement p_settlement);
	}

	public interface ITimeListener
	{
		void OnHourStarted(NPCSettlement p_settlement);
	}

	public interface ITileListener
	{
		void OnSettlementAreaRemoved(Area p_area, NPCSettlement p_settlement);
	}

	private Action<Character, NPCSettlement> _settlementRulerChanged;

	private Action<Faction, Faction, NPCSettlement> _factionOwnerChanged;

	private Action<NPCSettlement> _hourStarted;

	private Action<Area, NPCSettlement> _settlementAreaRemoved;

	public void SubscribeToSettlementRulerChangedEvent(IListener p_listener)
	{
		_settlementRulerChanged = (Action<Character, NPCSettlement>)Delegate.Combine(_settlementRulerChanged, new Action<Character, NPCSettlement>(p_listener.OnSettlementRulerChanged));
	}

	public void UnsubscribeToSettlementRulerChangedEvent(IListener p_listener)
	{
		_settlementRulerChanged = (Action<Character, NPCSettlement>)Delegate.Remove(_settlementRulerChanged, new Action<Character, NPCSettlement>(p_listener.OnSettlementRulerChanged));
	}

	public void ExecuteSettlementRulerChangedEvent(Character p_newRuler, NPCSettlement p_settlement)
	{
		_settlementRulerChanged?.Invoke(p_newRuler, p_settlement);
	}

	public void SubscribeToFactionOwnerChangedEvent(IListener p_listener)
	{
		_factionOwnerChanged = (Action<Faction, Faction, NPCSettlement>)Delegate.Combine(_factionOwnerChanged, new Action<Faction, Faction, NPCSettlement>(p_listener.OnFactionOwnerChanged));
	}

	public void UnsubscribeToFactionOwnerChangedEvent(IListener p_listener)
	{
		_factionOwnerChanged = (Action<Faction, Faction, NPCSettlement>)Delegate.Remove(_factionOwnerChanged, new Action<Faction, Faction, NPCSettlement>(p_listener.OnFactionOwnerChanged));
	}

	public void ExecuteFactionOwnerChangedEvent(Faction p_previousOwner, Faction p_newOwner, NPCSettlement p_settlement)
	{
		_factionOwnerChanged?.Invoke(p_previousOwner, p_newOwner, p_settlement);
	}

	public void SubscribeToHourStartedEvent(ITimeListener p_listener)
	{
		_hourStarted = (Action<NPCSettlement>)Delegate.Combine(_hourStarted, new Action<NPCSettlement>(p_listener.OnHourStarted));
	}

	public void UnsubscribeToHourStartedEvent(ITimeListener p_listener)
	{
		_hourStarted = (Action<NPCSettlement>)Delegate.Remove(_hourStarted, new Action<NPCSettlement>(p_listener.OnHourStarted));
	}

	public void ExecuteHourStartedEvent(NPCSettlement p_settlement)
	{
		_hourStarted?.Invoke(p_settlement);
	}

	public void SubscribeToTileRemovedEvent(ITileListener p_listener)
	{
		_settlementAreaRemoved = (Action<Area, NPCSettlement>)Delegate.Combine(_settlementAreaRemoved, new Action<Area, NPCSettlement>(p_listener.OnSettlementAreaRemoved));
	}

	public void UnsubscribeToTileRemovedEvent(ITileListener p_listener)
	{
		_settlementAreaRemoved = (Action<Area, NPCSettlement>)Delegate.Remove(_settlementAreaRemoved, new Action<Area, NPCSettlement>(p_listener.OnSettlementAreaRemoved));
	}

	public void ExecuteTileRemovedEvent(Area p_area, NPCSettlement p_settlement)
	{
		_settlementAreaRemoved?.Invoke(p_area, p_settlement);
	}
}
