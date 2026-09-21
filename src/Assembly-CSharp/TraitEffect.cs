using System;
using System.Collections.Generic;

[Serializable]
public class TraitEffect
{
	public STAT stat;

	public float amount;

	public bool isPercentage;

	public TRAIT_REQUIREMENT_CHECKER checker;

	public TRAIT_REQUIREMENT_TARGET target;

	public DAMAGE_IDENTIFIER damageIdentifier;

	public string description;

	public bool hasRequirement;

	public bool isNot;

	public TRAIT_REQUIREMENT requirementType;

	public TRAIT_REQUIREMENT_SEPARATOR requirementSeparator;

	public List<string> requirements;
}
