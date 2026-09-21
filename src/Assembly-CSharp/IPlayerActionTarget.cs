using System.Collections.Generic;

public interface IPlayerActionTarget
{
	string name { get; }

	List<PLAYER_SKILL_TYPE> actions { get; }

	void ConstructDefaultPlayerActions(bool broadcastSignal = true);

	void AddPlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true);

	void RemovePlayerAction(PLAYER_SKILL_TYPE action, bool broadcastSignal = true);

	void ClearPlayerActions();
}
