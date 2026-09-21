using Inner_Maps.Location_Structures;
using Interrupts;

public class CrimeableOtherData : OtherData
{
	public ICrimeable crimeable { get; private set; }

	public override object obj => crimeable;

	public CrimeableOtherData(ICrimeable crimeable)
	{
		this.crimeable = crimeable;
		if (crimeable is ActualGoapNode actualGoapNode)
		{
			actualGoapNode.SetIsUsedAsCrime(p_state: true);
		}
		if (crimeable is InterruptHolder interruptHolder)
		{
			interruptHolder.SetShouldNotBeObjectPooled(state: true);
		}
	}

	public CrimeableOtherData(SaveDataCrimableOtherData data)
	{
		if (data.rumorableObjectType == OBJECT_TYPE.Action)
		{
			crimeable = DatabaseManager.Instance.actionDatabase.GetActionByPersistentIDSafe(data.rumorableID);
		}
		else if (data.rumorableObjectType == OBJECT_TYPE.Interrupt)
		{
			crimeable = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentIDSafe(data.rumorableID);
		}
	}

	public override SaveDataOtherData Save()
	{
		SaveDataCrimableOtherData saveDataCrimableOtherData = new SaveDataCrimableOtherData();
		saveDataCrimableOtherData.Save(this);
		return saveDataCrimableOtherData;
	}

	public override void CleanUp()
	{
		crimeable = null;
	}

	public override bool IsCharacterReferenced(Character p_character)
	{
		bool flag = base.IsCharacterReferenced(p_character);
		if (!flag)
		{
			flag = crimeable.IsCharacterReferenced(p_character);
		}
		return flag;
	}

	public override bool IsStructureReferenced(LocationStructure p_structure)
	{
		bool flag = base.IsStructureReferenced(p_structure);
		if (!flag)
		{
			flag = crimeable.IsStructureReferenced(p_structure);
		}
		return flag;
	}

	public override bool IsOtherDataInvalid()
	{
		bool flag = base.IsOtherDataInvalid();
		if (!flag && crimeable != null)
		{
			flag = crimeable.IsImportantDataNull();
		}
		return flag;
	}
}
