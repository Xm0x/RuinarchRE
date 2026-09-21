using System;
using Inner_Maps.Location_Structures;

public class ScheduledAction
{
	public string scheduleID;

	public Action action;

	public object scheduler;

	public void SetData(string p_scheduleID, Action p_action, object p_scheduler)
	{
		scheduleID = p_scheduleID;
		action = p_action;
		scheduler = p_scheduler;
	}

	public bool IsScheduleStillValid()
	{
		if (scheduler != null)
		{
			if (scheduler is Character character)
			{
				if (character.gridTileLocation != null)
				{
					return !character.hasBeenCleanedUp;
				}
				return false;
			}
			if (scheduler is TileObject tileObject)
			{
				return tileObject.gridTileLocation != null;
			}
			if (scheduler is LocationStructure locationStructure)
			{
				return !locationStructure.hasBeenCleanedUp;
			}
		}
		return true;
	}

	public override string ToString()
	{
		return $"{scheduleID} - {action.Method.Name} by {scheduler}";
	}

	public void Reset()
	{
		scheduleID = null;
		action = null;
		scheduler = null;
	}
}
