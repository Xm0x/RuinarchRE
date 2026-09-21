using System;

namespace Traits;

public class Malnourished : Status
{
	private Character owner;

	private readonly int deathDuration;

	private int _currentDeathDuration;

	public int currentDeathDuration => _currentDeathDuration;

	public override Type serializedData => typeof(SaveDataMalnourished);

	public Malnourished()
	{
		name = "Malnourished";
		description = "Has not eaten for a very long time.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = 0;
		moodEffect = -10;
		deathDuration = GameManager.Instance.GetTicksBasedOnHour(72);
		AddTraitOverrideFunctionIdentifier("Tick_Started_Trait");
	}

	public override void LoadFirstWaveInstancedTrait(SaveDataTrait saveDataTrait)
	{
		base.LoadFirstWaveInstancedTrait(saveDataTrait);
		SaveDataMalnourished saveDataMalnourished = saveDataTrait as SaveDataMalnourished;
		_currentDeathDuration = saveDataMalnourished.currentDeathDuration;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		owner = addTo as Character;
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		owner = addedTo as Character;
		_currentDeathDuration = 0;
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		owner = null;
	}

	public override void OnTickStarted(ITraitable traitable)
	{
		base.OnTickStarted(traitable);
		CheckDeath(owner);
	}

	public override void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
		base.OnCopyStatus(statusToCopy, from, to);
		if (statusToCopy is Malnourished malnourished)
		{
			_currentDeathDuration = malnourished.currentDeathDuration;
		}
	}

	private void CheckDeath(Character owner)
	{
		_currentDeathDuration = currentDeathDuration + 1;
		if (currentDeathDuration >= deathDuration)
		{
			owner.Death("starvation");
		}
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = owner;
	}
}
