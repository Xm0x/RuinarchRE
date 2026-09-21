using UnityEngine;

namespace Traits;

public class Hiding : Status
{
	private Character _owner;

	public Hiding()
	{
		name = "Hiding";
		description = "This character hiding from something.";
		type = TRAIT_TYPE.STATUS;
		effect = TRAIT_EFFECT.NEGATIVE;
		ticksDuration = GameManager.Instance.GetTicksBasedOnHour(12);
		isHidden = true;
	}

	public override void LoadTraitOnLoadTraitContainer(ITraitable addTo)
	{
		base.LoadTraitOnLoadTraitContainer(addTo);
		if (addTo is Character owner)
		{
			_owner = owner;
			StartCheckingForCowering();
		}
	}

	public override void OnAddTrait(ITraitable addedTo)
	{
		base.OnAddTrait(addedTo);
		if (addedTo is Character character)
		{
			_owner = character;
			_owner.jobQueue.CancelAllJobs();
			character.behaviourComponent.AddBehaviourComponent(typeof(DesiresIsolationBehaviour));
			StartCheckingForCowering();
		}
	}

	public override void OnRemoveTrait(ITraitable removedFrom, Character removedBy)
	{
		base.OnRemoveTrait(removedFrom, removedBy);
		if (removedFrom is Character character)
		{
			StopCheckingForCowering();
			character.behaviourComponent.RemoveBehaviourComponent(typeof(DesiresIsolationBehaviour));
			character.needsComponent.CheckExtremeNeeds();
			_owner = null;
		}
	}

	private void StartCheckingForCowering()
	{
		Messenger.AddListener(Signals.HOUR_STARTED, CoweringCheck);
	}

	private void CoweringCheck()
	{
		if (_owner.limiterComponent.canPerform)
		{
			int num = Random.Range(0, 100);
			int num2 = 50;
			if (num < num2)
			{
				_owner.interruptComponent.TriggerInterrupt(INTERRUPT.Cowering, _owner, "", null, "Got_Scared");
			}
		}
	}

	private void StopCheckingForCowering()
	{
		Messenger.RemoveListener(Signals.HOUR_STARTED, CoweringCheck);
	}

	public override void CheckIfCharacterIsStillReferenced(Character p_character)
	{
		base.CheckIfCharacterIsStillReferenced(p_character);
		_ = _owner;
	}
}
