using Inner_Maps.Location_Structures;

public abstract class OtherData
{
	public int actionReferenceCount { get; private set; }

	public abstract object obj { get; }

	public abstract SaveDataOtherData Save();

	public virtual void LoadAdditionalData(SaveDataOtherData data)
	{
		actionReferenceCount = data.actionReferenceCount;
	}

	public override string ToString()
	{
		if (obj != null)
		{
			return obj.ToString();
		}
		return base.ToString();
	}

	public void CheckIfStructureIsStillReferenced(LocationStructure p_structure)
	{
		IsStructureReferenced(p_structure);
	}

	public void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		IsCharacterReferenced(p_character);
	}

	public abstract void CleanUp();

	public virtual bool IsStructureReferenced(LocationStructure p_structure)
	{
		if (obj == p_structure)
		{
			return true;
		}
		return false;
	}

	public virtual bool IsCharacterReferenced(Character p_character)
	{
		if (obj == p_character)
		{
			return true;
		}
		return false;
	}

	public virtual bool IsOtherDataInvalid()
	{
		if (obj == null)
		{
			return true;
		}
		return false;
	}

	public void IncreaseReferenceCount()
	{
		actionReferenceCount++;
	}

	public void DecreaseReferenceCount()
	{
		actionReferenceCount--;
	}
}
