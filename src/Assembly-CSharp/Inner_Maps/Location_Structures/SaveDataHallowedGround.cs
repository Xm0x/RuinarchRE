namespace Inner_Maps.Location_Structures;

public class SaveDataHallowedGround : SaveDataNaturalStructureWithStructureObject
{
	public RELIGION claimedByReligion;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		HallowedGround hallowedGround = locationStructure as HallowedGround;
		claimedByReligion = hallowedGround.claimedByReligion;
	}
}
