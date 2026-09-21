using System;
using System.Collections.Generic;

public class ActionDatabase
{
	public Dictionary<string, ActualGoapNode> allActions { get; }

	public ActionDatabase()
	{
		allActions = new Dictionary<string, ActualGoapNode>();
	}

	public void AddAction(ActualGoapNode action)
	{
		if (!allActions.ContainsKey(action.persistentID))
		{
			allActions.Add(action.persistentID, action);
		}
	}

	public void RemoveAction(string action)
	{
		if (allActions.ContainsKey(action))
		{
			allActions.Remove(action);
		}
	}

	public ActualGoapNode GetActionByPersistentID(string id)
	{
		if (allActions.ContainsKey(id))
		{
			return allActions[id];
		}
		throw new NullReferenceException("Trying to get an action from the database with id " + id + " but the action is not loaded");
	}

	public ActualGoapNode GetActionByPersistentIDSafe(string id)
	{
		if (allActions.ContainsKey(id))
		{
			return allActions[id];
		}
		return null;
	}
}
