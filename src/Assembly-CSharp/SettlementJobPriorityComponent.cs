using System.Collections.Generic;

public class SettlementJobPriorityComponent
{
	public NPCSettlement settlement { get; private set; }

	public Dictionary<JOB_TYPE, int> primaryJobTracker { get; private set; }

	public SettlementJobPriorityComponent(NPCSettlement settlement)
	{
		this.settlement = settlement;
		ConstructPrimaryJobTracker();
	}

	private void ConstructPrimaryJobTracker()
	{
		primaryJobTracker = new Dictionary<JOB_TYPE, int>
		{
			{
				JOB_TYPE.PRODUCE_FOOD,
				0
			},
			{
				JOB_TYPE.PRODUCE_WOOD,
				0
			},
			{
				JOB_TYPE.HAUL,
				0
			},
			{
				JOB_TYPE.DOUSE_FIRE,
				0
			},
			{
				JOB_TYPE.REPAIR,
				0
			},
			{
				JOB_TYPE.CRAFT_OBJECT,
				0
			},
			{
				JOB_TYPE.RESTRAIN,
				0
			},
			{
				JOB_TYPE.REMOVE_STATUS,
				0
			},
			{
				JOB_TYPE.TEND_FARM,
				0
			},
			{
				JOB_TYPE.MINE,
				0
			},
			{
				JOB_TYPE.BUILD_BLUEPRINT,
				0
			}
		};
	}

	public string GetJobAssignments()
	{
		string text = string.Empty;
		if (primaryJobTracker != null)
		{
			foreach (KeyValuePair<JOB_TYPE, int> item in primaryJobTracker)
			{
				text = text + "\n" + item.Key.ToStringEnum() + " - " + item.Value;
			}
		}
		return text;
	}
}
