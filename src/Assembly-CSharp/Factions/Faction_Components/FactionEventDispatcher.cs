using System;
using Inner_Maps.Location_Structures;

namespace Factions.Faction_Components;

public class FactionEventDispatcher
{
	public interface IListener
	{
		void OnFactionLeaderChanged(ILeader p_newLeader);
	}

	private Action<ILeader> _factionLeaderChanged;

	public void SubscribeToFactionLeaderChangedEvent(IListener p_listener)
	{
		_factionLeaderChanged = (Action<ILeader>)Delegate.Combine(_factionLeaderChanged, new Action<ILeader>(p_listener.OnFactionLeaderChanged));
	}

	public void UnsubscribeToFactionLeaderChangedEvent(IListener p_listener)
	{
		_factionLeaderChanged = (Action<ILeader>)Delegate.Remove(_factionLeaderChanged, new Action<ILeader>(p_listener.OnFactionLeaderChanged));
	}

	public void ExecuteFactionLeaderChangedEvent(ILeader p_newLeader)
	{
		_factionLeaderChanged?.Invoke(p_newLeader);
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
	}

	public void OnDisbandFaction()
	{
		_factionLeaderChanged = null;
	}
}
