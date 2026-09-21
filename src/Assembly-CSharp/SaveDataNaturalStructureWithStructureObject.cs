using BayatGames.SaveGameFree.Types;
using Inner_Maps.Location_Structures;
using UnityEngine;

public class SaveDataNaturalStructureWithStructureObject : SaveDataNaturalStructure
{
	public string structureTemplateName;

	public Vector3Save structureObjectWorldPosition;

	public override void Save(LocationStructure structure)
	{
		base.Save(structure);
		NaturalStructureWithStructureObject naturalStructureWithStructureObject = structure as NaturalStructureWithStructureObject;
		string templateName = naturalStructureWithStructureObject.templateName;
		if (naturalStructureWithStructureObject.hasBeenDestroyed)
		{
			structureTemplateName = string.Empty;
			structureObjectWorldPosition = Vector3.zero;
		}
		else
		{
			templateName = templateName.Replace("(Clone)", "");
			structureTemplateName = templateName;
			structureObjectWorldPosition = naturalStructureWithStructureObject.structureObjectWorldPos;
		}
	}
}
