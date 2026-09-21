using System;

namespace Traits;

public abstract class RemnantTrait : Status
{
	public class SaveDataRemnantTrait : SaveDataTrait
	{
		public PLAYER_SKILL_TYPE spellUsed;

		public override void Save(Trait trait)
		{
			base.Save(trait);
			RemnantTrait remnantTrait = trait as RemnantTrait;
			spellUsed = remnantTrait.spellUsed;
		}
	}

	public PLAYER_SKILL_TYPE spellUsed { get; private set; }

	public override Type serializedData => typeof(SaveDataRemnantTrait);

	public override void LoadSecondWaveInstancedTrait(SaveDataTrait p_saveDataTrait)
	{
		base.LoadSecondWaveInstancedTrait(p_saveDataTrait);
		if (p_saveDataTrait is SaveDataRemnantTrait saveDataRemnantTrait)
		{
			spellUsed = saveDataRemnantTrait.spellUsed;
		}
	}

	public void SetSpellUsed(PLAYER_SKILL_TYPE p_spellUsed)
	{
		spellUsed = p_spellUsed;
	}
}
