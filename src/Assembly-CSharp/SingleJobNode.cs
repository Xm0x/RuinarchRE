public class SingleJobNode : JobNode
{
	public override ActualGoapNode singleNode => node;

	public ActualGoapNode node { get; private set; }

	public SingleJobNode()
	{
	}

	public SingleJobNode(SaveDataSingleJobNode saveDataJobNode)
		: base(saveDataJobNode)
	{
		node = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(saveDataJobNode.nodeID);
	}

	public void SetActionNode(ActualGoapNode p_action)
	{
		node = p_action;
	}

	public override void OnAttachPlanToJob(GoapPlanJob job)
	{
		node.OnAttachPlanToJob(job);
	}

	public override void OnUnattachPlanToJob(GoapPlanJob job)
	{
		node.OnUnattachPlanToJob(job);
	}

	public override void SetNextActualNode()
	{
	}

	public override bool IsCurrentActionNode(ActualGoapNode node)
	{
		return this.node == node;
	}

	public override void Reset()
	{
		if (node != null && !node.ProcessReturnToPool())
		{
			node.SetIsSupposedToBeInPool(p_state: true);
		}
		node = null;
	}
}
