using System.Collections.Generic;
using UtilityScripts;

public class SaveDataBaseRelationshipContainer : SaveData<BaseRelationshipContainer>
{
	public Dictionary<int, IRelationshipData> relationships;

	public Dictionary<int, List<string>> sharedOpinionModifiers;

	public List<string> charactersWithOpinion;

	public override void Save(BaseRelationshipContainer data)
	{
		base.Save(data);
		relationships = new Dictionary<int, IRelationshipData>(data.relationships);
		charactersWithOpinion = SaveUtilities.ConvertSavableListToIDs(data.charactersWithOpinion);
		sharedOpinionModifiers = new Dictionary<int, List<string>>();
		foreach (KeyValuePair<int, IRelationshipData> relationship in data.relationships)
		{
			List<string> list = new List<string>();
			for (int i = 0; i < relationship.Value.opinions.sharedOpinions.Count; i++)
			{
				SharedOpinionModifier sharedOpinionModifier = relationship.Value.opinions.sharedOpinions[i];
				list.Add(sharedOpinionModifier.persistentID);
			}
			sharedOpinionModifiers.Add(relationship.Key, list);
		}
	}

	public override BaseRelationshipContainer Load()
	{
		return new BaseRelationshipContainer(this);
	}

	public override void CleanUp()
	{
		if (charactersWithOpinion != null)
		{
			RuinarchListPool<string>.Release(charactersWithOpinion);
			charactersWithOpinion = null;
		}
		relationships?.Clear();
		relationships = null;
		sharedOpinionModifiers?.Clear();
		sharedOpinionModifiers = null;
	}
}
