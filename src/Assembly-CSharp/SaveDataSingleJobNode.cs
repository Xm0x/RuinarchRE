public class SaveDataSingleJobNode : SaveDataJobNode
{
	public string nodeID;

	public override void Save(JobNode data)
	{
		base.Save(data);
		SingleJobNode singleJobNode = data as SingleJobNode;
		nodeID = singleJobNode.node.persistentID;
		SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(singleJobNode.node);
	}

	public override JobNode Load()
	{
		return new SingleJobNode(this);
	}
}
