using Inner_Maps.Location_Structures;

public class ActualGoapNodeOtherData : OtherData
{
	public ActualGoapNode action { get; private set; }

	public override object obj => action;

	public ActualGoapNodeOtherData(ActualGoapNode action)
	{
		this.action = action;
	}

	public ActualGoapNodeOtherData(SaveDataActualGoapNodeOtherData action)
	{
		this.action = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(action.actionID);
	}

	public override SaveDataOtherData Save()
	{
		SaveDataActualGoapNodeOtherData saveDataActualGoapNodeOtherData = new SaveDataActualGoapNodeOtherData();
		saveDataActualGoapNodeOtherData.Save(this);
		return saveDataActualGoapNodeOtherData;
	}

	public override void CleanUp()
	{
		action = null;
	}

	public override bool IsCharacterReferenced(Character p_character)
	{
		bool flag = base.IsCharacterReferenced(p_character);
		if (!flag)
		{
			flag = action.IsCharacterReferenced(p_character);
		}
		return flag;
	}

	public override bool IsStructureReferenced(LocationStructure p_structure)
	{
		bool flag = base.IsStructureReferenced(p_structure);
		if (!flag)
		{
			flag = action.IsStructureReferenced(p_structure);
		}
		return flag;
	}

	public override bool IsOtherDataInvalid()
	{
		bool flag = base.IsOtherDataInvalid();
		if (!flag && action != null)
		{
			flag = action.IsNodeObjectInvalid();
		}
		return flag;
	}
}
