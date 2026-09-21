using System.Collections.Generic;
using Traits;
using UtilityScripts;

public class SaveDataTraitContainer : SaveData<ITraitContainer>
{
	public List<string> nonInstancedTraits;

	public List<string> instancedTraitsIDs;

	public Dictionary<string, int> stacks;

	public Dictionary<string, List<GameDate>> scheduleTickets;

	public override void Save(ITraitContainer data)
	{
		base.Save(data);
		nonInstancedTraits = RuinarchListPool<string>.Claim();
		instancedTraitsIDs = RuinarchListPool<string>.Claim();
		for (int i = 0; i < data.traits.Count; i++)
		{
			Trait trait = data.traits[i];
			if (TraitManager.Instance.IsInstancedTrait(trait.name))
			{
				instancedTraitsIDs.Add(trait.persistentID);
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(trait);
			}
			else
			{
				nonInstancedTraits.Add(trait.name);
			}
		}
		for (int j = 0; j < data.statuses.Count; j++)
		{
			Trait trait2 = data.statuses[j];
			if (TraitManager.Instance.IsInstancedTrait(trait2.name))
			{
				instancedTraitsIDs.Add(trait2.persistentID);
				SaveManager.Instance.saveCurrentProgressManager.AddToSaveHub(trait2);
			}
			else
			{
				nonInstancedTraits.Add(trait2.name);
			}
		}
		stacks = new Dictionary<string, int>();
		foreach (KeyValuePair<string, int> stack in data.stacks)
		{
			stacks.Add(stack.Key, stack.Value);
		}
		scheduleTickets = new Dictionary<string, List<GameDate>>();
		foreach (KeyValuePair<string, List<TraitRemoveSchedule>> scheduleTicket in data.scheduleTickets)
		{
			scheduleTickets.Add(scheduleTicket.Key, new List<GameDate>());
			for (int k = 0; k < scheduleTicket.Value.Count; k++)
			{
				TraitRemoveSchedule traitRemoveSchedule = scheduleTicket.Value[k];
				scheduleTickets[scheduleTicket.Key].Add(traitRemoveSchedule.removeDate);
			}
		}
	}

	public override void CleanUp()
	{
		if (nonInstancedTraits != null)
		{
			RuinarchListPool<string>.Release(nonInstancedTraits);
			nonInstancedTraits = null;
		}
		if (instancedTraitsIDs != null)
		{
			RuinarchListPool<string>.Release(instancedTraitsIDs);
			instancedTraitsIDs = null;
		}
		if (stacks != null)
		{
			stacks.Clear();
			stacks = null;
		}
		if (scheduleTickets != null)
		{
			scheduleTickets.Clear();
			scheduleTickets = null;
		}
	}
}
