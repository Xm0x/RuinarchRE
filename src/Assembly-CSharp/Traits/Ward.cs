using System;

namespace Traits;

public class Ward : Trait
{
	public int stackCount;

	public override Type serializedData => typeof(SaveDataWard);

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataWard saveDataWard = saveDataTrait as SaveDataWard;
		stackCount = saveDataWard.stackCount;
	}
}
