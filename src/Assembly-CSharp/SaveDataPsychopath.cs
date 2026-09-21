using System.Collections.Generic;
using Traits;

public class SaveDataPsychopath : SaveDataTrait
{
	public SerialVictim victim1Requirement;

	public SerialVictim victim2Requirement;

	public string targetVictimID;

	public Dictionary<int, OpinionData> opinionCopy;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Psychopath psychopath = trait as Psychopath;
		victim1Requirement = psychopath.victim1Requirement;
		victim2Requirement = psychopath.victim2Requirement;
		targetVictimID = ((psychopath.targetVictim != null) ? psychopath.targetVictim.persistentID : string.Empty);
		opinionCopy = psychopath.opinionCopy;
	}
}
