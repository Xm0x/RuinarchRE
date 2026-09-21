namespace Traits;

public static class TraitValidator
{
	public static bool CanAddTrait(ITraitable obj, Trait trait, ITraitContainer traitContainer)
	{
		if (trait.mutuallyExclusive != null)
		{
			for (int i = 0; i < trait.mutuallyExclusive.Length; i++)
			{
				if (obj.traitContainer.HasTrait(trait.mutuallyExclusive[i]))
				{
					return false;
				}
			}
		}
		bool flag = true;
		if (trait is Status status)
		{
			flag = !status.isStacking;
		}
		if (flag && obj.traitContainer.HasTrait(trait.name))
		{
			return false;
		}
		return true;
	}

	public static bool CanAddTrait(ITraitable obj, string traitName, ITraitContainer traitContainer)
	{
		Trait trait = ((!TraitManager.Instance.IsInstancedTrait(traitName)) ? TraitManager.Instance.allTraits[traitName] : TraitManager.Instance.CreateNewInstancedTraitClass<Trait>(traitName));
		return CanAddTrait(obj, trait, traitContainer);
	}

	public static bool CanAddTraitGeneric(ITraitable obj, string traitName, ITraitContainer traitContainer)
	{
		if (obj is Character { hasBeenCleanedUp: not false })
		{
			return false;
		}
		if (traitName == "Unconscious" && obj is Character { isDead: not false })
		{
			return false;
		}
		if (obj is Dragon)
		{
			if (traitName == "Zapped" || traitName == "Ensnared")
			{
				return false;
			}
			return true;
		}
		if (obj is Character character3 && character3.characterClass.className == "Barbarian" && traitName == "Zapped")
		{
			return false;
		}
		return true;
	}
}
