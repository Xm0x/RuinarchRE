using Inner_Maps.Location_Structures;
using Interrupts;

public class RumorOtherData : OtherData
{
	public Rumor rumor { get; private set; }

	public override object obj => rumor;

	public RumorOtherData(Rumor rumor)
	{
		this.rumor = rumor;
	}

	public RumorOtherData(SaveDataRumorOtherData data)
	{
	}

	public override void LoadAdditionalData(SaveDataOtherData data)
	{
		base.LoadAdditionalData(data);
		SaveDataRumorOtherData saveDataRumorOtherData = data as SaveDataRumorOtherData;
		if (saveDataRumorOtherData.rumorableObjectType == OBJECT_TYPE.Action)
		{
			ActualGoapNode actionByPersistentID = DatabaseManager.Instance.actionDatabase.GetActionByPersistentID(saveDataRumorOtherData.rumorableID);
			rumor = actionByPersistentID.rumor;
		}
		else if (saveDataRumorOtherData.rumorableObjectType == OBJECT_TYPE.Interrupt)
		{
			InterruptHolder interruptByPersistentID = DatabaseManager.Instance.interruptDatabase.GetInterruptByPersistentID(saveDataRumorOtherData.rumorableID);
			rumor = interruptByPersistentID.rumor;
		}
	}

	public override SaveDataOtherData Save()
	{
		SaveDataRumorOtherData saveDataRumorOtherData = new SaveDataRumorOtherData();
		saveDataRumorOtherData.Save(this);
		return saveDataRumorOtherData;
	}

	public override bool IsCharacterReferenced(Character p_character)
	{
		bool flag = base.IsCharacterReferenced(p_character);
		if (!flag)
		{
			flag = rumor.IsCharacterReferenced(p_character);
		}
		return flag;
	}

	public override bool IsStructureReferenced(LocationStructure p_structure)
	{
		bool flag = base.IsStructureReferenced(p_structure);
		if (!flag)
		{
			flag = rumor.IsStructureReferenced(p_structure);
		}
		return flag;
	}

	public override bool IsOtherDataInvalid()
	{
		bool flag = base.IsOtherDataInvalid();
		if (!flag && rumor != null)
		{
			flag = rumor.characterThatCreatedRumor == null || rumor.targetCharacter == null || rumor.rumorable == null || rumor.rumorable.IsImportantDataNull();
		}
		return flag;
	}

	public override void CleanUp()
	{
		rumor = null;
	}
}
