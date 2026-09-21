using Inner_Maps.Location_Structures;
using UtilityScripts;

public abstract class JobNode
{
	public string persistentID { get; }

	public abstract ActualGoapNode singleNode { get; }

	public JobNode()
	{
		persistentID = Utilities.GetNewUniqueID();
	}

	protected JobNode(SaveDataJobNode saveDataJobNode)
	{
		persistentID = saveDataJobNode.persistentID;
	}

	public abstract void OnAttachPlanToJob(GoapPlanJob job);

	public abstract void OnUnattachPlanToJob(GoapPlanJob job);

	public abstract void SetNextActualNode();

	public abstract bool IsCurrentActionNode(ActualGoapNode node);

	public abstract void Reset();

	public void DisconnectFromCharacter(Character p_character)
	{
		singleNode?.DisconnectFromCharacter(p_character);
	}

	public bool IsStructureReferenced(LocationStructure p_structure)
	{
		return singleNode?.IsStructureReferenced(p_structure) ?? false;
	}

	public bool IsCharacterReferenced(Character p_character)
	{
		return singleNode?.IsCharacterReferenced(p_character) ?? false;
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		singleNode?.CheckIfStructureIsStillReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		singleNode?.CheckIfCharacterIsStillReferenced(p_character);
	}
}
