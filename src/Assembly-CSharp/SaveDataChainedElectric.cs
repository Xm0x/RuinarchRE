using System;
using Traits;

[Serializable]
public class SaveDataChainedElectric : SaveDataTrait
{
	public int damage;

	public bool hasInflictedDamage;

	public bool isPlayerSource;

	public override void Save(Trait trait)
	{
		base.Save(trait);
		ChainedElectric chainedElectric = trait as ChainedElectric;
		damage = chainedElectric.damage;
		hasInflictedDamage = chainedElectric.hasInflictedDamage;
		isPlayerSource = chainedElectric.isPlayerSource;
	}
}
