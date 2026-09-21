using BayatGames.SaveGameFree.Types;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class SaveDataDemonicStructure : SaveDataLocationStructure
{
	public string structureTemplateName;

	public Vector3Save structureObjectWorldPosition;

	public string preOccupiedBy;

	public override void Save(LocationStructure locationStructure)
	{
		base.Save(locationStructure);
		DemonicStructure demonicStructure = locationStructure as DemonicStructure;
		if (demonicStructure.preOccupiedBy != null)
		{
			preOccupiedBy = demonicStructure.preOccupiedBy.persistentID;
		}
		if (demonicStructure.hasBeenDestroyed)
		{
			structureTemplateName = string.Empty;
			structureObjectWorldPosition = Vector3.zero;
			return;
		}
		string templateName = demonicStructure.templateName;
		templateName = templateName.Replace("(Clone)", "");
		structureTemplateName = templateName;
		structureObjectWorldPosition = demonicStructure.structureObjectWorldPos;
	}
}
