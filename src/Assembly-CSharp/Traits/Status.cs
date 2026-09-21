using System;
using System.Collections.Generic;
using UnityEngine;

namespace Traits;

[Serializable]
public class Status : Trait
{
	public bool hindersWitness;

	public bool hindersMovement;

	public bool hindersAttackTarget;

	public bool hindersPerform;

	public bool hindersSocials;

	public bool hindersFullnessRecovery;

	public bool hindersHappinessRecovery;

	public bool hindersTirednessRecovery;

	public bool isStacking;

	public int stackLimit;

	public float stackModifier;

	public bool isTangible;

	public void ApplyStackedMoodEffect(ITraitable addedTo, GameDate expiryDate, Character characterResponsible)
	{
		if (addedTo is Character character)
		{
			character.moodComponent.AddMoodEffect(Mathf.RoundToInt((float)moodEffect * stackModifier), this, expiryDate, characterResponsible);
		}
	}

	public void UnapplyStackedMoodEffect(ITraitable addedTo)
	{
		if (addedTo is Character character)
		{
			character.moodComponent.RemoveMoodEffect(-Mathf.RoundToInt((float)moodEffect * stackModifier), this);
		}
	}

	public virtual void OnStackStatus(ITraitable addedTo)
	{
	}

	public virtual void OnStackStatusAddedButStackIsAtLimit(ITraitable traitable)
	{
	}

	public virtual void OnUnstackStatus(ITraitable addedTo, bool bySchedule)
	{
	}

	public virtual void OnCopyStatus(Status statusToCopy, ITraitable from, ITraitable to)
	{
	}

	public virtual void OnRemoveStatusBySchedule(ITraitable removedFrom)
	{
	}

	public override string GetTestingData(ITraitable traitable = null)
	{
		string testingData = base.GetTestingData(traitable);
		if (traitable != null && traitable.traitContainer.stacks.ContainsKey(name))
		{
			return testingData + "\nStacks: " + traitable.traitContainer.stacks[name] + "/" + stackLimit;
		}
		return testingData;
	}

	public override string GetNameInUI(ITraitable traitable)
	{
		Dictionary<string, int> stacks = traitable.traitContainer.stacks;
		if (isStacking && stacks.ContainsKey(name))
		{
			int num = stacks[name];
			if (num > stackLimit)
			{
				num = stackLimit;
			}
			if (num > 1)
			{
				return $"{base.localizedName} (x{num})";
			}
		}
		return base.localizedName;
	}

	protected void EnablePlayerSourceChaosOrb(Character p_owner)
	{
		p_owner.traitComponent.SetWillProcessPlayerSourceChaosOrb(p_state: true);
	}

	protected void DisablePlayerSourceChaosOrb(Character p_owner)
	{
		bool flag = true;
		if (p_owner.traitContainer.HasTrait("Burning", "Frozen", "Poisoned", "Ensnared"))
		{
			Burning traitOrStatus = p_owner.traitContainer.GetTraitOrStatus<Burning>("Burning");
			if (traitOrStatus != null && traitOrStatus.isPlayerSource)
			{
				flag = false;
			}
			else
			{
				Frozen traitOrStatus2 = p_owner.traitContainer.GetTraitOrStatus<Frozen>("Frozen");
				if (traitOrStatus2 != null && traitOrStatus2.isPlayerSource)
				{
					flag = false;
				}
				else
				{
					Poisoned traitOrStatus3 = p_owner.traitContainer.GetTraitOrStatus<Poisoned>("Poisoned");
					if (traitOrStatus3 != null && traitOrStatus3.isPlayerSource)
					{
						flag = false;
					}
					else
					{
						Ensnared traitOrStatus4 = p_owner.traitContainer.GetTraitOrStatus<Ensnared>("Ensnared");
						if (traitOrStatus4 != null && traitOrStatus4.isPlayerSource)
						{
							flag = false;
						}
					}
				}
			}
		}
		if (flag)
		{
			p_owner.traitComponent.SetWillProcessPlayerSourceChaosOrb(p_state: false);
		}
	}
}
