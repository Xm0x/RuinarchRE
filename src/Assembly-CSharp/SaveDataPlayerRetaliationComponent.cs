using System.Collections.Generic;
using UtilityScripts;

public class SaveDataPlayerRetaliationComponent : SaveData<PlayerRetaliationComponent>
{
	public int retaliationCounter;

	public int retaliatorCount;

	public int destroyedStructuresByRetaliatorCounter;

	public List<string> spawnedRetaliators;

	public bool isRetaliating;

	public RuinarchBasicProgress retaliationProgress;

	public RACE retaliatorRace;

	public override void Save(PlayerRetaliationComponent data)
	{
		base.Save(data);
		retaliationCounter = data.retaliationCounter;
		retaliatorCount = data.retaliatorCount;
		destroyedStructuresByRetaliatorCounter = data.destroyedStructuresByRetaliatorCounter;
		spawnedRetaliators = SaveUtilities.ConvertSavableListToIDs(data.spawnedRetaliators);
		isRetaliating = data.isRetaliating;
		retaliationProgress = data.retaliationProgress;
		retaliatorRace = data.retaliatorRace;
	}

	public override PlayerRetaliationComponent Load()
	{
		return new PlayerRetaliationComponent(this);
	}
}
