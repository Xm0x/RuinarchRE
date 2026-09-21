namespace Traits;

public class Diplomatic : Trait
{
	public override bool isSingleton => true;

	public Diplomatic()
	{
		name = "Diplomatic";
		description = "A typical peaceloving do-gooder. Can mend relationships.";
		type = TRAIT_TYPE.BUFF;
		effect = TRAIT_EFFECT.NEUTRAL;
		ticksDuration = 0;
		AddTraitOverrideFunctionIdentifier("See_Poi_Trait");
	}

	public override bool OnSeePOI(IPointOfInterest targetPOI, Character characterThatWillDoJob)
	{
		if (targetPOI is Character character && character.limiterComponent.canPerform && ChanceData.RollChance(CHANCE_TYPE.Diplomatic_Reduce_Conflict) && character.faction == characterThatWillDoJob.faction && character.relationshipContainer.HasAliveEnemyCharacterThatIsNot(characterThatWillDoJob) && !characterThatWillDoJob.relationshipContainer.IsEnemiesWith(character))
		{
			characterThatWillDoJob.interruptComponent.TriggerInterrupt(INTERRUPT.Reduce_Conflict, character);
		}
		return base.OnSeePOI(targetPOI, characterThatWillDoJob);
	}
}
