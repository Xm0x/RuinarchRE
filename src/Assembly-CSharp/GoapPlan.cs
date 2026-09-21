using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class GoapPlan : IObjectPoolTester
{
	public IPointOfInterest target { get; private set; }

	public JobNode startingNode { get; private set; }

	public JobNode endNode { get; private set; }

	public JobNode currentNode { get; private set; }

	public JobNode previousNode { get; private set; }

	public ActualGoapNode currentActualNode => GetCurrentActualNode();

	public List<JobNode> allNodes { get; private set; }

	public int currentNodeIndex { get; private set; }

	public bool isEnd { get; private set; }

	public bool isBeingRecalculated { get; private set; }

	public bool isPersonalPlan { get; private set; }

	public bool doNotRecalculate { get; private set; }

	public bool resetPlanOnFinishRecalculation { get; private set; }

	public bool isAssigned { get; set; }

	public GoapPlan()
	{
	}

	public GoapPlan(SaveDataGoapPlan data)
	{
		if (!string.IsNullOrEmpty(data.poiTargetID))
		{
			if (data.targetObjectType == OBJECT_TYPE.Character)
			{
				target = DatabaseManager.Instance.characterDatabase.GetCharacterByPersistentID(data.poiTargetID);
			}
			else
			{
				target = DatabaseManager.Instance.tileObjectDatabase.GetTileObjectByPersistentID(data.poiTargetID);
			}
		}
		currentNodeIndex = data.currentNodeIndex;
		isEnd = data.isEnd;
		isPersonalPlan = data.isPersonalPlan;
		doNotRecalculate = data.doNotRecalculate;
		isAssigned = data.isAssigned;
		allNodes = RuinarchListPool<JobNode>.Claim();
		for (int i = 0; i < data.allNodes.Count; i++)
		{
			JobNode jobNode = data.allNodes[i].Load();
			if (jobNode.singleNode != null)
			{
				allNodes.Add(jobNode);
			}
		}
		if (allNodes.Count > 0)
		{
			startingNode = GetJobNodeWithPersistentID(data.startingNodeID);
			endNode = GetJobNodeWithPersistentID(data.endNodeID);
			currentNode = GetJobNodeWithPersistentID(data.currentNodeID);
			previousNode = GetJobNodeWithPersistentID(data.previousNodeID);
		}
	}

	public void SetTarget(IPointOfInterest p_target)
	{
		target = p_target;
	}

	public void SetIsPersonalPlan(bool p_state)
	{
		isPersonalPlan = p_state;
	}

	private JobNode GetJobNodeWithPersistentID(string id)
	{
		for (int i = 0; i < allNodes.Count; i++)
		{
			JobNode jobNode = allNodes[i];
			if (jobNode.persistentID == id)
			{
				return jobNode;
			}
		}
		return null;
	}

	public void SetActionNodes(ActualGoapNode p_action)
	{
		if (allNodes == null)
		{
			allNodes = RuinarchListPool<JobNode>.Claim();
		}
		SingleJobNode singleJobNode = ObjectPoolManager.Instance.CreateNewSingleJobNode();
		singleJobNode.SetActionNode(p_action);
		allNodes.Add(singleJobNode);
		InitializeNodes();
	}

	public void SetActionNodes(ActualGoapNode p_action1, ActualGoapNode p_action2)
	{
		if (allNodes == null)
		{
			allNodes = RuinarchListPool<JobNode>.Claim();
		}
		SingleJobNode singleJobNode = ObjectPoolManager.Instance.CreateNewSingleJobNode();
		SingleJobNode singleJobNode2 = ObjectPoolManager.Instance.CreateNewSingleJobNode();
		singleJobNode.SetActionNode(p_action1);
		singleJobNode2.SetActionNode(p_action2);
		allNodes.Add(singleJobNode);
		allNodes.Add(singleJobNode2);
		InitializeNodes();
	}

	public void SetActionNodes(ActualGoapNode p_action1, ActualGoapNode p_action2, ActualGoapNode p_action3, ActualGoapNode p_action4, ActualGoapNode p_action5)
	{
		if (allNodes == null)
		{
			allNodes = RuinarchListPool<JobNode>.Claim();
		}
		SingleJobNode singleJobNode = ObjectPoolManager.Instance.CreateNewSingleJobNode();
		SingleJobNode singleJobNode2 = ObjectPoolManager.Instance.CreateNewSingleJobNode();
		SingleJobNode singleJobNode3 = ObjectPoolManager.Instance.CreateNewSingleJobNode();
		SingleJobNode singleJobNode4 = ObjectPoolManager.Instance.CreateNewSingleJobNode();
		SingleJobNode singleJobNode5 = ObjectPoolManager.Instance.CreateNewSingleJobNode();
		singleJobNode.SetActionNode(p_action1);
		singleJobNode2.SetActionNode(p_action2);
		singleJobNode3.SetActionNode(p_action3);
		singleJobNode4.SetActionNode(p_action4);
		singleJobNode5.SetActionNode(p_action5);
		allNodes.Add(singleJobNode);
		allNodes.Add(singleJobNode2);
		allNodes.Add(singleJobNode3);
		allNodes.Add(singleJobNode4);
		allNodes.Add(singleJobNode5);
		InitializeNodes();
	}

	public void SetNodes(List<JobNode> nodes)
	{
		allNodes = nodes;
		InitializeNodes();
	}

	private void InitializeNodes()
	{
		startingNode = allNodes[0];
		endNode = allNodes[allNodes.Count - 1];
		currentNode = startingNode;
		currentNodeIndex = 0;
	}

	public void SetNextNode()
	{
		if (currentNode != null)
		{
			previousNode = currentNode;
			int num = currentNodeIndex + 1;
			if (num < allNodes.Count)
			{
				currentNode = allNodes[num];
				currentNodeIndex = num;
			}
			else
			{
				currentNode = null;
			}
		}
	}

	private ActualGoapNode GetCurrentActualNode()
	{
		return currentNode.singleNode;
	}

	public bool HasNodeWithAction(INTERACTION_TYPE actionType)
	{
		if (allNodes != null)
		{
			for (int i = 0; i < allNodes.Count; i++)
			{
				if (allNodes[i].singleNode.goapType == actionType)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void SetIsBeingRecalculated(bool state)
	{
		isBeingRecalculated = state;
	}

	public void SetDoNotRecalculate(bool state)
	{
		doNotRecalculate = state;
	}

	public void SetResetPlanOnFinishRecalculation(bool p_state)
	{
		resetPlanOnFinishRecalculation = p_state;
	}

	public void OnAttachPlanToJob(GoapPlanJob job)
	{
		for (int i = 0; i < allNodes.Count; i++)
		{
			allNodes[i].OnAttachPlanToJob(job);
		}
	}

	public void OnUnattachPlanToJob(GoapPlanJob job)
	{
		for (int i = 0; i < allNodes.Count; i++)
		{
			allNodes[i].OnUnattachPlanToJob(job);
		}
	}

	public string LogPlan()
	{
		string text = string.Empty;
		for (int i = 0; i < allNodes.Count; i++)
		{
			JobNode jobNode = allNodes[i];
			if (i > 0)
			{
				text += "\n";
			}
			text += $"{i + 1}.";
			ActualGoapNode singleNode = jobNode.singleNode;
			text += $" ({singleNode.cost}){singleNode.action?.goapName} - Actor: {singleNode.actor?.name}, Target: {singleNode.poiTarget?.nameWithID}";
		}
		return text;
	}

	private string GetPlanSummary()
	{
		return string.Concat(GetGoalSummary() + "\nPlanned Actions are: ", LogPlan());
	}

	private string GetGoalSummary()
	{
		string text = "Goal: ";
		for (int i = 0; i < endNode.singleNode.action.baseExpectedEffects.Count; i++)
		{
			GoapEffect arg = endNode.singleNode.action.baseExpectedEffects[i];
			text += $"{arg}, ";
		}
		return text;
	}

	public void Reset()
	{
		for (int i = 0; i < allNodes.Count; i++)
		{
			if (allNodes[i] is SingleJobNode data)
			{
				ObjectPoolManager.Instance.ReturnSingleJobNodeToPool(data);
			}
		}
		RuinarchListPool<JobNode>.Release(allNodes);
		allNodes = null;
		target = null;
		startingNode = null;
		endNode = null;
		currentNode = null;
		previousNode = null;
		currentNodeIndex = 0;
		isEnd = false;
		isBeingRecalculated = false;
		doNotRecalculate = false;
		resetPlanOnFinishRecalculation = false;
	}

	public void DisconnectFromCharacter(Character p_character, out bool shouldCancelPlan)
	{
		shouldCancelPlan = false;
		if (!(currentActualNode != null))
		{
			return;
		}
		currentActualNode.DisconnectFromCharacter(p_character);
		if (currentActualNode.IsNodeObjectInvalid() || currentActualNode.IsCharacterReferenced(p_character))
		{
			shouldCancelPlan = true;
		}
		for (int i = 0; i < allNodes.Count; i++)
		{
			JobNode jobNode = allNodes[i];
			jobNode.DisconnectFromCharacter(p_character);
			if (currentActualNode.IsNodeObjectInvalid() || jobNode.IsCharacterReferenced(p_character))
			{
				shouldCancelPlan = true;
			}
		}
	}

	public void DisconnectFromStructure(LocationStructure p_structure, out bool shouldCancelPlan)
	{
		shouldCancelPlan = false;
		if (currentActualNode != null && currentActualNode.IsStructureReferenced(p_structure))
		{
			shouldCancelPlan = true;
		}
		if (shouldCancelPlan)
		{
			return;
		}
		List<JobNode> list = RuinarchListPool<JobNode>.Claim(allNodes.Count);
		list.AddRange(allNodes);
		for (int i = 0; i < list.Count; i++)
		{
			JobNode jobNode = list[i];
			if (currentNode != null && currentNode == jobNode)
			{
				continue;
			}
			bool flag = jobNode.IsStructureReferenced(p_structure);
			if (list.IsIndexInList(currentNodeIndex))
			{
				if (i < currentNodeIndex)
				{
					if (flag)
					{
						allNodes.RemoveAt(i);
						if (jobNode.singleNode.isSupposedToBeInPool)
						{
							jobNode.singleNode.ProcessReturnToPool();
						}
					}
				}
				else if (i > currentNodeIndex && flag)
				{
					shouldCancelPlan = true;
					break;
				}
			}
			else if (flag)
			{
				shouldCancelPlan = true;
				break;
			}
		}
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		currentActualNode?.CheckIfStructureIsStillReferenced(p_structure);
		for (int i = 0; i < allNodes.Count; i++)
		{
			allNodes[i].CheckIfStructureIsStillReferenced(p_structure);
		}
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		currentActualNode?.CheckIfCharacterIsStillReferenced(p_character);
		for (int i = 0; i < allNodes.Count; i++)
		{
			allNodes[i].CheckIfCharacterIsStillReferenced(p_character);
		}
	}
}
