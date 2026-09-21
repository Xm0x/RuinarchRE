namespace Traits;

public class Melting : Status
{
	public override bool isSingleton => true;

	public Melting()
	{
		name = "Melting";
		description = "This is melting.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		PerTick(traitable);
	}

	private void PerTick(ITraitable traitable)
	{
		if (traitable.gridTileLocation != null)
		{
			traitable.AdjustHP(-20, ELEMENTAL_TYPE.Normal, triggerDeath: true, null, null, showHPBar: true);
		}
	}
}
