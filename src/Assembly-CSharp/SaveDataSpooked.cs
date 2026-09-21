using System.Collections.Generic;
using Traits;

public class SaveDataSpooked : SaveDataTrait
{
	public List<string> sourceOfFearIDs;

	public List<PLAYER_SKILL_TYPE> sourceOfFearSpells;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Spooked spooked = trait as Spooked;
		sourceOfFearIDs = new List<string>(spooked.sourceOfFearIDs);
		sourceOfFearSpells = new List<PLAYER_SKILL_TYPE>(spooked.sourceOfFearSpells);
	}
}
