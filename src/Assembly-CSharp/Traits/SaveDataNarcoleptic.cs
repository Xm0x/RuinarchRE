using System.Collections.Generic;
using UtilityScripts;

namespace Traits;

public class SaveDataNarcoleptic : SaveDataTrait
{
	public List<string> witnessedNarcolepticAttack;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		Narcoleptic narcoleptic = trait as Narcoleptic;
		witnessedNarcolepticAttack = SaveUtilities.ConvertSavableListToIDs(narcoleptic.witnessedNarcolepticAttack);
	}

	public override void CleanUp()
	{
		base.CleanUp();
		if (witnessedNarcolepticAttack != null)
		{
			RuinarchListPool<string>.Release(witnessedNarcolepticAttack);
			witnessedNarcolepticAttack = null;
		}
	}
}
