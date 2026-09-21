using System.Collections.Generic;
using Traits;

public class SaveDataObsessed : SaveDataTrait
{
	public string targetCharacter;

	public List<string> sawCarriersOfTargetOfObsession;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Obsessed obsessed = trait as Obsessed;
		targetCharacter = obsessed.targetCharacter.persistentID;
		sawCarriersOfTargetOfObsession = SaveUtilities.ConvertSavableListToIDs(obsessed.sawCarriersOfTargetOfObsession);
	}
}
