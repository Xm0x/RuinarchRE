using System;

namespace Traits;

public class Slayer : Trait
{
	public int stackCount;

	public override Type serializedData => typeof(SaveDataSlayer);

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataSlayer saveDataSlayer = saveDataTrait as SaveDataSlayer;
		stackCount = saveDataSlayer.stackCount;
	}
}
