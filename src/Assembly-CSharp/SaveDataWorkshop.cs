using Inner_Maps.Location_Structures;

public class SaveDataWorkshop : SaveDataManMadeStructure
{
	public SaveDataWorkShopRequestForm[] requestForms;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		Workshop workshop = locationStructure as Workshop;
		if (workshop.requests.Count > 0)
		{
			requestForms = new SaveDataWorkShopRequestForm[workshop.requests.Count];
			for (int i = 0; i < workshop.requests.Count; i++)
			{
				WorkShopRequestForm data = workshop.requests[i];
				SaveDataWorkShopRequestForm saveDataWorkShopRequestForm = new SaveDataWorkShopRequestForm();
				saveDataWorkShopRequestForm.Save(data);
				requestForms[i] = saveDataWorkShopRequestForm;
			}
		}
	}
}
