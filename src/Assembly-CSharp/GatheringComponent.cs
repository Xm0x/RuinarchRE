using Inner_Maps.Location_Structures;

public class GatheringComponent : CharacterComponent
{
	public Gathering currentGathering { get; private set; }

	public bool hasGathering => currentGathering != null;

	public GatheringComponent()
	{
	}

	public GatheringComponent(SaveDataGatheringComponent data)
	{
	}

	public void SetCurrentGathering(Gathering gathering)
	{
		currentGathering = gathering;
	}

	public void LoadReferences(SaveDataGatheringComponent data)
	{
		if (!string.IsNullOrEmpty(data.currentGathering))
		{
			currentGathering = DatabaseManager.Instance.gatheringDatabase.GetGatheringByPersistentID(data.currentGathering);
		}
	}

	public void DisconnectFromCharacter(Character p_character)
	{
		if (currentGathering != null)
		{
			if (currentGathering.host == p_character || currentGathering.jobOwner == p_character)
			{
				currentGathering.DisbandGathering();
			}
			else
			{
				currentGathering.RemoveAttendee(p_character);
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		currentGathering?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
